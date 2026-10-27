using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class RecordingReadyWait : IRetryQueueReadyWait
	{
		private readonly RetryQueueReadyWait _inner = new RetryQueueReadyWait();
		private readonly List<TimeSpan> _waits = new List<TimeSpan>();

		public IReadOnlyList<TimeSpan> Waits
		{
			get
			{
				lock (_waits)
				{
					return _waits.ToArray();
				}
			}
		}

		public Task WaitAsync(TimeSpan delay, CancellationToken token)
		{
			lock (_waits)
			{
				_waits.Add(delay);
			}
			return _inner.WaitAsync(delay, token);
		}

		public void Signal() => _inner.Signal();
	}

	internal class RetryQueueTests
	{
		private DefaultRetryProcessor _processor;

		[SetUp]
		public void SetUp()
		{
			_processor = new DefaultRetryProcessor();
		}

		private sealed class TestQueuedSession
		{
			public RetrySession Session { get; set; }
			public TaskCompletionSource<bool> Terminal { get; set; }
		}

		private TestQueuedSession NewSession(Func<CancellationToken, Task> func = null)
		{
			var terminal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var session = RetrySession.Create(
				func ?? (async _ => await Task.Yield()),
				_processor,
				RetryCountInfo.Limited(5),
				null,
				DefaultRetryProcessor.RetryErrorContextCreator,
				false,
				default,
				true);
			session.TerminalHandler = () =>
			{
				terminal.TrySetResult(true);
				return Task.CompletedTask;
			};
			return new TestQueuedSession { Session = session, Terminal = terminal };
		}

		private static Func<CancellationToken, Task> CreatePausableFunc(Task gate, Action entered = null)
		{
			return async token =>
			{
				entered?.Invoke();
				if (gate.IsCompleted)
				{
					return;
				}
				var cancellableDelay = Task.Delay(Timeout.Infinite, token);
				var winner = await Task.WhenAny(gate, cancellableDelay);
				if (winner != gate)
				{
					token.ThrowIfCancellationRequested();
				}
			};
		}

		[Test]
		public async Task Should_ReadyTimeOrdering_WinOverEnqueueOrder()
		{
			var queue = new RetryQueue();
			var order = new List<string>();

			TestQueuedSession NewOrdered(string name) =>
				NewSession(async _ =>
				{
					lock (order)
					{
						order.Add(name);
					}
					await Task.Yield();
				});

			var first = NewOrdered("first");
			var second = NewOrdered("second");
			var third = NewOrdered("third");

			await queue.EnqueueAsync(first.Session, DateTime.UtcNow + TimeSpan.FromMilliseconds(80), first.Session.Token);
			await queue.EnqueueAsync(second.Session, DateTime.UtcNow + TimeSpan.FromMilliseconds(10), second.Session.Token);
			await queue.EnqueueAsync(third.Session, DateTime.UtcNow + TimeSpan.FromMilliseconds(40), third.Session.Token);

			ClassicAssert.AreEqual(3, queue.PendingCount);

			await DeferredRetryTestHelpers.Complete(first.Terminal.Task);
			await DeferredRetryTestHelpers.Complete(second.Terminal.Task);
			await DeferredRetryTestHelpers.Complete(third.Terminal.Task);

			lock (order)
			{
				ClassicAssert.AreEqual(new[] { "second", "third", "first" }, order.ToArray());
			}

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
			ClassicAssert.AreEqual(0, queue.PendingCount);
		}

		[Test]
		public async Task Should_CapacityWait_ParkEnqueue_UntilSessionFinalizes()
		{
			var queue = new RetryQueue(new RetryQueueOptions { Capacity = 1, Overflow = RetryQueueOverflowPolicy.Wait });
			var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			var blocking = NewSession(CreatePausableFunc(gate.Task, () => entered.TrySetResult(true)));
			var queued = NewSession();

			await queue.EnqueueAsync(blocking.Session, DateTime.UtcNow, blocking.Session.Token);
			await DeferredRetryTestHelpers.Complete(entered.Task);

			var parked = queue.EnqueueAsync(queued.Session, DateTime.UtcNow, queued.Session.Token);
			await Task.Delay(20);

			ClassicAssert.IsFalse(parked.IsCompleted);
			ClassicAssert.AreEqual(1, queue.PendingCount);

			gate.SetResult(true);

			await DeferredRetryTestHelpers.Complete(parked);
			ClassicAssert.AreEqual(1, queue.PendingCount);

			await DeferredRetryTestHelpers.Complete(blocking.Terminal.Task);
			await DeferredRetryTestHelpers.Complete(queued.Terminal.Task);
			await DeferredRetryTestHelpers.WaitUntil(() => queue.PendingCount == 0);
			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
		}

		[Test]
		public async Task Should_CapacityReject_Throw_WhenSessionIsParked()
		{
			var queue = new RetryQueue(new RetryQueueOptions { Capacity = 1, Overflow = RetryQueueOverflowPolicy.Reject });
			var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			var blocking = NewSession(CreatePausableFunc(gate.Task, () => entered.TrySetResult(true)));
			var rejected = NewSession();

			await queue.EnqueueAsync(blocking.Session, DateTime.UtcNow, blocking.Session.Token);
			await DeferredRetryTestHelpers.Complete(entered.Task);

			var exception = Assert.ThrowsAsync<RetryQueueRejectedException>(
				async () => await queue.EnqueueAsync(rejected.Session, DateTime.UtcNow, rejected.Session.Token));
			ClassicAssert.IsNotNull(exception);
			ClassicAssert.AreEqual(0, rejected.Session.Result.Errors.Count());

			gate.SetResult(true);
			await DeferredRetryTestHelpers.Complete(blocking.Terminal.Task);
			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
			ClassicAssert.AreEqual(0, queue.PendingCount);
		}

		[Test]
		public async Task Should_CompleteAsync_DrainAllAcceptedSessions_AndRejectNewOnes()
		{
			var queue = new RetryQueue();
			var order = new List<string>();

			TestQueuedSession NewOrdered(string name) =>
				NewSession(async _ =>
				{
					lock (order)
					{
						order.Add(name);
					}
					await Task.Yield();
				});

			var slow = NewOrdered("slow");
			var fast = NewOrdered("fast");

			await queue.EnqueueAsync(slow.Session, DateTime.UtcNow + TimeSpan.FromMilliseconds(60), slow.Session.Token);
			await queue.EnqueueAsync(fast.Session, DateTime.UtcNow + TimeSpan.FromMilliseconds(20), fast.Session.Token);

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());

			ClassicAssert.AreEqual(0, queue.PendingCount);
			await DeferredRetryTestHelpers.Complete(slow.Terminal.Task);
			await DeferredRetryTestHelpers.Complete(fast.Terminal.Task);
			lock (order)
			{
				ClassicAssert.AreEqual(new[] { "fast", "slow" }, order.ToArray());
			}

			var afterCompletion = NewSession();
			var exception = Assert.ThrowsAsync<RetryQueueRejectedException>(
				async () => await queue.EnqueueAsync(afterCompletion.Session, DateTime.UtcNow, afterCompletion.Session.Token));
			ClassicAssert.IsNotNull(exception);
		}

		[Test]
		public async Task Should_CancelAsync_ResolveSleepingSession_AsFailedAndCanceled()
		{
			var queue = new RetryQueue();
			var sleeping = NewSession();

			await queue.EnqueueAsync(sleeping.Session, DateTime.UtcNow + TimeSpan.FromSeconds(30), sleeping.Session.Token);
			ClassicAssert.AreEqual(1, queue.PendingCount);

			await DeferredRetryTestHelpers.Complete(queue.CancelAsync());
			await DeferredRetryTestHelpers.Complete(sleeping.Terminal.Task);

			ClassicAssert.AreEqual(0, queue.PendingCount);
			ClassicAssert.IsTrue(sleeping.Session.Result.IsCanceled);
			ClassicAssert.IsTrue(sleeping.Session.Result.IsFailed);
			ClassicAssert.IsNotNull(sleeping.Session.Result.PolicyCanceledError);
		}

		[Test]
		public async Task Should_CancelAsync_ResolveInFlightSession_AsFailedAndCanceled()
		{
			var queue = new RetryQueue();
			var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var inFlow = NewSession(CreatePausableFunc(new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously).Task, () => entered.TrySetResult(true)));

			await queue.EnqueueAsync(inFlow.Session, DateTime.UtcNow, inFlow.Session.Token);
			await DeferredRetryTestHelpers.Complete(entered.Task);

			await DeferredRetryTestHelpers.Complete(queue.CancelAsync());
			await DeferredRetryTestHelpers.Complete(inFlow.Terminal.Task);

			ClassicAssert.AreEqual(0, queue.PendingCount);
			ClassicAssert.IsTrue(inFlow.Session.Result.IsCanceled);
			ClassicAssert.IsTrue(inFlow.Session.Result.IsFailed);
			ClassicAssert.IsNotNull(inFlow.Session.Result.PolicyCanceledError);
		}

		[Test]
		public async Task Should_CancelAsync_RejectParkedEnqueue_AndResolveInFlightSession()
		{
			var queue = new RetryQueue(new RetryQueueOptions { Capacity = 1, Overflow = RetryQueueOverflowPolicy.Wait });
			var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			var blocking = NewSession(CreatePausableFunc(gate.Task, () => entered.TrySetResult(true)));
			var parkedSession = NewSession();

			await queue.EnqueueAsync(blocking.Session, DateTime.UtcNow, blocking.Session.Token);
			await DeferredRetryTestHelpers.Complete(entered.Task);

			var parked = queue.EnqueueAsync(parkedSession.Session, DateTime.UtcNow, parkedSession.Session.Token);
			await Task.Delay(20);
			ClassicAssert.IsFalse(parked.IsCompleted);

			await DeferredRetryTestHelpers.Complete(queue.CancelAsync());

			var exception = Assert.ThrowsAsync<RetryQueueRejectedException>(async () => await parked);
			ClassicAssert.IsNotNull(exception);

			await DeferredRetryTestHelpers.Complete(blocking.Terminal.Task);
			ClassicAssert.AreEqual(0, queue.PendingCount);
		}

		[Test]
		public async Task Should_WorkerFault_TerminalHandler_ResolveSessionAsFailedWithRetryQueueWorkerCatchBlock()
		{
			var queue = new RetryQueue();
			var session = RetrySession.Create(
				async _ => await Task.Yield(),
				_processor,
				RetryCountInfo.Limited(1),
				null,
				DefaultRetryProcessor.RetryErrorContextCreator,
				false,
				default,
				true);
			session.TerminalHandler = () => throw new InvalidOperationException("terminal handler failed");

			await queue.EnqueueAsync(session, DateTime.UtcNow, session.Token);
			await DeferredRetryTestHelpers.WaitUntil(() => queue.PendingCount == 0);

			ClassicAssert.IsTrue(session.Result.IsFailed);
			var catchBlockError = session.Result.CatchBlockErrors.FirstOrDefault();
			ClassicAssert.IsNotNull(catchBlockError);
			ClassicAssert.AreEqual(CatchBlockExceptionSource.RetryQueueWorker, catchBlockError.ExceptionSource);
			ClassicAssert.IsTrue(catchBlockError.IsCritical);

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
		}

		[Test]
		public async Task Should_ReadyWaitSeam_ReceiveReadyTimeBasedDelays()
		{
			var readyWait = new RecordingReadyWait();
			var queue = new RetryQueue(null, readyWait);
			var sleeping = NewSession();

			await queue.EnqueueAsync(sleeping.Session, DateTime.UtcNow + TimeSpan.FromMilliseconds(200), sleeping.Session.Token);
			await DeferredRetryTestHelpers.Complete(sleeping.Terminal.Task);
			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());

			var waits = readyWait.Waits;
			ClassicAssert.IsTrue(waits.Any(w => w > TimeSpan.Zero && w < TimeSpan.FromMilliseconds(201)));
		}
	}
}