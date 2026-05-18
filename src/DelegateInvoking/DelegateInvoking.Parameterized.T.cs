using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		// Simple policy methods for Func<TParam, T>

		public static PolicyResult<T> InvokeWithSimple<TParam, T>(this Func<TParam, T> func, TParam param, CancellationToken token = default)
			=> InvokeWithSimple(func, param, (ErrorProcessorParam)null, token);

		public static PolicyResult<T> InvokeWithSimple<TParam, T>(this Func<TParam, T> func, TParam param, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimple(func, param, null, policyParams, token);

		public static PolicyResult<T> InvokeWithSimple<TParam, T>(this Func<TParam, T> func, TParam param, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).Handle(func, param, token);

		public static PolicyResult<T> InvokeWithSimple<TParam, T>(this Func<TParam, T> func, TParam param, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).Handle(func, param, token);

		// Retry policy methods for Func<TParam, T>

		public static PolicyResult<T> InvokeWithRetry<TParam, T>(this Func<TParam, T> func, TParam param, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetry(func, param, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetry<TParam, T>(this Func<TParam, T> func, TParam param, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).Handle(func, param, token);

		// WaitAndRetry with TimeSpan delay for Func<TParam, T>

		public static PolicyResult<T> InvokeWithWaitAndRetry<TParam, T>(this Func<TParam, T> func, TParam param, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(func, param, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<TParam, T>(this Func<TParam, T> func, TParam param, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).Handle(func, param, token);

		// WaitAndRetry with retry function for Func<TParam, T>

		public static PolicyResult<T> InvokeWithWaitAndRetry<TParam, T>(this Func<TParam, T> func, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(func, param, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<TParam, T>(this Func<TParam, T> func, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(func, param, token);

		// Infinite retry methods for Func<TParam, T>

		public static PolicyResult<T> InvokeWithRetryInfinite<TParam, T>(this Func<TParam, T> func, TParam param, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryInfinite(func, param, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetryInfinite<TParam, T>(this Func<TParam, T> func, TParam param, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).Handle(func, param, token);

		// WaitAndRetryInfinite with TimeSpan delay for Func<TParam, T>

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<TParam, T>(this Func<TParam, T> func, TParam param, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfinite(func, param, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<TParam, T>(this Func<TParam, T> func, TParam param, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).Handle(func, param, token);

		// WaitAndRetryInfinite with retry function for Func<TParam, T>

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<TParam, T>(this Func<TParam, T> func, TParam param, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfinite(func, param, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<TParam, T>(this Func<TParam, T> func, TParam param, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(func, param, token);

		// Fallback policy methods for Func<TParam, T>

		public static PolicyResult<T> InvokeWithFallback<TParam, T>(this Func<TParam, T> func, TParam param, Func<TParam, T> fallback, CancellationToken token = default)
			=> InvokeWithFallback(func, param, fallback, null, token);

		public static PolicyResult<T> InvokeWithFallback<TParam, T>(this Func<TParam, T> func, TParam param, Func<TParam, T> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithFallback(func, param, fallback, policyParams, CancellationType.Precancelable, token);

		public static PolicyResult<T> InvokeWithFallback<TParam, T>(this Func<TParam, T> func, TParam param, Func<TParam, T> fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token)
		{
			var fallbackProvider = FallbackFuncsProvider.Create().AddOrReplaceFallbackFunc(fallback, convertType);
			var policy = new FallbackPolicy(fallbackProvider);
			if (policyParams != null)
			{
				policy = (FallbackPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(policy);
			}
			return policy.Handle(func, param, token);
		}
	}
}
