using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static PolicyDelegateCollection WithRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner(retryCount, policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner(retryCount, delay, policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner(retryCount, delayOnRetryFunc, policyParams);
		}

		public static PolicyDelegateCollection WithInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner(policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner(delay, policyParams);
		}

		public static PolicyDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner(delayOnRetryFunc, policyParams);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner(fallback, policyParams);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner(fallback, policyParams, convertType);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner(fallbackAsync, policyParams);
		}

		public static PolicyDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner(fallbackAsync, policyParams, convertType);
		}

		public static PolicyDelegateCollection WithSimple(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner(policyParams);
		}
	}
}
