using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateTCollectionExtensions
	{
		public static IPolicyNeedDelegateCollection<T> WithRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(retryCount, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithWaitAndRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(retryCount, delay, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithWaitAndRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(retryCount, delayOnRetryFunc, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(delay, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(delayOnRetryFunc, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, T> fallbackFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>, T>(fallbackFunc, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<T> fallbackFunc, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>, T>(fallbackFunc, policyParams, convertType);
		}

		public static IPolicyNeedDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, Task<T>> fallbackFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>,T>(fallbackFunc, policyParams);
		}

		public static IPolicyNeedDelegateCollection<T> WithFallback<T>(this PolicyDelegateCollection<T> policyDelegateCollection, Func<Task<T>> fallbackFunc, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>, T>(fallbackFunc, policyParams, convertType);
		}

		public static IPolicyNeedDelegateCollection<T> WithSimple<T>(this PolicyDelegateCollection<T> policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<PolicyDelegateCollection<T>, IPolicyNeedDelegateCollection<T>>(policyParams);
		}
	}
}
