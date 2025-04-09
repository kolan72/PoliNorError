using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

namespace PoliNorError
{
	public sealed partial class RetryPolicy : Policy, IRetryPolicy, IWithErrorFilter<RetryPolicy>, IWithInnerErrorFilter<RetryPolicy>
	{
		public RetryPolicy(int retryCount, bool failedIfSaveErrorThrows = false, RetryDelay retryDelay = null) : this(retryCount, null, failedIfSaveErrorThrows, retryDelay) { }

		public RetryPolicy(int retryCount, Action<RetryCountInfoOptions> action, bool failedIfSaveErrorThrows = false, RetryDelay retryDelay = null) : this(retryCount, null, failedIfSaveErrorThrows, action, retryDelay) { }

		public RetryPolicy(int retryCount, IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, Action<RetryCountInfoOptions> action = null, RetryDelay retryDelay = null)
			: this(retryCount, null, bulkErrorProcessor, failedIfSaveErrorThrows, action, retryDelay) { }

		public RetryPolicy(IRetryProcessor retryProcessor, int retryCount, Action<RetryCountInfoOptions> action = null) : this(retryProcessor, RetryCountInfo.Limited(retryCount, action)) { }

		internal RetryPolicy(int retryCount, IDelayProvider delayProvider, IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, Action<RetryCountInfoOptions> action = null, RetryDelay retryDelay = null)
			: this(RetryCountInfo.Limited(retryCount, action), delayProvider, bulkErrorProcessor, failedIfSaveErrorThrows, retryDelay) { }

		internal RetryPolicy(RetryCountInfo retryCountInfo, IDelayProvider delayProvider, IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, RetryDelay retryDelay = null)
			: this(new DefaultRetryProcessor(bulkErrorProcessor, failedIfSaveErrorThrows, delayProvider), retryCountInfo)
		{
			Delay = retryDelay;
		}

		internal RetryPolicy(IRetryProcessor retryProcessor, RetryCountInfo retryCountInfo) : base(retryProcessor) => (RetryInfo, RetryProcessor) = (retryCountInfo, retryProcessor);

		public static RetryPolicy InfiniteRetries(bool failedIfSaveErrorThrows = false, RetryDelay retryDelay = null) => InfiniteRetries(null, failedIfSaveErrorThrows, retryDelay);

		public static RetryPolicy InfiniteRetries(Action<RetryCountInfoOptions> action, bool failedIfSaveErrorThrows = false, RetryDelay retryDelay = null) => InfiniteRetries(null, failedIfSaveErrorThrows, action, retryDelay);

		public static RetryPolicy InfiniteRetries(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, Action<RetryCountInfoOptions> action = null, RetryDelay retryDelay = null)
			=> InfiniteRetries(null, bulkErrorProcessor, failedIfSaveErrorThrows, action, retryDelay);

		public static RetryPolicy InfiniteRetries(IRetryProcessor retryProcessor, Action<RetryCountInfoOptions> action) => new RetryPolicy(retryProcessor, RetryCountInfo.Infinite(action));

		internal static RetryPolicy InfiniteRetries(IDelayProvider delayProvider, IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, Action<RetryCountInfoOptions> action = null, RetryDelay retryDelay = null)
			=> new RetryPolicy(RetryCountInfo.Infinite(action), delayProvider, bulkErrorProcessor, failedIfSaveErrorThrows, retryDelay);

		public RetryPolicy WithWait(Func<int, TimeSpan> delayOnRetryFunc)
		{
			return this.WithErrorProcessor(new DelayErrorProcessor(delayOnRetryFunc));
		}

		public RetryPolicy WithWait(Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg)
		{
			return this.WithErrorProcessor(new DelayErrorProcessor(delayOnRetryFunc, delayFuncArg));
		}

		public RetryPolicy WithWait(TimeSpan delay)
		{
			this.WithErrorProcessor(new DelayErrorProcessor(delay));
			return this;
		}

		public RetryPolicy WithWait(Func<int, Exception, TimeSpan> retryFunc)
		{
			this.WithErrorProcessor(new DelayErrorProcessor(retryFunc));
			return this;
		}

		public RetryPolicy WithWait(DelayErrorProcessor delayErrorProcessor)
		{
			this.WithErrorProcessor(delayErrorProcessor);
			return this;
		}

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			PolicyResult retryResult;
			if (Delay is null)
			{
				retryResult = RetryProcessor.Retry(Act, RetryInfo, token);
			}
			else
			{
				retryResult = ((DefaultRetryProcessor)RetryProcessor).Retry(Act, RetryInfo, Delay, token);
			}

			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
							 .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult Handle<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			PolicyResult retryResult;
			if (Delay is null)
			{
				retryResult = processor.RetryWithErrorContext(Act, param, RetryInfo, token);
			}
			else
			{
				retryResult = processor.RetryWithErrorContext(Act, param, RetryInfo, Delay, token);
			}

