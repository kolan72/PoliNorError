using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Executes retry sessions handed over by <see cref="DeferredRetry"/>'s <c>StartAsync</c> methods:
	/// every accepted session is stepped sequentially by a single internal worker with ready-time based scheduling, i.e. a session
	/// with a later <see cref="TimeSpan"/> delay never blocks a session that is already due.
	/// </summary>
	/// <remarks>
	/// The queue guarantees that every accepted session reaches a terminal state: success, definitive failure,
	/// cancellation or shutdown via <see cref="CompleteAsync"/>/<see cref="CancelAsync"/>.
	/// Completing and canceling the queue are idempotent; after the queue has stopped accepting sessions,
	/// new enqueueing attempts throw a <see cref="RetryQueueRejectedException"/>.
	/// </remarks>
	public sealed class RetryQueue
	{
		private const string NOT_ACCEPTING_MSG = "The retry queue is completed or canceled and does not accept new retry sessions.";
		private const string CAPACITY_MSG = "The retry queue capacity limit is reached.";

		private static readonly TimeSpan _maxWaitDelay = TimeSpan.FromMilliseconds(int.MaxValue);

		private readonly object _sync = new object();
		private readonly List<QueueEntry> _heap = new List<QueueEntry>();
		private readonly List<QueueEntry> _canceling = new List<QueueEntry>();
		private readonly List<QueueEntry> _accepted = new List<QueueEntry>();
		private readonly SemaphoreSlim _slots;
		private readonly IRetryQueueReadyWait _readyWait;
		private readonly TaskCompletionSource<bool> _stoppedTcs =
			new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		private Task _pumpTask;
		private long _sequence;
		private bool _acceptStopped;
		private bool _cancelRequested;

		/// <summary>
		/// Initializes an unbounded queue with the default overflow policy.
		/// </summary>
		public RetryQueue() : this(null, null) {}

		/// <summary>
		/// Initializes a queue with the specified options.
		/// </summary>
		/// <param name="options">Capacity and overflow settings. <c>null</c> means the defaults.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="RetryQueueOptions.Capacity"/> is negative.</exception>
		public RetryQueue(RetryQueueOptions options) : this(options, null) {}

		internal RetryQueue(RetryQueueOptions options, IRetryQueueReadyWait readyWait)
		{
			var opts = options ?? new RetryQueueOptions();
			if (opts.Capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(options), "Capacity must be non-negative.");

			Capacity = opts.Capacity;
			Overflow = opts.Overflow;
			_slots = Capacity > 0 ? new SemaphoreSlim(Capacity, Capacity) : null;
			_readyWait = readyWait ?? new RetryQueueReadyWait();
		}

		/// <summary>
		/// Gets the maximum number of sessions the queue holds at the same time. Zero means the queue is unbounded.
		/// </summary>
		public int Capacity { get; }

		/// <summary>
		/// Gets the behavior used when <see cref="Capacity"/> is reached and greater than zero.
		/// </summary>
		public RetryQueueOverflowPolicy Overflow { get; }

		/// <summary>
		/// Gets the number of accepted sessions that have not reached a terminal state yet.
		/// </summary>
		public int PendingCount
		{
			get
			{
				lock (_sync)
				{
					return _accepted.Count;
				}
			}
		}

		/// <summary>
		/// Stops accepting new sessions and lets every already accepted session run to its terminal state,
		/// then waits until the internal worker exits.
		/// </summary>
		/// <remarks>Session attempts that are waiting for a free capacity slot are rejected with a <see cref="RetryQueueRejectedException"/>.</remarks>
		public async Task CompleteAsync()
		{
			Task pump;
			lock (_sync)
			{
				_acceptStopped = true;
				pump = _pumpTask;
			}
			_stoppedTcs.TrySetResult(true);
			_readyWait.Signal();

			if (pump != null)
			{
				await pump.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Stops accepting new sessions, aborts every pending and in-flight session
		/// (their handles resolve with the canceled <c>PolicyResult</c> shape) and waits until the internal worker exits.
		/// </summary>
		/// <remarks>Session attempts that are waiting for a free capacity slot are rejected with a <see cref="RetryQueueRejectedException"/>.</remarks>
		public async Task CancelAsync()
		{
			Task pump;
			List<QueueEntry> accepted;
			lock (_sync)
			{
				_acceptStopped = true;
				_cancelRequested = true;
				pump = _pumpTask;
				accepted = new List<QueueEntry>(_accepted);
				while (_heap.Count > 0)
				{
					var entry = PopHeap();
					_canceling.Add(entry);
				}
			}
			_stoppedTcs.TrySetResult(true);
			_readyWait.Signal();

			foreach (var entry in accepted)
			{
				entry.Session.CancelSession();
			}

			if (pump != null)
			{
				await pump.ConfigureAwait(false);
			}
		}

		internal async Task EnqueueAsync(RetrySessionBase session, DateTime readyTimeUtc, CancellationToken token)
		{
			ThrowIfNotAccepting();

			if (_slots != null)
			{
				if (Overflow == RetryQueueOverflowPolicy.Reject)
				{
					if (!await _slots.WaitAsync(0, token).ConfigureAwait(false))
					{
						throw new RetryQueueRejectedException(CAPACITY_MSG);
					}
				}
				else
				{
					await WaitSlotAsync(token).ConfigureAwait(false);
				}
			}

			var entry = new QueueEntry(session, readyTimeUtc);
			var accepted = false;
			lock (_sync)
			{
				if (!_acceptStopped)
				{
					entry.Sequence = _sequence++;
					InsertHeap(entry);
					_accepted.Add(entry);
					entry.CancellationRegistration = session.Token.Register(OnSessionCanceling, entry);
					EnsurePumpStarted();
					accepted = true;
				}
			}

			if (!accepted)
			{
				_slots?.Release();
				throw new RetryQueueRejectedException(NOT_ACCEPTING_MSG);
			}

			_readyWait.Signal();
		}

		internal static DateTime GetReadyTimeUtc(TimeSpan? delay) => AddDelay(DateTime.UtcNow, delay ?? TimeSpan.Zero);

		internal static DateTime AddDelay(DateTime utcNow, TimeSpan delay)
		{
			if (delay <= TimeSpan.Zero)
			{
				return utcNow;
			}
			if (delay > _maxWaitDelay)
			{
				delay = _maxWaitDelay;
			}
			return utcNow + delay;
		}

		private async Task WaitSlotAsync(CancellationToken token)
		{
			var slotTask = _slots.WaitAsync(Timeout.Infinite, token);
			var winner = await Task.WhenAny(slotTask, _stoppedTcs.Task).ConfigureAwait(false);
			if (winner == _stoppedTcs.Task)
			{
				if (await slotTask.ConfigureAwait(false))
				{
					_slots.Release();
				}
				throw new RetryQueueRejectedException(NOT_ACCEPTING_MSG);
			}
			await slotTask.ConfigureAwait(false);
		}

		private void ThrowIfNotAccepting()
		{
			lock (_sync)
			{
				if (_acceptStopped)
				{
					throw new RetryQueueRejectedException(NOT_ACCEPTING_MSG);
				}
			}
		}

		private void EnsurePumpStarted()
		{
			if (_pumpTask == null)
			{
				_pumpTask = Task.Run(PumpAsync);
			}
		}

		private async Task PumpAsync()
		{
			while (true)
			{
				var entry = default(QueueEntry);
				var kind = PumpWorkKind.None;
				var waitDelay = Timeout.InfiniteTimeSpan;
				var exit = false;

				lock (_sync)
				{
					if (_canceling.Count > 0)
					{
						entry = _canceling[0];
						_canceling.RemoveAt(0);
						kind = PumpWorkKind.Canceled;
					}
					else
					{
						var now = DateTime.UtcNow;
						QueueEntry due = null;
						if (_heap.Count > 0 && _heap[0].ReadyTimeUtc <= now)
						{
							due = PopHeap();
						}

						if (due != null)
						{
							entry = due;
							if (_cancelRequested || due.Session.Token.IsCancellationRequested)
							{
								kind = PumpWorkKind.Canceled;
							}
							else if (due.Session.Result.IsFailed)
							{
								kind = PumpWorkKind.TerminalFailure;
							}
							else
							{
								kind = PumpWorkKind.Step;
							}
						}
						else
						{
							exit = _acceptStopped && _accepted.Count == 0;
							if (!exit && _heap.Count > 0)
							{
								waitDelay = _heap[0].ReadyTimeUtc - now;
								if (waitDelay < TimeSpan.Zero)
								{
									waitDelay = TimeSpan.Zero;
								}
								else if (waitDelay > _maxWaitDelay)
								{
									waitDelay = _maxWaitDelay;
								}
							}
						}
					}
				}

				if (exit)
				{
					return;
				}

				if (entry != null)
				{
					await ProcessAsync(entry, kind).ConfigureAwait(false);
					continue;
				}

				await _readyWait.WaitAsync(waitDelay, CancellationToken.None).ConfigureAwait(false);
			}
		}

		private async Task ProcessAsync(QueueEntry entry, PumpWorkKind kind)
		{
			var session = entry.Session;
			switch (kind)
			{
				case PumpWorkKind.Canceled:
					session.ApplyCancellation();
					await FinalizeAsync(entry).ConfigureAwait(false);
					return;
				case PumpWorkKind.TerminalFailure:
					await FinalizeAsync(entry).ConfigureAwait(false);
					return;
			}

			RetryStepOutcome outcome;
			try
			{
				session.IncrementCount();
				outcome = await session.StepAsync(session.ConfigureAwait, session.Token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				session.OnWorkerFault(ex);
				outcome = RetryStepOutcome.Terminal;
			}

			if (outcome.IsTerminal)
			{
				await FinalizeAsync(entry).ConfigureAwait(false);
				return;
			}

			bool cancelNow;
			lock (_sync)
			{
				cancelNow = _cancelRequested || session.Token.IsCancellationRequested;
			}

			if (cancelNow)
			{
				session.ApplyCancellation();
				await FinalizeAsync(entry).ConfigureAwait(false);
				return;
			}

			lock (_sync)
			{
				entry.ReadyTimeUtc = AddDelay(DateTime.UtcNow, outcome.Delay ?? TimeSpan.Zero);
				InsertHeap(entry);
			}
			_readyWait.Signal();
		}

		private async Task FinalizeAsync(QueueEntry entry)
		{
			entry.CancellationRegistration.Dispose();
			try
			{
				await entry.Session.RunTerminalHandlerAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				entry.Session.OnWorkerFault(ex);
			}
			finally
			{
				entry.Session.DisposeCancellation();
				lock (_sync)
				{
					var removed = _accepted.Remove(entry);
					if (entry.HeapIndex >= 0)
					{
						RemoveFromHeap(entry.HeapIndex);
					}
					if (removed && _slots != null)
					{
						_slots.Release();
					}
				}
			}
		}

		private void OnSessionCanceling(object state)
		{
			var entry = (QueueEntry)state;
			var signal = false;
			lock (_sync)
			{
				if (entry.HeapIndex >= 0)
				{
					RemoveFromHeap(entry.HeapIndex);
					_canceling.Add(entry);
					signal = true;
				}
			}
			if (signal)
			{
				_readyWait.Signal();
			}
		}

		private void InsertHeap(QueueEntry entry)
		{
			_heap.Add(entry);
			entry.HeapIndex = _heap.Count - 1;
			SiftUp(entry.HeapIndex);
		}

		private QueueEntry PopHeap()
		{
			var top = _heap[0];
			RemoveFromHeap(0);
			return top;
		}

		private void RemoveFromHeap(int index)
		{
			var removed = _heap[index];
			var last = _heap[_heap.Count - 1];
			_heap.RemoveAt(_heap.Count - 1);
			removed.HeapIndex = -1;
			if (index < _heap.Count)
			{
				_heap[index] = last;
				last.HeapIndex = index;
				SiftDown(index);
				SiftUp(index);
			}
		}

		private void SiftUp(int index)
		{
			while (index > 0)
			{
				var parent = (index - 1) / 2;
				if (!IsBefore(_heap[index], _heap[parent]))
				{
					return;
				}
				Swap(index, parent);
				index = parent;
			}
		}

		private void SiftDown(int index)
		{
			var count = _heap.Count;
			while (true)
			{
				var left = index * 2 + 1;
				if (left >= count)
				{
					return;
				}
				var right = left + 1;
				var child = right < count && IsBefore(_heap[right], _heap[left]) ? right : left;
				if (!IsBefore(_heap[child], _heap[index]))
				{
					return;
				}
				Swap(index, child);
				index = child;
			}
		}

		private void Swap(int a, int b)
		{
			var tmp = _heap[a];
			_heap[a] = _heap[b];
			_heap[b] = tmp;
			_heap[a].HeapIndex = a;
			_heap[b].HeapIndex = b;
		}

		private static bool IsBefore(QueueEntry a, QueueEntry b)
		{
			var cmp = a.ReadyTimeUtc.CompareTo(b.ReadyTimeUtc);
			return cmp != 0 ? cmp < 0 : a.Sequence < b.Sequence;
		}

		private enum PumpWorkKind
		{
			None = 0,
			Step,
			Canceled,
			TerminalFailure
		}

		private sealed class QueueEntry
		{
			public QueueEntry(RetrySessionBase session, DateTime readyTimeUtc)
			{
				Session = session;
				ReadyTimeUtc = readyTimeUtc;
				HeapIndex = -1;
			}

			public RetrySessionBase Session { get; }
			public DateTime ReadyTimeUtc { get; set; }
			public long Sequence { get; set; }
			public int HeapIndex { get; set; }
			public CancellationTokenRegistration CancellationRegistration { get; set; }
		}
	}

	internal interface IRetryQueueReadyWait
	{
		Task WaitAsync(TimeSpan delay, CancellationToken token);

		void Signal();
	}

	internal sealed class RetryQueueReadyWait : IRetryQueueReadyWait
	{
		private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

		public Task WaitAsync(TimeSpan delay, CancellationToken token) => _signal.WaitAsync(delay, token);

		public void Signal() => _signal.Release();
	}
}