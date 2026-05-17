using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		// Simple policy methods for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, errorContext, (ErrorProcessorParam)null, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, errorContext, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, errorContext, null, policyParams, configureAwait, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, errorContext, catchBlockFilter, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).HandleAsync(func, errorContext, configureAwait, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, errorContext, catchBlockHandler, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, CatchBlockHandler catchBlockHandler, bool configureAwait, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).HandleAsync(func, errorContext, configureAwait, token);

		// Retry policy methods for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryAsync(func, errorContext, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryAsync(func, errorContext, retryCount, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, errorContext, configureAwait, token);

		// WaitAndRetry with TimeSpan delay for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, errorContext, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, errorContext, retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, errorContext, configureAwait, token);

		// WaitAndRetry with retry function for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, errorContext, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryAsync(func, errorContext, retryCount, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, errorContext, configureAwait, token);

		// Infinite retry methods for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryInfiniteAsync(func, errorContext, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).HandleAsync(func, errorContext, configureAwait, token);

		// WaitAndRetryInfinite with TimeSpan delay for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfiniteAsync(func, errorContext, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, errorContext, configureAwait, token);

		// WaitAndRetryInfinite with retry function for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfiniteAsync(func, errorContext, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, errorContext, configureAwait, token);

		// Fallback policy methods for Func<CancellationToken, Task> with TErrorContext

		public static Task<PolicyResult> InvokeWithFallbackAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, Func<CancellationToken, Task> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, errorContext, fallback, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, Func<CancellationToken, Task> fallback, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallback).HandleAsync(func, errorContext, configureAwait, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, Func<Task> fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, errorContext, fallback, policyParams, false, convertType, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync<TErrorContext>(this Func<CancellationToken, Task> func, TErrorContext errorContext, Func<Task> fallback, ErrorProcessorParam policyParams, bool configureAwait, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallback, convertType).HandleAsync(func, errorContext, configureAwait, token);
	}
}
