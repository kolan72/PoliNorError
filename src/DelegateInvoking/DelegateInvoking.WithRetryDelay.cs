using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		public static PolicyResult InvokeWithRetryDelay(this Action action, int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelay(action, retryCount, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetryDelay(this Action action, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicy(retryCount, retryDelay, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryDelayAsync(this Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayAsync(func, retryCount, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryDelayAsync(this Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayAsync(func, retryCount, retryDelay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryDelayAsync(this Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicy(retryCount, retryDelay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithRetryDelayInfinite(this Action action, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayInfinite(action, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetryDelayInfinite(this Action action, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicy(retryDelay, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryDelayInfiniteAsync(this Func<CancellationToken, Task> func, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayInfiniteAsync(func, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryDelayInfiniteAsync(this Func<CancellationToken, Task> func, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryDelayInfiniteAsync(func, retryDelay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryDelayInfiniteAsync(this Func<CancellationToken, Task> func, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicy(retryDelay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);
	}
}
