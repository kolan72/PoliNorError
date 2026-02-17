using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

namespace PoliNorError
{
	public sealed class CircuitBreakerPolicy : Policy, ICircuitBreakerPolicy, IWithErrorFilter<CircuitBreakerPolicy>, IWithInnerErrorFilter<CircuitBreakerPolicy>, ICanAddErrorFilter<CircuitBreakerPolicy>
	{
		private readonly ISimplePolicyProcessor _processor;
		private readonly CircuitBreakerOptions _options;
		private readonly object _stateLock = new object();

		private CircuitBreakerState _state = CircuitBreakerState.Closed;
		private int _failureCount;
		private int _halfOpenCallCount;
		private DateTime _windowStartUtc = DateTime.UtcNow;
		private DateTime _openUntilUtc;

		private Action<PolicyResult> _onBreak;
		private Action _onReset;
		private Action _onHalfOpen;

		public CircuitBreakerPolicy(IBulkErrorProcessor bulkErrorProcessor = null, CircuitBreakerOptions options = null)
			: this(new SimplePolicyProcessor(bulkErrorProcessor), options)
		{
		}

		internal CircuitBreakerPolicy(ISimplePolicyProcessor processor, CircuitBreakerOptions options = null)
			: base((IPolicyProcessor)processor)
		{
			_processor = processor;
			_options = options ?? new CircuitBreakerOptions();
		}

		public CircuitBreakerState State
		{
			get
			{
				lock (_stateLock)
				{
					return _state;
				}
			}
		}

		public CircuitBreakerPolicy WithOptions(Action<CircuitBreakerOptions> configure)
		{
			configure?.Invoke(_options);
			return this;
		}

		public CircuitBreakerPolicy OnBreak(Action<PolicyResult> onBreak)
		{
			_onBreak = onBreak;
			return this;
		}

		public CircuitBreakerPolicy OnReset(Action onReset)
		{
			_onReset = onReset;
			return this;
		}

		public CircuitBreakerPolicy OnHalfOpen(Action onHalfOpen)
		{
			_onHalfOpen = onHalfOpen;
			return this;
		}

		ICircuitBreakerPolicy ICircuitBreakerPolicy.WithOptions(Action<CircuitBreakerOptions> configure) => WithOptions(configure);
		ICircuitBreakerPolicy ICircuitBreakerPolicy.OnBreak(Action<PolicyResult> onBreak) => OnBreak(onBreak);
		ICircuitBreakerPolicy ICircuitBreakerPolicy.OnReset(Action onReset) => OnReset(onReset);
		ICircuitBreakerPolicy ICircuitBreakerPolicy.OnHalfOpen(Action onHalfOpen) => OnHalfOpen(onHalfOpen);

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			if (action is null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!TryEnterExecution())
			{
				return CreateOpenRejectedResult();
			}

			var result = _processor.Execute(action, token).SetPolicyName(PolicyName);
			UpdateStateByResult(result);
			HandlePolicyResult(result, token);
			return result;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!TryEnterExecution())
			{
				return CreateOpenRejectedResult<T>();
			}

			var result = _processor.Execute(func, token).SetPolicyName(PolicyName);
			UpdateStateByResult(result);
			HandlePolicyResult(result, token);
			return result;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!TryEnterExecution())
			{
				return CreateOpenRejectedResult();
			}

			var result = (await _processor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait)).SetPolicyName(PolicyName);
			UpdateStateByResult(result);
			await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
			return result;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func is null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			if (!TryEnterExecution())
			{
				return CreateOpenRejectedResult<T>();
			}

			var result = (await _processor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait)).SetPolicyName(PolicyName);
			UpdateStateByResult(result);
			await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
			return result;
		}

		public CircuitBreakerPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<CircuitBreakerPolicy, TException>(func);
		public CircuitBreakerPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<CircuitBreakerPolicy>(expression);
		public CircuitBreakerPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<CircuitBreakerPolicy, TException>(func);
		public CircuitBreakerPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<CircuitBreakerPolicy>(expression);
		public CircuitBreakerPolicy IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<CircuitBreakerPolicy, TInnerException>(predicate);
		public CircuitBreakerPolicy ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<CircuitBreakerPolicy, TInnerException>(predicate);

		public CircuitBreakerPolicy AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		public CircuitBreakerPolicy AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}

		private bool TryEnterExecution()
		{
			lock (_stateLock)
			{
				var now = DateTime.UtcNow;
				if (_state == CircuitBreakerState.Open)
				{
					if (now < _openUntilUtc)
					{
						return false;
					}

					_state = CircuitBreakerState.HalfOpen;
					_halfOpenCallCount = 0;
					_onHalfOpen?.Invoke();
					PublishTelemetry("circuit_half_open", null, CancellationToken.None);
				}

				if (_state == CircuitBreakerState.HalfOpen)
				{
					if (_halfOpenCallCount >= Math.Max(1, _options.HalfOpenMaxCalls))
					{
						return false;
					}

					_halfOpenCallCount++;
				}

				return true;
			}
		}

		private void UpdateStateByResult(PolicyResult result)
		{
			var isFailure = IsFailure(result);
			lock (_stateLock)
			{
				var now = DateTime.UtcNow;
				if (now - _windowStartUtc > _options.SamplingDuration)
				{
					_windowStartUtc = now;
					_failureCount = 0;
				}

				switch (_state)
				{
					case CircuitBreakerState.Closed:
						if (isFailure)
						{
							_failureCount++;
							if (_failureCount >= Math.Max(1, _options.FailureThreshold))
							{
								_state = CircuitBreakerState.Open;
								_openUntilUtc = now.Add(_options.OpenDuration);
								_onBreak?.Invoke(result);
								PublishTelemetry("circuit_opened", result.Errors.FirstOrDefault(), CancellationToken.None);
							}
						}
						break;
					case CircuitBreakerState.HalfOpen:
						if (isFailure)
						{
							_state = CircuitBreakerState.Open;
							_openUntilUtc = now.Add(_options.OpenDuration);
							_onBreak?.Invoke(result);
							PublishTelemetry("circuit_reopened", result.Errors.FirstOrDefault(), CancellationToken.None);
						}
						else
						{
							_state = CircuitBreakerState.Closed;
							_failureCount = 0;
							_windowStartUtc = now;
							_onReset?.Invoke();
							PublishTelemetry("circuit_reset", null, CancellationToken.None);
						}
						break;
				}
			}
		}

		private bool IsFailure(PolicyResult result)
		{
			if (_options.BreakOnHandledExceptionsOnly)
			{
				return result.Errors.Any() && !result.ErrorFilterUnsatisfied;
			}

			return result.IsFailed || result.IsCanceled || result.Errors.Any();
		}

		private PolicyResult CreateOpenRejectedResult()
		{
			var result = new PolicyResult();
			result.AddError(new CircuitBreakerOpenException());
			result.SetFailedInner();
			result.SetPolicyName(PolicyName);
			PublishTelemetry("circuit_open_rejected", result.Errors.FirstOrDefault(), CancellationToken.None);
			return result;
		}

		private PolicyResult<T> CreateOpenRejectedResult<T>()
		{
			var result = new PolicyResult<T>();
			result.AddError(new CircuitBreakerOpenException());
			result.SetFailedInner();
			result.SetPolicyName(PolicyName);
			PublishTelemetry("circuit_open_rejected", result.Errors.FirstOrDefault(), CancellationToken.None);
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
