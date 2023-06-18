using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class WithPolicyFuncExtensions
	{
		public static T WithRetry<T>(this Func<IPolicyBase, T> func, int retryCount, InvokeParams policyParams = null)
		{
			return func(policyParams.ToRetryPolicy(retryCount));
		}

		public static T WithRetry<T>(this Func<IPolicyBase, T> func, int retryCount, TimeSpan delay, InvokeParams policyParams = null)
		{
			return func(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static T WithRetry<T>(this Func<IPolicyBase, T> func, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return func(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static T WithRetry<T>(this Func<IPolicyBase, T> func, InvokeParams policyParams = null)
		{
			return func(policyParams.ToInfiniteRetryPolicy());
		}

		public static T WithRetry<T>(this Func<IPolicyBase, T> func, TimeSpan delay, InvokeParams policyParams = null)
		{
			return func(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static T WithRetry<T>(this Func<IPolicyBase, T> func, Func<int, Exception, TimeSpan> delayOnRetryFunc, InvokeParams policyParams = null)
		{
			return func(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static T WithFallback<T>(this Func<IPolicyBase, T> func, Action<CancellationToken> fallback, InvokeParams policyParams = null)
		{
			return func(policyParams.ToFallbackPolicy(fallback));
		}

		public static T WithFallback<T>(this Func<IPolicyBase, T> func, Action fallback, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return func(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static T WithFallback<T>(this Func<IPolicyBase, T> func, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams = null)
		{
			return func(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallback<T>(this Func<IPolicyBase, T> func, Func<Task> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return func(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallback<T, U>(this Func<IPolicyBase, T> func, Func<CancellationToken, U> fallbackAsync, InvokeParams policyParams = null)
		{
			return func(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallback<T, U>(this Func<IPolicyBase, T> func, Func<U> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return func(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallback<T, U>(this Func<IPolicyBase, T> func, Func<CancellationToken, Task<U>> fallbackAsync, InvokeParams policyParams = null)
		{
			return func(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallback<T, U>(this Func<IPolicyBase, T> func, Func<Task<U>> fallbackAsync, InvokeParams policyParams = null, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return func(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithSimple<T>(this Func<IPolicyBase, T> func, InvokeParams policyParams = null)
		{
			return func(policyParams.ToSimplePolicy());
		}
	}
}
