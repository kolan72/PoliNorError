using System;

namespace PoliNorError
{
	internal static class RetryPolicyErrorProcessorExtensions
	{
		public static RetryPolicy ToRetryPolicy(this PolicyErrorProcessor policyParams, int retryCount, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new RetryPolicy(retryCount, failedIfSaveErrorThrows));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, int retryCount, TimeSpan delay, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToRetryPolicy(retryCount, failedIfSaveErrorThrows).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this PolicyErrorProcessor policyParams, bool failedIfSaveErrorThrows = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(RetryPolicy.InfiniteRetries(failedIfSaveErrorThrows));
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, TimeSpan delay, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc, bool failedIfSaveErrorThrows = false)
		{
			return policyParams.ToInfiniteRetryPolicy(failedIfSaveErrorThrows).WithWait(delayOnRetryFunc);
		}
	}
}
