using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		// Simple policy methods for Action with TErrorContext
		// These don't conflict because existing Simple methods don't have a second parameter before CancellationToken

		public static PolicyResult InvokeWithSimple<TErrorContext>(this Action action, TErrorContext errorContext, CancellationToken token = default)
			=> InvokeWithSimple(action, errorContext, (ErrorProcessorParam)null, token);

		public static PolicyResult InvokeWithSimple<TErrorContext>(this Action action, TErrorContext errorContext, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimple(action, errorContext, null, policyParams, token);

		public static PolicyResult InvokeWithSimple<TErrorContext>(this Action action, TErrorContext errorContext, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).Handle(action, errorContext, token);

		public static PolicyResult InvokeWithSimple<TErrorContext>(this Action action, TErrorContext errorContext, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).Handle(action, errorContext, token);

		// Retry policy methods for Action with TErrorContext
		// These have int retryCount as third parameter, making them distinct from existing methods

		public static PolicyResult InvokeWithRetry<TErrorContext>(this Action action, TErrorContext errorContext, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetry(action, errorContext, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetry<TErrorContext>(this Action action, TErrorContext errorContext, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).Handle(action, errorContext, token);

		// WaitAndRetry with TimeSpan delay for Action with TErrorContext
		// These have int retryCount as third parameter, making them distinct

		public static PolicyResult InvokeWithWaitAndRetry<TErrorContext>(this Action action, TErrorContext errorContext, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(action, errorContext, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry<TErrorContext>(this Action action, TErrorContext errorContext, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).Handle(action, errorContext, token);

		// WaitAndRetry with retry function for Action with TErrorContext
		// These have int retryCount as third parameter, making them distinct

		public static PolicyResult InvokeWithWaitAndRetry<TErrorContext>(this Action action, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(action, errorContext, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry<TErrorContext>(this Action action, TErrorContext errorContext, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(action, errorContext, token);

		// Infinite retry methods for Action with TErrorContext
		// Note: We need to be careful here to avoid conflicts with existing InvokeWithRetryInfinite methods
		// The existing methods have signatures like: InvokeWithRetryInfinite(Action, ErrorProcessorParam, ...)
		// Our methods have: InvokeWithRetryInfinite<TErrorContext>(Action, TErrorContext, ErrorProcessorParam, ...)
		// These are distinct because of the additional TErrorContext parameter

		public static PolicyResult InvokeWithRetryInfinite<TErrorContext>(this Action action, TErrorContext errorContext, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).Handle(action, errorContext, token);

		// WaitAndRetryInfinite with TimeSpan delay for Action with TErrorContext
		// The existing method has: InvokeWithWaitAndRetryInfinite(Action, TimeSpan, ErrorProcessorParam, ...)
		// Our method has: InvokeWithWaitAndRetryInfinite<TErrorContext>(Action, TErrorContext, TimeSpan, ErrorProcessorParam, ...)
		// These are distinct because of the additional TErrorContext parameter before TimeSpan

		public static PolicyResult InvokeWithWaitAndRetryInfinite<TErrorContext>(this Action action, TErrorContext errorContext, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).Handle(action, errorContext, token);

		// WaitAndRetryInfinite with retry function for Action with TErrorContext
		// The existing method has: InvokeWithWaitAndRetryInfinite(Action, Func<int, Exception, TimeSpan>, ErrorProcessorParam, ...)
		// Our method has: InvokeWithWaitAndRetryInfinite<TErrorContext>(Action, TErrorContext, Func<int, Exception, TimeSpan>, ErrorProcessorParam, ...)
		// These are distinct because of the additional TErrorContext parameter before the retry function

		public static PolicyResult InvokeWithWaitAndRetryInfinite<TErrorContext>(this Action action, TErrorContext errorContext, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(action, errorContext, token);

		// Fallback policy methods for Action with TErrorContext
		// The existing method has: InvokeWithFallback(Action, Action<CancellationToken>, ErrorProcessorParam, ...)
		// Our method has: InvokeWithFallback<TErrorContext>(Action, TErrorContext, Action<CancellationToken>, ErrorProcessorParam, ...)
		// These are distinct because of the additional TErrorContext parameter before the fallback

		public static PolicyResult InvokeWithFallback<TErrorContext>(this Action action, TErrorContext errorContext, Action<CancellationToken> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToFallbackPolicy(fallback).Handle(action, errorContext, token);

		public static PolicyResult InvokeWithFallback<TErrorContext>(this Action action, TErrorContext errorContext, Action fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token)
			=> policyParams.ToFallbackPolicy(fallback, convertType).Handle(action, errorContext, token);
	}
}