			retryResult = retryResult
							  .SetWrappedPolicyResults(Wrapper)
							  .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult Handle<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			if (HasPolicyWrapperFactory)
			{
				return Handle(action.Apply(param), token);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

				PolicyResult retryResult;
				if (Delay is null)
				{
					retryResult = processor.Retry(action, param, RetryInfo, token);
				}
				else
				{
					retryResult = processor.Retry(action, param, RetryInfo, Delay, token);
				}

				retryResult = retryResult
								  .SetPolicyName(PolicyName);

				HandlePolicyResult(retryResult, token);
				return retryResult;
			}
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			PolicyResult<T> retryResult;
			if (Delay is null)
			{
				retryResult = RetryProcessor.Retry(Fn, RetryInfo, token);
			}
			else
			{
				retryResult = ((DefaultRetryProcessor)RetryProcessor).Retry(Fn, RetryInfo, Delay, token);
			}

			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult<T> Handle<TErrorContext, T>(Func<T> func, TErrorContext param, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			PolicyResult<T> retryResult;
			if (Delay is null)
			{
				retryResult = processor.RetryWithErrorContext(Fn, param, RetryInfo, token);
			}
			else
			{
				retryResult = processor.RetryWithErrorContext(Fn, param, RetryInfo, Delay, token);
			}

			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult<T> Handle<TParam, T>(Func<TParam, T> func, TParam param, CancellationToken token = default)
		{
			if (HasPolicyWrapperFactory)
			{
				return Handle(func.Apply(param), token);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

				PolicyResult<T> retryResult;
				if (Delay is null)
				{
					retryResult = processor.Retry(func, param, RetryInfo, token);
				}
				else
				{
					retryResult = processor.Retry(func, param, RetryInfo, Delay, token);
				}

				retryResult = retryResult
								  .SetPolicyName(PolicyName);

				HandlePolicyResult(retryResult, token);
				return retryResult;
			}
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			PolicyResult retryResult;

			if (Delay is null)
			{
				retryResult = await RetryProcessor.RetryAsync(Fn, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				retryResult = await ((DefaultRetryProcessor)RetryProcessor).RetryAsync(Fn, RetryInfo, Delay, configureAwait, token).ConfigureAwait(configureAwait);
			}

			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, bool configureAwait, CancellationToken token)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			PolicyResult retryResult;

			if (Delay is null)
			{
				retryResult = await processor.RetryWithErrorContextAsync(Fn, param, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				retryResult = await processor.RetryWithErrorContextAsync(Fn, param, RetryInfo, Delay, configureAwait, token).ConfigureAwait(configureAwait);
			}
			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, bool configureAwait, CancellationToken token)
		{
			if (HasPolicyWrapperFactory)
			{
				return await HandleAsync(func.Apply(param), configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

				PolicyResult retryResult;

				if (Delay is null)
				{
					retryResult = await processor.RetryAsync(func, param, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
				}
				else
				{
					retryResult = await processor.RetryAsync(func, param, RetryInfo, Delay, configureAwait, token).ConfigureAwait(configureAwait);
				}
				retryResult = retryResult
										.SetPolicyName(PolicyName);

				await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
				return retryResult;
			}
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			PolicyResult<T> retryResult;

			if (Delay is null)
			{
				retryResult = await RetryProcessor.RetryAsync(Fn, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				retryResult = await ((DefaultRetryProcessor)RetryProcessor).RetryAsync(Fn, RetryInfo, Delay, configureAwait, token).ConfigureAwait(configureAwait);
			}

			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
						.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, bool configureAwait, CancellationToken token)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			PolicyResult<T> retryResult;

			if (Delay is null)
			{
				retryResult = await processor.RetryWithErrorContextAsync(Fn, param, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				retryResult = await processor.RetryWithErrorContextAsync(Fn, param, RetryInfo, Delay, configureAwait, token).ConfigureAwait(configureAwait);
			}
			retryResult = retryResult.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public RetryPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<RetryPolicy, TException>(func);

		public RetryPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<RetryPolicy>(expression);

		public RetryPolicy ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<RetryPolicy, TException1, TException2>();

		public RetryPolicy ExcludeErrorSet(IErrorSet errorSet) => this.ExcludeErrorSet<RetryPolicy>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the handling by the Retry policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public RetryPolicy ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<RetryPolicy, TInnerException>(predicate);

		public RetryPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<RetryPolicy, TException>(func);

		public RetryPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<RetryPolicy>(expression);

		public RetryPolicy IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<RetryPolicy, TException1, TException2>();

		public RetryPolicy IncludeErrorSet(IErrorSet errorSet) => this.IncludeErrorSet<RetryPolicy>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the handling by the Retry policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public RetryPolicy IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<RetryPolicy, TInnerException>(predicate);

		public RetryPolicy AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public RetryPolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public RetryPolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public RetryPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public RetryPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public RetryPolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{RetryPolicy}"/>
		public RetryPolicy SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{RetryPolicy, T}"/>
		public RetryPolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{RetryPolicy, T}"/>
		public RetryPolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}

		public RetryPolicy UseCustomErrorSaver(IErrorProcessor saveErrorProcessor)
		{
			RetryProcessor.UseCustomErrorSaver(saveErrorProcessor);
			return this;
		}

		public RetryPolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor _);
			return this.WithErrorContextProcessorOf<RetryPolicy, TErrorContext>(actionProcessor);
		}

		public RetryPolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor _);
			return this.WithErrorContextProcessorOf<RetryPolicy, TErrorContext>(actionProcessor, cancellationType);
		}

		public RetryPolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor _);
			return this.WithErrorContextProcessorOf<RetryPolicy, TErrorContext>(funcProcessor);
		}

		public RetryPolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor _);
			return this.WithErrorContextProcessorOf<RetryPolicy, TErrorContext>(funcProcessor, cancellationType);
		}

		public RetryPolicy WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor _);
			return this.WithErrorContextProcessor<RetryPolicy, TErrorContext>(errorProcessor);
		}

		internal IRetryProcessor RetryProcessor { get; }

		public RetryCountInfo RetryInfo { get; }

		internal RetryDelay Delay { get; }

		private void ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor proc)
		{
			ThrowHelper.ThrowIfNotImplemented(RetryProcessor, out proc);
		}
	}
}
