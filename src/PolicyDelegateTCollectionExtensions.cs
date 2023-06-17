using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateTCollectionExtensions
	{
		public static PolicyDelegateCollection<T> WithRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(retryCount, policyParams);
		}

		public static PolicyDelegateCollection<T> WithWaitAndRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(retryCount, delay, policyParams);
		}

		public static PolicyDelegateCollection<T> WithWaitAndRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(retryCount, delayOnRetryFunc, policyParams);
		}

		public static PolicyDelegateCollection<T> WithInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(policyParams);
		}

		public static PolicyDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(delay, policyParams);
		}

		public static PolicyDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(delayOnRetryFunc, policyParams);
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, T> fallbackFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallbackFunc, policyParams);
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<T> fallbackFunc, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallbackFunc, policyParams, convertType);
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, Task<T>> fallbackFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallbackFunc, policyParams);
		}

		public static PolicyDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<Task<T>> fallbackFunc, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallbackFunc, policyParams, convertType);
		}

		public static PolicyDelegateCollection<T> WithSimple<T>(this PolicyDelegateCollection<T> policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithSimple(policyParams);
		}

		internal static Func<IPolicyBase, PolicyDelegateCollection<T>> ToWithPolicyFunc<T>(this PolicyDelegateCollection<T> policyDelegateCollection)
		{
			return policyDelegateCollection.WithPolicy;
		}
	}
}
