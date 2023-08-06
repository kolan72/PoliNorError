using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyCollectionExtensions
	{
		public static PolicyCollection WithRetry(this PolicyCollection policyCollection, int retryCount, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithRetryInner(retryCount, policyParams);
		}

		public static PolicyCollection WithWaitAndRetry(this PolicyCollection policyCollection, int retryCount, TimeSpan delay, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithRetryInner(retryCount, delay, policyParams);
		}

		public static PolicyCollection WithWaitAndRetry(this PolicyCollection policyCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithRetryInner(retryCount, delayOnRetryFunc, policyParams);
		}

		public static PolicyCollection WithInfiniteRetry(this PolicyCollection policyCollection, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithRetryInner(policyParams);
		}

		public static PolicyCollection WithWaitAndInfiniteRetry(this PolicyCollection policyCollection, TimeSpan delay, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithRetryInner(delay, policyParams);
		}

		public static PolicyCollection WithWaitAndInfiniteRetry(this PolicyCollection policyCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithRetryInner(delayOnRetryFunc, policyParams);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Action<CancellationToken> fallback, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallback, policyParams);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Action fallback, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallback, policyParams, convertType);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallbackAsync, policyParams);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Func<Task> fallbackAsync, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallbackAsync, policyParams, convertType);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<CancellationToken, T> fallbackFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<T> fallbackFunc, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<CancellationToken, Task<T>> fallbackFunc, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<Task<T>> fallbackFunc, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		public static PolicyCollection WithSimple(this PolicyCollection policyCollection, ErrorProcessorDelegate policyParams = null)
		{
			return policyCollection.WithSimpleInner(policyParams);
		}

		public static PolicyDelegateCollectionResult HandleDelegate(this PolicyCollection policyCollection, Action action, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(action).Handle(token);
		}

		public static PolicyDelegateCollectionResult HandleDelegate(this PolicyCollection policyCollection, Func<CancellationToken, Task> func, CancellationToken token = default)
		{
			return  policyCollection.BuildCollectionHandlerFor(func).Handle(token);
		}

		public static PolicyDelegateCollectionResult<T> HandleDelegate<T>(this PolicyCollection policyCollection, Func<T> action, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(action).Handle(token);
		}

		public static PolicyDelegateCollectionResult<T> HandleDelegate<T>(this PolicyCollection policyCollection, Func<CancellationToken, Task<T>> func, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(func).Handle(token);
		}

		public static Task<PolicyDelegateCollectionResult> HandleDelegateAsync(this PolicyCollection policyCollection, Action action, CancellationToken token) => HandleDelegateAsync(policyCollection, action, false, token);

		public static Task<PolicyDelegateCollectionResult> HandleDelegateAsync(this PolicyCollection policyCollection, Action action, bool configAwait = false, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(action).HandleAsync(configAwait, token);
		}

		public static Task<PolicyDelegateCollectionResult> HandleDelegateAsync(this PolicyCollection policyCollection, Func<CancellationToken, Task> func, CancellationToken token) => HandleDelegateAsync(policyCollection, func, false, token);

		public static Task<PolicyDelegateCollectionResult> HandleDelegateAsync(this PolicyCollection policyCollection, Func<CancellationToken, Task> func, bool configAwait = false, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		public static Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(this PolicyCollection policyCollection, Func<T> func, CancellationToken token) => HandleDelegateAsync(policyCollection, func, false, token);

		public static Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(this PolicyCollection policyCollection, Func<T> func, bool configAwait = false, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		public static Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(this PolicyCollection policyCollection, Func<CancellationToken, Task<T>> func, CancellationToken token) => HandleDelegateAsync(policyCollection, func, false, token);

		public static Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(this PolicyCollection policyCollection, Func<CancellationToken, Task<T>> func, bool configAwait = false, CancellationToken token = default)
		{
			return policyCollection.BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}
	}
}
