using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static PolicyDelegateCollection WithRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicy(retryCount));
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static PolicyDelegateCollection WithInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicy());
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, TimeSpan delay, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, FallbackErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action fallback, FallbackErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, FallbackErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, FallbackErrorProcessor policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}
	}
}
