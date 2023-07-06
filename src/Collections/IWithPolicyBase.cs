using System;
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
		public static K WithRetryInner<T, K>(this T t, int retryCount, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToRetryPolicy(retryCount));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, TimeSpan delay, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static K WithRetryInner<T, K>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static K WithRetryInner<T, K>(this T t, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToInfiniteRetryPolicy());
		}

		public static K WithRetryInner<T, K>(this T t, TimeSpan delay, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static K WithRetryInner<T, K>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static K WithFallbackInner<T, K>(this T t, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Action<CancellationToken> fallback, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallback));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<U> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, U> fallbackAsync, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K>(this T t, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<Task<U>> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static K WithFallbackInner<T, K, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static K WithSimpleInner<T, K>(this T t, InvokeParams policyParams = null) where T : IWithPolicyBase<K>
		{
			return t.ToWithPolicyFunc<T, K>()(policyParams.ToSimplePolicy());
		}

		public static Func<IPolicyBase, K> ToWithPolicyFunc<T, K>(this T t) where T : IWithPolicyBase<K>
		{
			return (p) => t.WithPolicy(p);
		}
	}
}
