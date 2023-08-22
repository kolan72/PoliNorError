using System;

namespace PoliNorError
{
	internal static class RetryPolicyErrorProcessorExtensions
	{
		public static RetryPolicy ToRetryPolicy(this PolicyErrorProcessor policyParams, int retryCount, bool setFailedIfInvocationError = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new RetryPolicy(retryCount, setFailedIfInvocationError));
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, int retryCount, TimeSpan delay, bool setFailedIfInvocationError = false)
		{
			return policyParams.ToRetryPolicy(retryCount, setFailedIfInvocationError).WithWait(delay);
		}

		public static RetryPolicy ToRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, bool setFailedIfInvocationError = false)
		{
			return policyParams.ToRetryPolicy(retryCount, setFailedIfInvocationError).WithWait(delayOnRetryFunc);
		}

		public static RetryPolicy ToInfiniteRetryPolicy(this PolicyErrorProcessor policyParams, bool setFailedIfInvocationError = false)
		{
			return (RetryPolicy)policyParams.GetValueOrDefault().ConfigurePolicy(RetryPolicy.InfiniteRetries(setFailedIfInvocationError));
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, TimeSpan delay, bool setFailedIfInvocationError = false)
		{
			return policyParams.ToInfiniteRetryPolicy(setFailedIfInvocationError).WithWait(delay);
		}

		public static RetryPolicy ToInfiniteRetryPolicyWithDelayProcessorOf(this PolicyErrorProcessor policyParams, Func<int, Exception, TimeSpan> delayOnRetryFunc, bool setFailedIfInvocationError = false)
		{
			return policyParams.ToInfiniteRetryPolicy(setFailedIfInvocationError).WithWait(delayOnRetryFunc);
		}
	}
}
