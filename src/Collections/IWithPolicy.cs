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
		public static T WithRetryInner<T>(this T t, int retryCount, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToRetryPolicy(retryCount));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, TimeSpan delay, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static T WithRetryInner<T>(this T t, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToInfiniteRetryPolicy());
		}

		public static T WithRetryInner<T>(this T t, TimeSpan delay, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static T WithRetryInner<T>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static T WithFallbackInner<T>(this T t, Action<CancellationToken> fallback, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallback));
		}

		public static T WithFallbackInner<T>(this T t, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static T WithFallbackInner<T>(this T t, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T>(this T t, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, U> fallbackAsync, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<U> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<Task<U>> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithSimpleInner<T>(this T t, InvokeParams policyParams = null) where T : IWithPolicy<T>
		{
			return t.ToWithPolicyFunc()(policyParams.ToSimplePolicy());
		}

		public static Func<IPolicyBase, T> ToWithPolicyFunc<T>(this T t) where T : IWithPolicy<T>
		{
			return (p) => t.WithPolicy(p);
		}
	}
}
