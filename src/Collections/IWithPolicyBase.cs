using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IWithPolicyBase<out T>
	{
		T WithPolicy(IPolicyBase policyBase);
	}

	internal static class IWithPolicyBaseExtensions
	{
		public static K WithRetryInner<T, K>(this T t, int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithFallbackInner<T, K>(this T t, Action fallback, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Action<CancellationToken> fallback, ErrorProcessorParam policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<U> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, U> fallbackAsync, ErrorProcessorParam policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<Task> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<Task<U>> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, ErrorProcessorParam policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithSimpleInner<T, K>(this T t, ErrorProcessorParam policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToSimplePolicy());
		}
	}
}
