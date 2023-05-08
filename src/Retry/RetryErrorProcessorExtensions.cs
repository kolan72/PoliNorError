using System;

namespace PoliNorError
{
	internal static class RetryErrorProcessorExtensions
	{
		public static RetryPolicy ToRetryPolicy(this RetryErrorProcessor policyParams, int retryCount)
		{
			return (policyParams ?? RetryErrorProcessor.Default()).ConfigurePolicy(new RetryPolicy(retryCount));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this RetryErrorProcessor policyParams, int retryCount, TimeSpan delay)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this RetryErrorProcessor policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this RetryErrorProcessor policyParams)
		{
			return (policyParams ?? RetryErrorProcessor.Default()).ConfigurePolicy(RetryPolicy.InfiniteRetries());
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this RetryErrorProcessor policyParams, TimeSpan delay)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this RetryErrorProcessor policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delayOnRetryFunc);
		}
	}
}
