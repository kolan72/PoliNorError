using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		// Simple policy methods for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithSimple<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, CancellationToken token = default)
			=> InvokeWithSimple(func, errorContext, (ErrorProcessorParam)null, token);

		public static PolicyResult<T> InvokeWithSimple<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimple(func, errorContext, null, policyParams, token);

		public static PolicyResult<T> InvokeWithSimple<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).Handle(func, errorContext, token);

		public static PolicyResult<T> InvokeWithSimple<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).Handle(func, errorContext, token);

		// Retry policy methods for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithRetry<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetry(func, errorContext, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithRetry<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).Handle(func, errorContext, token);

		// WaitAndRetry with TimeSpan delay for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithWaitAndRetry<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(func, errorContext, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).Handle(func, errorContext, token);

		// WaitAndRetry with retry function for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithWaitAndRetry<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(func, errorContext, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(func, errorContext, token);

		// Infinite retry methods for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithRetryInfinite<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).Handle(func, errorContext, token);

		// WaitAndRetryInfinite with TimeSpan delay for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).Handle(func, errorContext, token);

		// WaitAndRetryInfinite with retry function for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(func, errorContext, token);

		// Fallback policy methods for Func<T> with TErrorContext

		public static PolicyResult<T> InvokeWithFallback<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, Func<CancellationToken, T> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToFallbackPolicy(fallback).Handle(func, errorContext, token);

		public static PolicyResult<T> InvokeWithFallback<TErrorContext, T>(this Func<T> func, TErrorContext errorContext, Func<T> fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallback, convertType).Handle(func, errorContext, token);
	}
}
