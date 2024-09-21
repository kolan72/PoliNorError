using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		public static PolicyResult<T> InvokeWithRetryDelay<T>(this Func<T> func, int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelay(func, retryCount, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetryDelay<T>(this Func<T> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicy(retryCount, retryDelay, errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithRetryDelayAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayAsync(func, retryCount, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithRetryDelayAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayAsync(func, retryCount, retryDelay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithRetryDelayAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicy(retryCount, retryDelay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithRetryDelayInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayInfiniteAsync(func, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithRetryDelayInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayInfiniteAsync(func, retryDelay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithRetryDelayInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicy(retryDelay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static PolicyResult<T> InvokeWithRetryDelayInfinite<T>(this Func<T> func, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayInfinite(func, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetryDelayInfinite<T>(this Func<T> func, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicy(retryDelay, errorSaver, failedIfSaveErrorThrows).Handle(func, token);
	}
}
