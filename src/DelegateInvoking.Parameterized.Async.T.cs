using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		// Simple policy methods for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, param, (ErrorProcessorParam)null, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, param, policyParams, false, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, param, null, policyParams, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, param, catchBlockFilter, policyParams, false, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).HandleAsync(func, param, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, param, catchBlockHandler, false, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, CatchBlockHandler catchBlockHandler, bool configureAwait, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).HandleAsync(func, param, configureAwait, token);

		// Retry policy methods for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryAsync(func, param, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryAsync(func, param, retryCount, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, param, configureAwait, token);

		// WaitAndRetry with TimeSpan delay for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, param, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, param, retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, param, configureAwait, token);

		// WaitAndRetry with retry function for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, param, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, param, retryCount, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, param, configureAwait, token);

		// Infinite retry methods for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryInfiniteAsync(func, param, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryInfiniteAsync(func, param, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).HandleAsync(func, param, configureAwait, token);

		// WaitAndRetryInfinite with TimeSpan delay for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfiniteAsync(func, param, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfiniteAsync(func, param, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, param, configureAwait, token);

		// WaitAndRetryInfinite with retry function for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfiniteAsync(func, param, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfiniteAsync(func, param, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, param, configureAwait, token);

		// Fallback policy methods for Func<TParam, CancellationToken, Task<T>>

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<TParam, CancellationToken, Task<T>> fallback, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, param, fallback, null, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<TParam, CancellationToken, Task<T>> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> func.InvokeWithFallbackAsync(param, fallback, policyParams, false, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<TParam, T>(this Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<TParam, CancellationToken, Task<T>> fallback, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token)
		{
			var fallbackProvider = FallbackFuncsProvider.Create().AddOrReplaceAsyncFallbackFunc(fallback);
			var policy = new FallbackPolicy(fallbackProvider);
			if (policyParams != null)
			{
				policy = (FallbackPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(policy);
			}
			return policy.HandleAsync(func, param, configureAwait, token);
		}
	}
}
