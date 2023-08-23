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
		public static K WithRetryInner<T, K>(this T t, int retryCount, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicy(retryCount, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, TimeSpan delay, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicy(errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, TimeSpan delay, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithRetryInner<T, K>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, PolicyErrorProcessor policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaver errorSaver = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc, errorSaver, failedIfSaveErrorThrows));
		}

		public static K WithFallbackInner<T, K>(this T t, Action fallback, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Action<CancellationToken> fallback, PolicyErrorProcessor policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<U> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, U> fallbackAsync, PolicyErrorProcessor policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<Task> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<CancellationToken, Task> fallbackAsync, PolicyErrorProcessor policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<Task<U>> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, PolicyErrorProcessor policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithSimpleInner<T, K>(this T t, PolicyErrorProcessor policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToSimplePolicy());
		}
	}
}
