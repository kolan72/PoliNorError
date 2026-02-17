using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class BulkheadPolicy : Policy, IBulkheadPolicy
	{
		private readonly ISimplePolicyProcessor _processor;
		private readonly BulkheadOptions _options;

		private readonly object _sync = new object();
		private SemaphoreSlim _semaphore;
		private int _queuedCount;
		private Action<PolicyResult> _onRejected;

		public BulkheadPolicy(int maxParallelization = 1, int maxQueueSize = 0, IBulkErrorProcessor bulkErrorProcessor = null)
			: this(new BulkheadOptions { MaxParallelization = maxParallelization, MaxQueueSize = maxQueueSize }, bulkErrorProcessor)
		{
		}

		public BulkheadPolicy(BulkheadOptions options = null, IBulkErrorProcessor bulkErrorProcessor = null)
			: this(new SimplePolicyProcessor(bulkErrorProcessor), options)
		{
		}

		internal BulkheadPolicy(ISimplePolicyProcessor processor, BulkheadOptions options = null)
			: base((IPolicyProcessor)processor)
		{
			_processor = processor;
			_options = options ?? new BulkheadOptions();
			_semaphore = new SemaphoreSlim(Math.Max(1, _options.MaxParallelization));
		}

		public BulkheadPolicy WithLimits(int maxParallelization, int maxQueueSize = 0)
		{
			lock (_sync)
			{
				_options.MaxParallelization = Math.Max(1, maxParallelization);
				_options.MaxQueueSize = Math.Max(0, maxQueueSize);
				_semaphore = new SemaphoreSlim(_options.MaxParallelization);
			}

			return this;
		}

		public BulkheadPolicy WithQueueTimeout(TimeSpan timeout)
		{
			_options.QueueTimeout = timeout;
			return this;
		}

		public BulkheadPolicy OnRejected(Action<PolicyResult> onRejected)
		{
			_onRejected = onRejected;
			return this;
		}

		IBulkheadPolicy IBulkheadPolicy.WithLimits(int maxParallelization, int maxQueueSize) => WithLimits(maxParallelization, maxQueueSize);
		IBulkheadPolicy IBulkheadPolicy.WithQueueTimeout(TimeSpan timeout) => WithQueueTimeout(timeout);
		IBulkheadPolicy IBulkheadPolicy.OnRejected(Action<PolicyResult> onRejected) => OnRejected(onRejected);

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			if (action is null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!TryEnter(token))
			{
				return CreateRejectedResult();
			}

			try
			{
				var result = _processor.Execute(action, token).SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!TryEnter(token))
			{
				return CreateRejectedResult<T>();
			}

			try
			{
				var result = _processor.Execute(func, token).SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!await TryEnterAsync(token).ConfigureAwait(configureAwait))
			{
				return CreateRejectedResult();
			}

			try
			{
				var result = (await _processor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait)).SetPolicyName(PolicyName);
				await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
				return result;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!await TryEnterAsync(token).ConfigureAwait(configureAwait))
			{
				return CreateRejectedResult<T>();
			}

			try
			{
				var result = (await _processor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait)).SetPolicyName(PolicyName);
				await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
				return result;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private bool TryEnter(CancellationToken token)
		{
			if (_semaphore.Wait(0, token))
			{
				return true;
			}

			if (_options.MaxQueueSize <= 0)
			{
				return false;
			}

			if (Interlocked.Increment(ref _queuedCount) > _options.MaxQueueSize)
			{
				Interlocked.Decrement(ref _queuedCount);
				return false;
			}

			try
			{
				if (_options.QueueTimeout == Timeout.InfiniteTimeSpan)
				{
					_semaphore.Wait(token);
					return true;
				}

				return _semaphore.Wait(_options.QueueTimeout, token);
			}
			finally
			{
				Interlocked.Decrement(ref _queuedCount);
			}
		}

		private async Task<bool> TryEnterAsync(CancellationToken token)
		{
			if (await _semaphore.WaitAsync(0, token).ConfigureAwait(false))
			{
				return true;
			}

			if (_options.MaxQueueSize <= 0)
			{
				return false;
			}

			if (Interlocked.Increment(ref _queuedCount) > _options.MaxQueueSize)
			{
				Interlocked.Decrement(ref _queuedCount);
				return false;
			}

			try
			{
				if (_options.QueueTimeout == Timeout.InfiniteTimeSpan)
				{
					await _semaphore.WaitAsync(token).ConfigureAwait(false);
					return true;
				}

				return await _semaphore.WaitAsync(_options.QueueTimeout, token).ConfigureAwait(false);
			}
			finally
			{
				Interlocked.Decrement(ref _queuedCount);
			}
		}

		private PolicyResult CreateRejectedResult()
		{
			var result = new PolicyResult();
			result.AddError(new BulkheadRejectedException());
			result.SetFailedInner();
			result.SetPolicyName(PolicyName);
			_onRejected?.Invoke(result);
			PublishTelemetry("bulkhead_rejected", result.Errors.FirstOrDefault(), CancellationToken.None);
			return result;
		}

		private PolicyResult<T> CreateRejectedResult<T>()
		{
			var result = new PolicyResult<T>();
			result.AddError(new BulkheadRejectedException());
			result.SetFailedInner();
			result.SetPolicyName(PolicyName);
			_onRejected?.Invoke(result);
			PublishTelemetry("bulkhead_rejected", result.Errors.FirstOrDefault(), CancellationToken.None);
			return result;
		}

		private void PublishTelemetry(string eventName, Exception exception, CancellationToken token)
		{
			var sink = PolicyRuntimeMetadata.GetEventSink(this);
			if (sink is null)
			{
				return;
			}

			sink.Publish(new PolicyTelemetryEvent
			{
				EventName = eventName,
				Exception = exception,
				PolicyName = PolicyName,
				Context = new PolicyExecutionContext
				{
					OperationKey = PolicyRuntimeMetadata.GetOperationKey(this),
					CancellationToken = token
				}
			});
		}
	}
}
