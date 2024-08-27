using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		public static PolicyResult InvokeWithRetry(this Action action, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetry(action,  retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetry(this Action action, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetry(action, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetry(action, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static PolicyResult InvokeWithRetryAndDelay(this Action action, int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAndDelay(action, retryCount, retryDelay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetryAndDelay(this Action action, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToRetryPolicy(retryCount, retryDelay, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAsync(func, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAsync(func, retryCount, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithRetryAndDelayAsync(this Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryAndDelayAsync(func, retryCount, retryDelay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithRetryAndDelayAsync(this Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
			=> policyParams.ToRetryPolicy(retryCount, retryDelay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithRetryInfinite(this Action action, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfinite(action, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetryInfinite(this Action action, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfinite(action, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfinite(action, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static PolicyResult InvokeWithRetryAndDelayInfinite(this Action action, RetryDelay retryDelay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> policyParams.ToInfiniteRetryPolicy(retryDelay, errorSaver, failedIfSaveErrorThrows).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfiniteAsync(func, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithRetryInfiniteAsync(func, policyParams, failedIfSaveErrorThrows, errorSaver, false,  token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, delay, policyParams, failedIfSaveErrorThrows, errorSaver, false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
				=> InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, policyParams, failedIfSaveErrorThrows, errorSaver,  false, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows, RetryErrorSaverParam errorSaver, bool configureAwait, CancellationToken token)
				=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action<CancellationToken> fallback, CancellationToken token = default)
				=> InvokeWithFallback(action, fallback, null, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action<CancellationToken> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
				=> policyParams.ToFallbackPolicy(fallback).Handle(action, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, CancellationToken token = default) => InvokeWithFallback(action, fallback, null, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithFallback(action, fallback, policyParams, CancellationType.Precancelable, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallback, convertType).Handle(action, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, null, CancellationType.Precancelable, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, ErrorProcessorParam policyParams, CancellationType convertType = CancellationType.Precancelable, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, convertType, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, ErrorProcessorParam policyParams, bool configureAwait, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallbackAsync, convertType).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, null, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallbackAsync).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithSimple(this Action action, CancellationToken token = default) => InvokeWithSimple(action, (ErrorProcessorParam)null, token);

		public static PolicyResult InvokeWithSimple(this Action action, ErrorProcessorParam policyParams, CancellationToken token = default) => InvokeWithSimple(action, null, policyParams, token);

		public static PolicyResult InvokeWithSimple(this Action action, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).Handle(action, token);

		public static PolicyResult InvokeWithSimple(this Action action, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).Handle(action, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, (ErrorProcessorParam)null, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, null, policyParams, configureAwait, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, catchBlockFilter, policyParams, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, bool configureAwait, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> InvokeWithSimpleAsync(func, catchBlockHandler, false, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CatchBlockHandler catchBlockHandler, bool configureAwait, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).HandleAsync(func, configureAwait, token);
	}
}
