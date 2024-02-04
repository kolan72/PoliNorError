using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class PolicyCollection
	{
		public PolicyCollection WithRetry(int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public PolicyCollection WithWaitAndRetry(int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public PolicyCollection WithWaitAndRetry(int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public PolicyCollection WithInfiniteRetry(ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public PolicyCollection WithWaitAndInfiniteRetry(TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public PolicyCollection WithWaitAndInfiniteRetry(Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public PolicyCollection WithFallback(Action<CancellationToken> fallback, ErrorProcessorParam policyParams = null)
		{
			return WithFallback(fallback, false, policyParams);
		}

		public PolicyCollection WithFallback(Action<CancellationToken> fallback, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallback, policyParams, onlyGenericFallbackForGenericDelegate);
		}

		public PolicyCollection WithFallback(Action fallback, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return WithFallback(fallback, false, policyParams, convertType);
		}

		public PolicyCollection WithFallback(Action fallback, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallback, policyParams, convertType, onlyGenericFallbackForGenericDelegate);
		}

		public PolicyCollection WithFallback(Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams = null)
		{
			return WithFallback(fallbackAsync, false, policyParams);
		}

		public PolicyCollection WithFallback(Func<CancellationToken, Task> fallbackAsync, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallbackAsync, policyParams, onlyGenericFallbackForGenericDelegate);
		}

		public PolicyCollection WithFallback(Func<Task> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return WithFallback(fallbackAsync, false, policyParams, convertType);
		}

		public PolicyCollection WithFallback(Func<Task> fallbackAsync, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallbackAsync, policyParams, convertType, onlyGenericFallbackForGenericDelegate);
		}

		public PolicyCollection WithFallback<T>(Func<CancellationToken, T> fallbackFunc, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams);
		}

		public PolicyCollection WithFallback<T>(Func<T> fallbackFunc, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		public PolicyCollection WithFallback<T>(Func<CancellationToken, Task<T>> fallbackFunc, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams);
		}

		public PolicyCollection WithFallback<T>(Func<Task<T>> fallbackFunc, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		public PolicyCollection WithSimple(ErrorProcessorParam policyParams = null)
		{
			return this.WithSimpleInner(policyParams);
		}
	}
}
