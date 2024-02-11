using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IWithPolicy<out T> : IWithPolicyBase<T> where T : IWithPolicy<T>
	{
	}

	internal static class IWithPolicyExtensions
	{
		public static T WithRetryInner<T>(this T t, int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc, errorSaver, failedIfSaveErrorThrows));
		}

		public static T WithRetryInner<T>(this T t, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows));
		}

		public static T WithRetryInner<T>(this T t, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows));
		}

		public static T WithRetryInner<T>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc, errorSaver, failedIfSaveErrorThrows));
		}

		public static T WithFallbackInner<T>(this T t, Action<CancellationToken> fallback, ErrorProcessorParam policyParams = null, bool onlyGenericFallbackForGenericDelegate = false) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback, onlyGenericFallbackForGenericDelegate));
		}

		public static T WithFallbackInner<T>(this T t, Action fallback, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable, bool onlyGenericFallbackForGenericDelegate = false) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback, convertType, onlyGenericFallbackForGenericDelegate));
		}

		public static T WithFallbackInner<T>(this T t, Func<Task> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable, bool onlyGenericFallbackForGenericDelegate = false) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType, onlyGenericFallbackForGenericDelegate));
		}

		public static T WithFallbackInner<T>(this T t, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams = null, bool onlyGenericFallbackForGenericDelegate = false) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, onlyGenericFallbackForGenericDelegate));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, U> fallbackAsync, ErrorProcessorParam policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<U> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, ErrorProcessorParam policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<Task<U>> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithSimpleInner<T>(this T t, ErrorProcessorParam policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToSimplePolicy());
		}
	}
}
