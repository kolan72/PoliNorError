using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateTCollectionExtensions
	{
		public static IPolicyDelegateCollection<T> WithPolicyAndDelegate<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, IPolicyBase errorPolicy, Func<CancellationToken, Task<T>> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static IPolicyDelegateCollection<T> WithPolicyAndDelegate<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, IPolicyBase errorPolicy, Func<T> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static INeedDelegateCollection<T> WithRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, int retryCount, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(retryCount, policyParams);
		}

		public static INeedDelegateCollection<T> WithWaitAndRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, int retryCount, TimeSpan delay, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(retryCount, delay, policyParams);
		}

		public static INeedDelegateCollection<T> WithWaitAndRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(retryCount, delayOnRetryFunc, policyParams);
		}

		public static INeedDelegateCollection<T> WithInfiniteRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(policyParams);
		}

		public static INeedDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, TimeSpan delay, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(delay, policyParams);
		}

		public static INeedDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(delayOnRetryFunc, policyParams);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, T> fallbackFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<T> fallbackFunc, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams, convertType);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, Task<T>> fallbackFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>,T>(fallbackFunc, policyParams);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<Task<T>> fallbackFunc, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams, convertType);
		}

		public static INeedDelegateCollection<T> WithSimple<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, ErrorProcessorDelegate policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(policyParams);
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(act.ToCancelableAction(convertType));
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(func.ToCancelableFunc(convertType));
		}

		public static PolicyDelegateCollectionResult<T> HandleAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().Handle(token);
		}

		public static Task<PolicyDelegateCollectionResult<T>> HandleAllAsync<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, CancellationToken token) => HandleAllAsync(policyDelegateCollection, false, token);

		public static Task<PolicyDelegateCollectionResult<T>> HandleAllAsync<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, bool configAwait = false, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().HandleAsync(configAwait, token);
		}
	}
}
