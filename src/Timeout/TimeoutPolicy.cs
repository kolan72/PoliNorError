using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class TimeoutPolicy : Policy, ITimeoutPolicy
	{
		private readonly ISimplePolicyProcessor _processor;
		private readonly TimeoutPolicyOptions _options;

		public TimeoutPolicy(TimeSpan timeout)
			: this(new TimeoutPolicyOptions { Timeout = timeout })
		{
		}

		public TimeoutPolicy(TimeoutPolicyOptions options = null, IBulkErrorProcessor bulkErrorProcessor = null)
			: this(new SimplePolicyProcessor(bulkErrorProcessor), options)
		{
		}

		internal TimeoutPolicy(ISimplePolicyProcessor processor, TimeoutPolicyOptions options = null)
			: base((IPolicyProcessor)processor)
		{
			_processor = processor;
			_options = options ?? new TimeoutPolicyOptions();
		}

		public TimeoutPolicy WithTimeout(TimeSpan timeout)
		{
			_options.Timeout = timeout;
			return this;
		}

		public TimeoutPolicy WithStrategy(TimeoutStrategy strategy)
		{
			_options.Strategy = strategy;
			return this;
		}

		public TimeoutPolicy WithOptions(Action<TimeoutPolicyOptions> configure)
		{
			configure?.Invoke(_options);
			return this;
		}

		ITimeoutPolicy ITimeoutPolicy.WithTimeout(TimeSpan timeout) => WithTimeout(timeout);
		ITimeoutPolicy ITimeoutPolicy.WithStrategy(TimeoutStrategy strategy) => WithStrategy(strategy);
		ITimeoutPolicy ITimeoutPolicy.WithOptions(Action<TimeoutPolicyOptions> configure) => WithOptions(configure);

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			if (action is null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var timeoutHit = false;
			void TimedAction() => ExecuteSyncWithTimeout(action, token, () => timeoutHit = true);

			var result = _processor.Execute(TimedAction, token).SetPolicyName(PolicyName);
			if (timeoutHit)
			{
				result.SetFailedAndCanceled();
				PublishTelemetry("timeout", result.Errors.FirstOrDefault() ?? new TimeoutRejectedException(_options.Timeout), token);
				if (_options.ThrowTimeoutException)
				{
					throw new TimeoutRejectedException(_options.Timeout);
				}
			}

			HandlePolicyResult(result, token);
			return result;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var timeoutHit = false;
			T TimedFunc() => ExecuteSyncWithTimeout(func, token, () => timeoutHit = true);

			var result = _processor.Execute(TimedFunc, token).SetPolicyName(PolicyName);
			if (timeoutHit)
			{
				result.SetFailedAndCanceled();
				PublishTelemetry("timeout", result.Errors.FirstOrDefault() ?? new TimeoutRejectedException(_options.Timeout), token);
				if (_options.ThrowTimeoutException)
				{
					throw new TimeoutRejectedException(_options.Timeout);
				}
			}

			HandlePolicyResult(result, token);
			return result;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var timeoutHit = false;
			async Task TimedAction(CancellationToken ct) =>
				await ExecuteAsyncWithTimeout(func, ct, configureAwait, () => timeoutHit = true).ConfigureAwait(configureAwait);

			var result = (await _processor.ExecuteAsync(TimedAction, configureAwait, token).ConfigureAwait(configureAwait))
				.SetPolicyName(PolicyName);

			if (timeoutHit)
			{
				result.SetFailedAndCanceled();
				PublishTelemetry("timeout", result.Errors.FirstOrDefault() ?? new TimeoutRejectedException(_options.Timeout), token);
				if (_options.ThrowTimeoutException)
				{
					throw new TimeoutRejectedException(_options.Timeout);
				}
			}

			await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
			return result;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var timeoutHit = false;
			async Task<T> TimedFunc(CancellationToken ct) =>
				await ExecuteAsyncWithTimeout(func, ct, configureAwait, () => timeoutHit = true).ConfigureAwait(configureAwait);

			var result = (await _processor.ExecuteAsync(TimedFunc, configureAwait, token).ConfigureAwait(configureAwait))
				.SetPolicyName(PolicyName);

			if (timeoutHit)
			{
				result.SetFailedAndCanceled();
				PublishTelemetry("timeout", result.Errors.FirstOrDefault() ?? new TimeoutRejectedException(_options.Timeout), token);
				if (_options.ThrowTimeoutException)
				{
					throw new TimeoutRejectedException(_options.Timeout);
				}
			}

			await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
			return result;
		}

		private void ExecuteSyncWithTimeout(Action action, CancellationToken token, Action onTimeout)
		{
			using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token))
			{
				if (_options.Strategy == TimeoutStrategy.Optimistic)
				{
					linkedCts.CancelAfter(_options.Timeout);
				}

				var task = Task.Run(action, linkedCts.Token);
				var completed = task.Wait(_options.Timeout);
				if (!completed)
				{
					onTimeout?.Invoke();
					linkedCts.Cancel();
					throw new TimeoutRejectedException(_options.Timeout);
				}

				task.GetAwaiter().GetResult();
			}
		}

		private T ExecuteSyncWithTimeout<T>(Func<T> func, CancellationToken token, Action onTimeout)
		{
			using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token))
			{
				if (_options.Strategy == TimeoutStrategy.Optimistic)
				{
					linkedCts.CancelAfter(_options.Timeout);
				}

				var task = Task.Run(func, linkedCts.Token);
				var completed = task.Wait(_options.Timeout);
				if (!completed)
				{
					onTimeout?.Invoke();
					linkedCts.Cancel();
					throw new TimeoutRejectedException(_options.Timeout);
				}

				return task.GetAwaiter().GetResult();
			}
		}

		private async Task ExecuteAsyncWithTimeout(
			Func<CancellationToken, Task> func,
			CancellationToken token,
			bool configureAwait,
			Action onTimeout)
		{
			if (_options.Strategy == TimeoutStrategy.Optimistic)
			{
				using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token))
				{
					linkedCts.CancelAfter(_options.Timeout);
					try
					{
						await func(linkedCts.Token).ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException) when (linkedCts.IsCancellationRequested && !token.IsCancellationRequested)
					{
						onTimeout?.Invoke();
						throw new TimeoutRejectedException(_options.Timeout);
					}
				}

				return;
			}

			var execTask = func(token);
			var timeoutTask = Task.Delay(_options.Timeout, token);
			var completed = await Task.WhenAny(execTask, timeoutTask).ConfigureAwait(configureAwait);
			if (completed != execTask)
			{
				onTimeout?.Invoke();
				throw new TimeoutRejectedException(_options.Timeout);
			}

			await execTask.ConfigureAwait(configureAwait);
		}

		private async Task<T> ExecuteAsyncWithTimeout<T>(
			Func<CancellationToken, Task<T>> func,
			CancellationToken token,
			bool configureAwait,
			Action onTimeout)
		{
			if (_options.Strategy == TimeoutStrategy.Optimistic)
			{
				using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token))
				{
					linkedCts.CancelAfter(_options.Timeout);
					try
					{
						return await func(linkedCts.Token).ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException) when (linkedCts.IsCancellationRequested && !token.IsCancellationRequested)
					{
						onTimeout?.Invoke();
						throw new TimeoutRejectedException(_options.Timeout);
					}
				}
			}

			var execTask = func(token);
			var timeoutTask = Task.Delay(_options.Timeout, token);
			var completed = await Task.WhenAny(execTask, timeoutTask).ConfigureAwait(configureAwait);
			if (completed != execTask)
			{
				onTimeout?.Invoke();
				throw new TimeoutRejectedException(_options.Timeout);
			}

			return await execTask.ConfigureAwait(configureAwait);
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
