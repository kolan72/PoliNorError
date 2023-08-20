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
		public static T WithRetryInner<T>(this T t, int retryCount, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToRetryPolicy(retryCount));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, TimeSpan delay, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay));
		}

		public static T WithRetryInner<T>(this T t, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delayOnRetryFunc));
		}

		public static T WithRetryInner<T>(this T t, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicy());
		}

		public static T WithRetryInner<T>(this T t, TimeSpan delay, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay));
		}

		public static T WithRetryInner<T>(this T t, Func<int, Exception, TimeSpan> delayOnRetryFunc, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delayOnRetryFunc));
		}

		public static T WithFallbackInner<T>(this T t, Action<CancellationToken> fallback, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback));
		}

		public static T WithFallbackInner<T>(this T t, Action fallback, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallback, convertType));
		}

		public static T WithFallbackInner<T>(this T t, Func<Task> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T>(this T t, Func<CancellationToken, Task> fallbackAsync, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, U> fallbackAsync, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<U> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<CancellationToken, Task<U>> fallbackAsync, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync));
		}

		public static T WithFallbackInner<T, U>(this T t, Func<Task<U>> fallbackAsync, PolicyErrorProcessor policyParams = null, CancellationType convertType = CancellationType.Precancelable) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToFallbackPolicy(fallbackAsync, convertType));
		}

		public static T WithSimpleInner<T>(this T t, PolicyErrorProcessor policyParams = null) where T : IWithPolicy<T>
		{
			return t.WithPolicy(policyParams.ToSimplePolicy());
		}
	}
}
