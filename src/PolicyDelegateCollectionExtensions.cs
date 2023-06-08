using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static PolicyDelegateCollection WithRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, InvokeParams<RetryPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicy(retryCount));
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams<RetryPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams<RetryPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static PolicyDelegateCollection WithInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, InvokeParams<RetryPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicy());
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, TimeSpan delay, InvokeParams<RetryPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams<RetryPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, InvokeParams<FallbackPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action fallback, InvokeParams<FallbackPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams<FallbackPolicy> policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, InvokeParams<FallbackPolicy> policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}
	}
}
