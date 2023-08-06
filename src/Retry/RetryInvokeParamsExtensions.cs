using System;

namespace PoliNorError
{
	internal static class RetryInvokeParamsExtensions
	{
		public static RetryPolicy ToRetryPolicy(this ErrorProcessorDelegate policyParams, int retryCount)
		{
			return (RetryPolicy)(policyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new RetryPolicy(retryCount));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorDelegate policyParams, int retryCount, TimeSpan delay)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this ErrorProcessorDelegate policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this ErrorProcessorDelegate policyParams)
		{
			return (RetryPolicy)(policyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(RetryPolicy.InfiniteRetries());
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorDelegate policyParams, TimeSpan delay)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this ErrorProcessorDelegate policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delayOnRetryFunc);
		}
	}
}
