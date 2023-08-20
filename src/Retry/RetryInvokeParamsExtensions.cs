using System;

namespace PoliNorError
{
	internal static class RetryInvokeParamsExtensions
	{
		public static RetryPolicy ToRetryPolicy(this PolicyErrorProcessor policyParams, int retryCount)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new RetryPolicy(retryCount));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, int retryCount, TimeSpan delay)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToRetryPolicy(retryCount).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this PolicyErrorProcessor policyParams)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(RetryPolicy.InfiniteRetries());
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, TimeSpan delay)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc)
		{
			return policyParams.ToInfiniteRetryPolicy().WithWait(delayOnRetryFunc);
		}
	}
}
