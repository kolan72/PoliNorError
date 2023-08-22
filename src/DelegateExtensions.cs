using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class DelegateExtensions
	{
		public static PolicyResult InvokeWithRetry(this Action action, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetry(action,  retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetry(this Action action, int retryCount, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).ConfigureBy(errorSaver).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetry(action, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, TimeSpan delay, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, failedIfSaveErrorThrows).ConfigureBy(errorSaver).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetry(action, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, Func<int, Exception, TimeSpan> retryFunc, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, failedIfSaveErrorThrows).ConfigureBy(errorSaver).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAsync(func, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAsync(func, retryCount, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows, RetryErrorSaver errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).ConfigureBy(errorSaver).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows, RetryErrorSaver errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, failedIfSaveErrorThrows).ConfigureBy(errorSaver).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows, RetryErrorSaver errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, failedIfSaveErrorThrows).ConfigureBy(errorSaver).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithRetryInfinite(this Action action, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfinite(action, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetryInfinite(this Action action, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).ConfigureBy(errorSaver).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfinite(action, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, TimeSpan delay, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay,failedIfSaveErrorThrows).ConfigureBy(errorSaver).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfinite(action, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, Func<int, Exception, TimeSpan> retryFunc, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, failedIfSaveErrorThrows).ConfigureBy(errorSaver).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfiniteAsync(func, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfiniteAsync(func, policyParams, failedIfSaveErrorThrows, errorSaver, false,  token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows, RetryErrorSaver errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).ConfigureBy(errorSaver).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows, RetryErrorSaver errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, failedIfSaveErrorThrows).ConfigureBy(errorSaver).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver,  false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows, RetryErrorSaver errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, failedIfSaveErrorThrows).ConfigureBy(errorSaver).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action<CancellationToken> fallback, CancellationToken token = default)
				=> InvokeWithFallback(action, fallback, null, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action<CancellationToken> fallback, PolicyErrorProcessor policyParams, CancellationToken token = default)
				=> policyParams.ToFallbackPolicy(fallback).Handle(action, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, CancellationToken token = default) => InvokeWithFallback(action, fallback, null, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, PolicyErrorProcessor policyParams, CancellationToken token = default)
			=> InvokeWithFallback(action, fallback, policyParams, CancellationType.Precancelable, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, PolicyErrorProcessor policyParams, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallback, convertType).Handle(action, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, null, CancellationType.Precancelable, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, PolicyErrorProcessor policyParams, CancellationType convertType = CancellationType.Precancelable, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, convertType, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, PolicyErrorProcessor policyParams, bool configureAwait, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallbackAsync, convertType).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, null, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, PolicyErrorProcessor policyParams, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, PolicyErrorProcessor policyParams, bool configureAwait, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallbackAsync).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithSimple(this Action action, CancellationToken token = default) => InvokeWithSimple(action, null, token);

		public static PolicyResult InvokeWithSimple(this Action action, PolicyErrorProcessor policyParams, CancellationToken token = default)
			=> policyParams.ToSimplePolicy().Handle(action, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, null, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, PolicyErrorProcessor policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, PolicyErrorProcessor policyParams, bool configureAwait, CancellationToken token)
			=> policyParams.ToSimplePolicy().HandleAsync(func, configureAwait, token);
	}
}
