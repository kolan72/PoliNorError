using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class RetryPolicy : HandleErrorPolicyBase, IRetryPolicy, IWithErrorFilter<RetryPolicy>, ICanAddPolicyResultHandler<RetryPolicy>
	{
		private readonly IRetryProcessor _retryProcessor;

		public RetryPolicy(int retryCount) : this(retryCount, null) { }

		public RetryPolicy(int retryCount, Action<PolicyResult, Exception> errorSaverFunc) : this(retryCount, null, errorSaverFunc) {}

		public RetryPolicy(int retryCount, Action<RetryCountInfoOptions> action, Action<PolicyResult, Exception> errorSaverFunc = null) : this(retryCount, null, errorSaverFunc, action ){ }

		public RetryPolicy(int retryCount, IBulkErrorProcessor bulkErrorProcessor, Action<PolicyResult, Exception> errorSaverFunc = null, Action<RetryCountInfoOptions> action = null) : this(new DefaultRetryProcessor(bulkErrorProcessor, errorSaverFunc), retryCount, action) { }

		public RetryPolicy(IRetryProcessor retryProcessor, int retryCount, Action<RetryCountInfoOptions> action = null) : this(retryProcessor, RetryCountInfo.Limited(retryCount, action)) { }

		public static RetryPolicy InfiniteRetries() => InfiniteRetries(null);

		public static RetryPolicy InfiniteRetries(Action<PolicyResult, Exception> errorSaverFunc) => InfiniteRetries(null, errorSaverFunc);

		public static RetryPolicy InfiniteRetries(Action<RetryCountInfoOptions> action, Action<PolicyResult, Exception> errorSaverFunc = null) => InfiniteRetries(null, errorSaverFunc, action);

		public static RetryPolicy InfiniteRetries(IBulkErrorProcessor bulkErrorProcessor, Action<PolicyResult, Exception> errorSaverFunc = null, Action<RetryCountInfoOptions> action = null) => InfiniteRetries(new DefaultRetryProcessor(bulkErrorProcessor, errorSaverFunc), action);

		public static RetryPolicy InfiniteRetries(IRetryProcessor retryProcessor, Action<RetryCountInfoOptions> action) => new RetryPolicy(retryProcessor, RetryCountInfo.Infinite(action));

		internal RetryPolicy(IRetryProcessor retryProcessor, RetryCountInfo retryCountInfo) : base(retryProcessor) => (RetryInfo, _retryProcessor) = (retryCountInfo, retryProcessor);

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
			PolicyResult retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = _retryProcessor.Retry(action, RetryInfo, token);
			}
			else
			{
				if (action == null)
					return PolicyResult.ForSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper(_wrappedPolicy, action, token);
				Action actionWrapped = wrapper.Handle;

				retryResult = _retryProcessor.Retry(actionWrapped, RetryInfo, token);
				retryResult.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}
			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			PolicyResult<T> retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = _retryProcessor.Retry(func, RetryInfo, token);
			}
			else
			{
				if (func == null)
					return PolicyResult<T>.ForSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper<T>(_wrappedPolicy, func, token);
				Func<T> funcWrapped = wrapper.Handle;

				retryResult = _retryProcessor.Retry(funcWrapped, RetryInfo, token);
				retryResult.WrappedPolicyResults = wrapper.PolicyResults.Select(pr => pr.ToPolicyDelegateResult());
			}
			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			PolicyResult retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = await _retryProcessor.RetryAsync(func, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				if (func == null)
					return PolicyResult.ForNotSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper(_wrappedPolicy, func, token, configureAwait);
				Func<CancellationToken, Task> funcWrapped = wrapper.HandleAsync;

				retryResult = await _retryProcessor.RetryAsync(funcWrapped, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
				retryResult.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}
			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			PolicyResult<T> retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = await _retryProcessor.RetryAsync(func, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				if (func == null)
					return PolicyResult<T>.ForNotSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper<T>(_wrappedPolicy, func, token, configureAwait);
				Func<CancellationToken, Task<T>> funcWrapped = wrapper.HandleAsync;

				retryResult = await _retryProcessor.RetryAsync(funcWrapped, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
				retryResult.WrappedPolicyResults = wrapper.PolicyResults.Select(pr => pr.ToPolicyDelegateResult());
			}
			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public RetryPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<RetryPolicy, TException>(func);

		public RetryPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<RetryPolicy>(expression);

		public RetryPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<RetryPolicy, TException>(func);

		public RetryPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<RetryPolicy>(expression);

		public RetryPolicy AddPolicyResultHandler(Action<PolicyResult> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public RetryPolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public RetryPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public RetryPolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public RetryPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public RetryCountInfo RetryInfo { get; }
	}
}
