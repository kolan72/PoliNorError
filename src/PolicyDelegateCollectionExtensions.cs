using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static PolicyDelegateCollection WithRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicy(retryCount));
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static PolicyDelegateCollection WithInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicy());
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action fallback, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static PolicyDelegateCollection WithSimple(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithPolicy(policyParams.ToSimplePolicy());
		}
	}
}
