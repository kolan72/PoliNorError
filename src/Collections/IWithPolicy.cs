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
		public static T WithRetryInner<T>(this T t, int retryCount, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToRetryPolicy(retryCount));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, TimeSpan delay, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static T WithRetryInner<T>(this T t, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToInfiniteRetryPolicy());
		}

		public static T WithRetryInner<T>(this T t, TimeSpan delay, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static T WithRetryInner<T>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static T WithFallbackInner<T>(this T t, Action<CancellationToken> fallback, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallback));
		}

		public static T WithFallbackInner<T>(this T t, Action fallback, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static T WithFallbackInner<T>(this T t, Func<Task> fallbackAsync, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T>(this T t, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, U> fallbackAsync, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<U> fallbackAsync, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<Task<U>> fallbackAsync, ErrorProcessorDelegate policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithSimpleInner<T>(this T t, ErrorProcessorDelegate policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToSimplePolicy());
		}

		public static Func<IPolicyBase, T> ToWithPolicyFunc<T>(this T t) where T : IWithPolicy<T>
		{
			return (p) => t.WithPolicy(p);
		}
	}
}
