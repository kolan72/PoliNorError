using System;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.PolicyResultHandleErrorDelegates;

namespace PoliNorError
{
	public sealed class DefaultRetryProcessor : PolicyProcessor, IRetryProcessor
	{
		private readonly Action<PolicyResult, Exception> _errorSaverFunc;
		private readonly bool _isErrorSaverDefault;

		public DefaultRetryProcessor(Action<Exception> errorSaverFunc = null, bool setFailedIfInvocationError = false) : this(null, errorSaverFunc, setFailedIfInvocationError) { }

		public DefaultRetryProcessor(IBulkErrorProcessor bulkErrorProcessor, Action<Exception> errorSaverFunc = null, bool setFailedIfInvocationError = false)
			: base(bulkErrorProcessor)
		{
			_errorSaverFunc = GetWrappedErrorSaver(errorSaverFunc, setFailedIfInvocationError);
			_isErrorSaverDefault = errorSaverFunc == null;
		}

		public PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			if (action == null)
				return PolicyResult.ForSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(action)}' is null."));

			var result = PolicyResult.ForSync();
			result.ErrorsNotUsed = !_isErrorSaverDefault;

			int tryCount = retryCountInfo.StartTryCount;
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					action();
					if (tryCount == retryCountInfo.StartTryCount)
					{
						result.SetOk();
					}
					if(result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (AggregateException ae) when (ae.HasCanceledException(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					_errorSaverFunc(result, ex);
					if (result.IsFailed)
					{
						break;
					}
					var handleResult = HandleCatchBlockAndChangeResult(ex, result, retryCountInfo, tryCount, token);
					if (!handleResult.IsFailed)
						tryCount++;
				}
			}
			while (!result.IsFailed);
			return result;
		}

		public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult<T>.ForSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(func)}' is null."));

			if (typeof(T).Equals(typeof(Task)) || typeof(T).IsSubclassOf(typeof(Task)))
			{
				throw new ArgumentException("Do not use this method for task return type!");
			}

			var result = PolicyResult<T>.ForSync();
			result.ErrorsNotUsed = !_isErrorSaverDefault;

			int tryCount = retryCountInfo.StartTryCount;
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					var res = func();
					if (tryCount == retryCountInfo.StartTryCount)
					{
						result.SetOk();
					}
					result.SetResult(res);
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (AggregateException ae) when (ae.HasCanceledException(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					_errorSaverFunc(result, ex);
					if (result.IsFailed)
					{
						break;
					}
					var handleResult = HandleCatchBlockAndChangeResult(ex, result, retryCountInfo, tryCount, token);
					if (!handleResult.IsFailed)
						tryCount++;
				}
			}
			while (!result.IsFailed);
			return result;
		}

		public async Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult.ForNotSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(func)}' is null."));

			var result = PolicyResult.InitByConfigureAwait(configureAwait);
			result.ErrorsNotUsed = !_isErrorSaverDefault;

			int tryCount = retryCountInfo.StartTryCount;
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					await func(token).ConfigureAwait(configureAwait);
					if (tryCount == retryCountInfo.StartTryCount)
					{
						result.SetOk();
					}
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					_errorSaverFunc(result, ex);
					if (result.IsFailed)
					{
						break;
					}
					var handleResult = await HandleCatchBlockAndChangeResultAsync(ex, result, retryCountInfo, tryCount, configureAwait, token).ConfigureAwait(configureAwait);
					if (!handleResult.IsFailed)
						Interlocked.Increment(ref tryCount);
				}
			}
			while (!result.IsFailed);
			return result;
		}

		public async Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult<T>.ForNotSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(func)}' is null."));

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);
			result.ErrorsNotUsed = !_isErrorSaverDefault;

			int tryCount = retryCountInfo.StartTryCount;
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					var res = await func(token).ConfigureAwait(configureAwait);
					if (tryCount == retryCountInfo.StartTryCount)
					{
						result.SetOk();
					}
					result.SetResult(res);
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					_errorSaverFunc(result, ex);
					if (result.IsFailed)
					{
						break;
					}
					var handleResult = await HandleCatchBlockAndChangeResultAsync(ex, result, retryCountInfo, tryCount, configureAwait, token).ConfigureAwait(configureAwait);
					if (!handleResult.IsFailed)
						Interlocked.Increment(ref tryCount);
				}
			}
			while (!result.IsFailed);
			return result;
		}

		private PolicyResult HandleCatchBlockAndChangeResult(Exception ex, PolicyResult result, RetryCountInfo retryCountInfo, int tryErrorCount, CancellationToken token)
		{
			result.ChangeByHandleCatchBlockResult(CanHandleCatchBlock());
			return result;
			HandleCatchBlockResult CanHandleCatchBlock()
			{
				if (token.IsCancellationRequested)
				{
					return HandleCatchBlockResult.Canceled;
				}
				var checkRetryResult = CanRetry(ex, retryCountInfo, tryErrorCount);
				if (checkRetryResult == HandleCatchBlockResult.Success)
				{
					var bulkProcessResult = _bulkErrorProcessor.Process(ProcessErrorInfo.FromRetry(tryErrorCount), ex, token);
					result.AddBulkProcessorErrors(bulkProcessResult);
					return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : checkRetryResult;
				}
				else
				{
					return checkRetryResult;
				}
			}
		}

		private async Task<PolicyResult> HandleCatchBlockAndChangeResultAsync(Exception ex, PolicyResult result, RetryCountInfo retryCountInfo, int tryErrorCount, bool configAwait, CancellationToken token)
		{
			result.ChangeByHandleCatchBlockResult(await CanHandleCatchBlockAsync().ConfigureAwait(configAwait));
			return result;
			async Task<HandleCatchBlockResult> CanHandleCatchBlockAsync()
			{
				if (token.IsCancellationRequested)
				{
					return HandleCatchBlockResult.Canceled;
				}
				var checkRetryResult = CanRetry(ex, retryCountInfo, tryErrorCount);
				if (checkRetryResult == HandleCatchBlockResult.Success)
				{
					var bulkProcessResult = await _bulkErrorProcessor.ProcessAsync(ProcessErrorInfo.FromRetry(tryErrorCount), ex, configAwait, token).ConfigureAwait(configAwait);
					result.AddBulkProcessorErrors(bulkProcessResult);
					return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : checkRetryResult;
				}
				else
				{
					return checkRetryResult;
				}
			}
		}

		private HandleCatchBlockResult CanRetry(Exception exception, RetryCountInfo retryCountInfo, int curErrorsCount)
		{
			if (!GetCanHandle()(exception))
				return HandleCatchBlockResult.FailedByErrorFilter;
			else if (!retryCountInfo.CanRetry(curErrorsCount))
				return HandleCatchBlockResult.FailedByPolicyRules;
			else
				return HandleCatchBlockResult.Success;
		}
	}
}
