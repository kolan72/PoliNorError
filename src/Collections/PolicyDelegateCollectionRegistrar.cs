using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of static methods to add policy or policy and a delegate to <see cref="IPolicyDelegateCollection"/> or <see cref="IPolicyDelegateCollection{T}"/>.
	/// </summary>
	public static partial class PolicyDelegateCollectionRegistrar
	{
		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Func<CancellationToken, Task> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Action action) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(action));

		public static INeedDelegateCollection WithRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action fallback, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams, convertType);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams, convertType);
		}

		public static INeedDelegateCollection WithSimple(this IPolicyDelegateCollection policyDelegateCollection, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams);
		}
	}
}
