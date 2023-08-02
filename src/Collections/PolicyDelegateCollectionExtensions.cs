using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Func<CancellationToken, Task> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Action action) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(action));

		public static INeedDelegateCollection WithRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, policyParams);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delay, policyParams);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delayOnRetryFunc, policyParams);
		}

		public static INeedDelegateCollection WithInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delay, policyParams);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delayOnRetryFunc, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams, convertType);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams, convertType);
		}

		public static INeedDelegateCollection WithSimple(this IPolicyDelegateCollection policyDelegateCollection, InvokeParams policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams);
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(act.ToCancelableAction(convertType));
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(func.ToCancelableFunc(convertType));
		}
	}
}
