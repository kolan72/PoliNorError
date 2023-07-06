using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static IPolicyNeedDelegateCollection WithRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(retryCount, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(retryCount, delay, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithWaitAndRetry(this PolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(retryCount, delayOnRetryFunc, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(policyParams);
		}

		public static IPolicyNeedDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(delay, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithWaitAndInfiniteRetry(this PolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(delayOnRetryFunc, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(fallback, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(fallback, policyParams, convertType);
		}

		public static IPolicyNeedDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(fallbackAsync, policyParams);
		}

		public static IPolicyNeedDelegateCollection WithFallback(this PolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(fallbackAsync, policyParams, convertType);
		}

		public static IPolicyNeedDelegateCollection WithSimple(this PolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<PolicyDelegateCollection, IPolicyNeedDelegateCollection>(policyParams);
		}
	}
}
