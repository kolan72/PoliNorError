using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class RetryPolicy : Policy, IRetryPolicy, IWithErrorFilter<RetryPolicy>
	{
		public RetryPolicy(int retryCount, bool failedIfSaveErrorThrows = false) : this(retryCount, null, failedIfSaveErrorThrows) { }

		public RetryPolicy(int retryCount, Action<RetryCountInfoOptions> action, bool failedIfSaveErrorThrows = false) : this(retryCount, null, failedIfSaveErrorThrows, action) { }

		public RetryPolicy(int retryCount, IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, Action<RetryCountInfoOptions> action = null) : this(new DefaultRetryProcessor(bulkErrorProcessor, failedIfSaveErrorThrows), retryCount, action) { }

		public RetryPolicy(IRetryProcessor retryProcessor, int retryCount, Action<RetryCountInfoOptions> action = null) : this(retryProcessor, RetryCountInfo.Limited(retryCount, action)) { }

		public static RetryPolicy InfiniteRetries(bool failedIfSaveErrorThrows = false) => InfiniteRetries(null, failedIfSaveErrorThrows);

		public static RetryPolicy InfiniteRetries(Action<RetryCountInfoOptions> action, bool failedIfSaveErrorThrows = false) => InfiniteRetries(null, failedIfSaveErrorThrows, action);

		public static RetryPolicy InfiniteRetries(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false, Action<RetryCountInfoOptions> action = null) => InfiniteRetries(new DefaultRetryProcessor(bulkErrorProcessor, failedIfSaveErrorThrows), action);

		public static RetryPolicy InfiniteRetries(IRetryProcessor retryProcessor, Action<RetryCountInfoOptions> action) => new RetryPolicy(retryProcessor, RetryCountInfo.Infinite(action));

		internal RetryPolicy(IRetryProcessor retryProcessor, RetryCountInfo retryCountInfo) : base(retryProcessor) => (RetryInfo, RetryProcessor) = (retryCountInfo, retryProcessor);

		public RetryPolicy WithWait(Func<int, TimeSpan> delayOnRetryFunc)
		{
			TimeSpan retryFunc(int retryAttempt, Exception _) => delayOnRetryFunc(retryAttempt);
			return WithWait(retryFunc);
		}

		public RetryPolicy WithWait(Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg)
		{
			TimeSpan retryFunc(int retryAttempt, Exception exc) => delayOnRetryFunc(delayFuncArg, retryAttempt, exc);
			WithWait(retryFunc);
			return this;
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

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = RetryProcessor.Retry(Act, RetryInfo, token)
							 .SetWrappedPolicyResults(Wrapper)
							 .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = RetryProcessor.Retry(Fn, RetryInfo, token)
							.SetWrappedPolicyResults(Wrapper)
							.SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = (await RetryProcessor.RetryAsync(Fn, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = (await RetryProcessor.RetryAsync(Fn, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public RetryPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<RetryPolicy, TException>(func);

		public RetryPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<RetryPolicy>(expression);

		public RetryPolicy ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<RetryPolicy, TException1, TException2>();

		public RetryPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<RetryPolicy, TException>(func);

		public RetryPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<RetryPolicy>(expression);

		public RetryPolicy IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<RetryPolicy, TException1, TException2>();

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

		public RetryPolicy UseCustomErrorSaver(IErrorProcessor saveErrorProcessor)
		{
			RetryProcessor.UseCustomErrorSaver(saveErrorProcessor);
			return this;
		}

		internal IRetryProcessor RetryProcessor { get; }

		public RetryCountInfo RetryInfo { get; }
	}
}
