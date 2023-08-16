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
		public static K WithRetryInner<T, K>(this T t, int retryCount, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicy(retryCount));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, TimeSpan delay, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static K WithRetryInner<T, K>(this T t, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicy());
		}

		public static K WithRetryInner<T, K>(this T t, TimeSpan delay, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static K WithRetryInner<T, K>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static K WithFallbackInner<T, K>(this T t, Action fallback, ErrorProcessorDelegate policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Action<CancellationToken> fallback, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<U> fallbackAsync, ErrorProcessorDelegate policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, U> fallbackAsync, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<Task> fallbackAsync, ErrorProcessorDelegate policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<Task<U>> fallbackAsync, ErrorProcessorDelegate policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithSimpleInner<T, K>(this T t, ErrorProcessorDelegate policyParams = null) where T : IWithPolicyBase<K>, IEnumerable<PolicyDelegateBase>
		{
			return t.WithPolicy(policyParams.ToSimplePolicy());
		}
	}
}
