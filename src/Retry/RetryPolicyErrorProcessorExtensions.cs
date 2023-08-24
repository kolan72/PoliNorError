using System;

namespace PoliNorError
{
	internal static class RetryPolicyErrorProcessorExtensions
	{
		public static RetryPolicy ToRetryPolicy(this ErrorProcessorParam policyParams, int retryCount, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new RetryPolicy(retryCount, failedIfSaveErrorThrows));
		}

		public static RetryPolicy ToRetryPolicy(this ErrorProcessorParam policyParams, int retryCount, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToRetryPolicy(policyParams, retryCount, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, int retryCount, TimeSpan delay, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToRetryPolicyWithDelayProcessorOf(policyParams, retryCount, delay, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToRetryPolicyWithDelayProcessorOf(policyParams, retryCount, delayOnRetryFunc, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this ErrorProcessorParam policyParams, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(RetryPolicy.InfiniteRetries(failedIfSaveErrorThrows));
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this ErrorProcessorParam policyParams, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToInfiniteRetryPolicy(policyParams, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, TimeSpan delay, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, TimeSpan delay, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToInfiniteRetryPolicyWithDelayProcessorOf(policyParams, delay, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorParam policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorSaverParam errorSaver, bool failedIfSaveErrorThrows = false)
		{
			return ToInfiniteRetryPolicyWithDelayProcessorOf(policyParams, delayOnRetryFunc, failedIfSaveErrorThrows).ConfigureBy(errorSaver);
		}
	}
}
