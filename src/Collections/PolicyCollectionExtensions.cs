using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyCollectionExtensions
	{
		public static PolicyCollection WithRetry(this PolicyCollection policyCollection, int retryCount, InvokeParams policyParams = null)
		{
			return policyCollection.WithRetryInner(retryCount, policyParams);
		}

		public static PolicyCollection WithWaitAndRetry(this PolicyCollection policyCollection, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyCollection.WithRetryInner(retryCount, delay, policyParams);
		}

		public static PolicyCollection WithWaitAndRetry(this PolicyCollection policyCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyCollection.WithRetryInner(retryCount, delayOnRetryFunc, policyParams);
		}

		public static PolicyCollection WithInfiniteRetry(this PolicyCollection policyCollection, InvokeParams policyParams = null)
		{
			return policyCollection.WithRetryInner(policyParams);
		}

		public static PolicyCollection WithWaitAndInfiniteRetry(this PolicyCollection policyCollection, TimeSpan delay, InvokeParams policyParams = null)
		{
			return policyCollection.WithRetryInner(delay, policyParams);
		}

		public static PolicyCollection WithWaitAndInfiniteRetry(this PolicyCollection policyCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return policyCollection.WithRetryInner(delayOnRetryFunc, policyParams);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallback, policyParams);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallback, policyParams, convertType);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallbackAsync, policyParams);
		}

		public static PolicyCollection WithFallback(this PolicyCollection policyCollection, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallbackAsync, policyParams, convertType);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<CancellationToken, T> fallbackFunc, InvokeParams policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<T> fallbackFunc, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<CancellationToken, Task<T>> fallbackFunc, InvokeParams policyParams = null)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams);
		}

		public static PolicyCollection WithFallback<T>(this PolicyCollection policyCollection, Func<Task<T>> fallbackFunc, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return policyCollection.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		public static PolicyCollection WithSimple(this PolicyCollection policyCollection, InvokeParams policyParams = null)
		{
			return policyCollection.WithSimpleInner(policyParams);
		}
	}
}
