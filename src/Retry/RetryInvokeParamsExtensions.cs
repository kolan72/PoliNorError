using System;

namespace PoliNorError
{
	internal static class RetryInvokeParamsExtensions
	{
		public static RetryPolicy ToRetryPolicy(this InvokeParams policyParams, int retryCount)
		{
			return (RetryPolicy)(policyParams ?? InvokeParams.Default()).ConfigurePolicy(new RetryPolicy(retryCount));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this InvokeParams policyParams, int retryCount, TimeSpan delay)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this InvokeParams policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this InvokeParams policyParams)
		{
			return (RetryPolicy)(policyParams ?? InvokeParams.Default()).ConfigurePolicy(RetryPolicy.InfiniteRetries());
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this InvokeParams policyParams, TimeSpan delay)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this InvokeParams policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delayOnRetryFunc);
		}
	}
}
