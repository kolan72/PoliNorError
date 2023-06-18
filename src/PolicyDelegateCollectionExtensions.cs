using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static PolicyDelegateCollection WithRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(retryCount, policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(retryCount, delay, policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(retryCount, delayOnRetryFunc, policyParams);
		}

		public static PolicyDelegateCollection WithInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(delay, policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithRetry(delayOnRetryFunc, policyParams);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallback, policyParams);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallback, policyParams, convertType);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallbackAsync, policyParams);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithFallback(fallbackAsync, policyParams, convertType);
		}

		public static PolicyDelegateCollection WithSimple(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.ToWithPolicyFunc().WithSimple(policyParams);
		}

		internal static Func<IPolicyBase, PolicyDelegateCollection> ToWithPolicyFunc(this PolicyDelegateCollection policyDelegateCollection)
		{
			return policyDelegateCollection.WithPolicy;
		}
	}
}
