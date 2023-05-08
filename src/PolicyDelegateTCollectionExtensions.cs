using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateTCollectionExtensions
	{
		public static PolicyDelegateCollection<T> WithPolicy<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<IPolicyBase> func)
		{
			return policyDelegateCollection.WithPolicy(func());
		}

        public static PolicyDelegateCollection<T> WithRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, RetryErrorProcessor policyParams = null)
        {
            return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicy(retryCount));
        }

		public static PolicyDelegateCollection<T> WithWaitAndRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, TimeSpan delay, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static PolicyDelegateCollection<T> WithWaitAndRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static PolicyDelegateCollection<T> WithInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicy());
		}

		public static PolicyDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, TimeSpan delay, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static PolicyDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, RetryErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, T> fallbackFunc, FallbackErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackFunc));
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<T> fallbackFunc, FallbackErrorProcessor policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackFunc, convertType));
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, Task<T>> fallbackFunc, FallbackErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackFunc));
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<Task<T>> fallbackFunc, FallbackErrorProcessor policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackFunc, convertType));
		}
	}
}
