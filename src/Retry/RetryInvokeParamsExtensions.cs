using System;

namespace PoliNorError
{
	internal static class RetryInvokeParamsExtensions
	{
		public static RetryPolicy ToRetryPolicy(this InvokeParams<RetryPolicy> policyParams, int retryCount)
		{
			return (policyParams ?? InvokeParams<RetryPolicy>.Default()).ConfigurePolicy(new RetryPolicy(retryCount));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this InvokeParams<RetryPolicy> policyParams, int retryCount, TimeSpan delay)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this InvokeParams<RetryPolicy> policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this InvokeParams<RetryPolicy> policyParams)
		{
			return (policyParams ?? InvokeParams<RetryPolicy>.Default()).ConfigurePolicy(RetryPolicy.InfiniteRetries());
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this InvokeParams<RetryPolicy> policyParams, TimeSpan delay)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this InvokeParams<RetryPolicy> policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delayOnRetryFunc);
		}
	}
}
