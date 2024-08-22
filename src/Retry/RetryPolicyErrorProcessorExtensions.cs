using System;

namespace PoliNorError
{
	internal static class RetryPolicyErrorProcessorExtensions
	{
		public static RetryPolicy ToRetryPolicy(this ErrorProcessorParam policyParams, int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new RetryPolicy(retryCount, failedIfSaveErrorThrows, retryDelay));
		}

		public static RetryPolicy ToRetryPolicy(this ErrorProcessorParam policyParams, int retryCount, RetryDelay retryDelay, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToRetryPolicy(policyParams, retryCount, retryDelay, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToRetryPolicy(this ErrorProcessorParam policyParams, int retryCount, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new RetryPolicy(retryCount, failedIfSaveErrorThrows));
		}

		public static RetryPolicy ToRetryPolicy(this ErrorProcessorParam policyParams, int retryCount, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToRetryPolicy(policyParams, retryCount, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, int retryCount, TimeSpan delay, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).WithWait(delay).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).WithWait(delayOnRetryFunc).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(RetryPolicy.InfiniteRetries(failedIfSaveErrorThrows));
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this ErrorProcessorParam policyParams, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToInfiniteRetryPolicy(policyParams, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, TimeSpan delay, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).WithWait(delay).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).WithWait(delayOnRetryFunc).ConfigureBy(errorSaver);
		}
	}
}
