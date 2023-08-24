using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of static methods for calling delegates in a resilient manner
	/// </summary>
	public static partial class DelegateInvoking
	{
		public static PolicyResult<T> InvokeWithRetry<T>(this Func<T> func, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default) => InvokeWithRetry(func, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetry<T>(this Func<T> func, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetry(func, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetry(func, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAsync(func, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAsync(func, retryCount, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static PolicyResult<T> InvokeWithRetryInfinite<T>(this Func<T> func, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfinite(func, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetryInfinite<T>(this Func<T> func, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfinite(func, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfinite(func, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfiniteAsync(func, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfiniteAsync(func, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<CancellationToken, T> fallback, CancellationToken token = default)
				=> InvokeWithFallback(func, fallback, null, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<CancellationToken, T> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
				=> policyParams.ToFallbackPolicy(fallback).Handle(func, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<T> fallback, CancellationToken token = default)
				=> InvokeWithFallback(func, fallback, null, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<T> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
				=> InvokeWithFallback(func, fallback, policyParams, CancellationType.Precancelable, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<T> fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token)
				=> policyParams.ToFallbackPolicy(fallback, convertType).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> funcAsync, Func<Task<T>> fallbackAsync, CancellationToken token = default)
				=> InvokeWithFallbackAsync(funcAsync, fallbackAsync, null, CancellationType.Precancelable, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> funcAsync, Func<Task<T>> fallbackAsync, ErrorProcessorParam policyParams, CancellationType convertType = CancellationType.Precancelable, CancellationToken token = default)
				=> InvokeWithFallbackAsync(funcAsync, fallbackAsync, policyParams, false, convertType, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> funcAsync, Func<Task<T>> fallbackAsync, ErrorProcessorParam policyParams, bool configureAwait, CancellationType convertType, CancellationToken token)
				=> policyParams.ToFallbackPolicy(fallbackAsync, convertType).HandleAsync(funcAsync, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallbackAsync, CancellationToken token = default)
				=> InvokeWithFallbackAsync(func, fallbackAsync, null, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallbackAsync, ErrorProcessorParam policyParams, CancellationToken token = default)
				=> InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallbackAsync, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token)
				=> policyParams.ToFallbackPolicy(fallbackAsync).HandleAsync(func, configureAwait, token);

		public static PolicyResult<T> InvokeWithSimple<T>(this Func<T> func, CancellationToken token = default) => InvokeWithSimple(func, null, token);

		public static PolicyResult<T> InvokeWithSimple<T>(this Func<T> func, ErrorProcessorParam policyParams, CancellationToken token = default)
				=> policyParams.ToSimplePolicy().Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<T>(this Func<CancellationToken, Task<T>> func, CancellationToken token = default)
				=> InvokeWithSimpleAsync(func, null, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<T>(this Func<CancellationToken, Task<T>> func, ErrorProcessorParam policyParams, CancellationToken token = default)
				=> InvokeWithSimpleAsync(func, policyParams, false, token);

		public static Task<PolicyResult<T>> InvokeWithSimpleAsync<T>(this Func<CancellationToken, Task<T>> func, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token)
				=> policyParams.ToSimplePolicy().HandleAsync(func, configureAwait, token);
	}
}
