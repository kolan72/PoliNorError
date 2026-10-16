using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class DelegateInvoking
	{
		// Simple policy methods for Action<TParam>

		public static PolicyResult InvokeWithSimple<TParam>(this Action<TParam> action, TParam param, CancellationToken token = default)
			=> InvokeWithSimple(action, param, (ErrorProcessorParam)null, token);

		public static PolicyResult InvokeWithSimple<TParam>(this Action<TParam> action, TParam param, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithSimple(action, param, null, policyParams, token);

		public static PolicyResult InvokeWithSimple<TParam>(this Action<TParam> action, TParam param, CatchBlockFilter catchBlockFilter, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> policyParams.ToSimplePolicy(catchBlockFilter).Handle(action, param, token);

		public static PolicyResult InvokeWithSimple<TParam>(this Action<TParam> action, TParam param, CatchBlockHandler catchBlockHandler, CancellationToken token = default)
			=> new SimplePolicy(catchBlockHandler.CatchBlockFilter, catchBlockHandler.BulkErrorProcessor).Handle(action, param, token);

		// Retry policy methods for Action<TParam>

		public static PolicyResult InvokeWithRetry<TParam>(this Action<TParam> action, TParam param, int retryCount, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetry(action, param, retryCount, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetry<TParam>(this Action<TParam> action, TParam param, int retryCount, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows).Handle(action, param, token);

		// WaitAndRetry with TimeSpan delay for Action<TParam>

		public static PolicyResult InvokeWithWaitAndRetry<TParam>(this Action<TParam> action, TParam param, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(action, param, retryCount, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry<TParam>(this Action<TParam> action, TParam param, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows).Handle(action, param, token);

		// WaitAndRetry with retry function for Action<TParam>

		public static PolicyResult InvokeWithWaitAndRetry<TParam>(this Action<TParam> action, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetry(action, param, retryCount, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetry<TParam>(this Action<TParam> action, TParam param, int retryCount, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(action, param, token);

		// Infinite retry methods for Action<TParam>

		public static PolicyResult InvokeWithRetryInfinite<TParam>(this Action<TParam> action, TParam param, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithRetryInfinite(action, param, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithRetryInfinite<TParam>(this Action<TParam> action, TParam param, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows).Handle(action, param, token);

		// WaitAndRetryInfinite with TimeSpan delay for Action<TParam>

		public static PolicyResult InvokeWithWaitAndRetryInfinite<TParam>(this Action<TParam> action, TParam param, TimeSpan delay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfinite(action, param, delay, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite<TParam>(this Action<TParam> action, TParam param, TimeSpan delay, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows).Handle(action, param, token);

		// WaitAndRetryInfinite with retry function for Action<TParam>

		public static PolicyResult InvokeWithWaitAndRetryInfinite<TParam>(this Action<TParam> action, TParam param, Func<int, Exception, TimeSpan> retryFunc, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> InvokeWithWaitAndRetryInfinite(action, param, retryFunc, null, failedIfSaveErrorThrows, errorSaver, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite<TParam>(this Action<TParam> action, TParam param, Func<int, Exception, TimeSpan> retryFunc, ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null, CancellationToken token = default)
			=> policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc, errorSaver, failedIfSaveErrorThrows).Handle(action, param, token);

		// Fallback policy methods for Action<TParam>

		public static PolicyResult InvokeWithFallback<TParam>(this Action<TParam> action, TParam param, Action<TParam> fallback, CancellationToken token = default)
			=> InvokeWithFallback(action, param, fallback, null, token);

		public static PolicyResult InvokeWithFallback<TParam>(this Action<TParam> action, TParam param, Action<TParam> fallback, ErrorProcessorParam policyParams, CancellationToken token = default)
			=> InvokeWithFallback(action, param, fallback, policyParams, CancellationType.Precancelable, token);

		public static PolicyResult InvokeWithFallback<TParam>(this Action<TParam> action, TParam param, Action<TParam> fallback, ErrorProcessorParam policyParams, CancellationType convertType, CancellationToken token)
		{
			var fallbackProvider = FallbackFuncsProvider.Create().AddOrReplaceFallbackAction(fallback, convertType);
			var policy = new FallbackPolicy(fallbackProvider);
			if (policyParams != null)
			{
				policy = (FallbackPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(policy);
			}
			return policy.Handle(action, param, token);
		}
	}
}
