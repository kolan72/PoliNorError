using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateCollectionExtensions
	{
		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Func<CancellationToken, Task> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Action action) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(action));

		public static INeedDelegateCollection WithRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, TimeSpan delay, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, PolicyErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action fallback, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams, convertType);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, PolicyErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams, convertType);
		}

		public static INeedDelegateCollection WithSimple(this IPolicyDelegateCollection policyDelegateCollection, PolicyErrorProcessor policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams);
		}

		public static PolicyDelegateCollectionResult HandleAll(this IPolicyDelegateCollection policyDelegateCollection, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().Handle(token);
		}

		public static Task<PolicyDelegateCollectionResult> HandleAllAsync(this IPolicyDelegateCollection policyDelegateCollection, CancellationToken token) => HandleAllAsync(policyDelegateCollection, false, token);

		public static Task<PolicyDelegateCollectionResult> HandleAllAsync(this IPolicyDelegateCollection policyDelegateCollection, bool configAwait = false, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().HandleAsync(configAwait, token);
		}
	}
}
