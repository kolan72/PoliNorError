using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackPolicyErrorProcessorExtensions
	{
		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorParam invokeFallbackPolicyParams, Func<T> fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorParam invokeFallbackPolicyParams, Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorParam invokeFallbackPolicyParams, Func<CancellationToken, Task<T>> fallbackAsync)
		{
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorParam invokeFallbackPolicyParams, Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable, bool onlyGenericFallbackForGenericDelegate = false)
		{
			var fb = new FallbackPolicy(onlyGenericFallbackForGenericDelegate);
			fb._fallbackFuncsProvider.FallbackAsync = fallbackAsync.ToCancelableFunc(convertType, true);
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(fb);
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorParam invokeFallbackPolicyParams, Action fallback, CancellationType convertType = CancellationType.Precancelable, bool onlyGenericFallbackForGenericDelegate = false)
		{
			var fb = new FallbackPolicy(onlyGenericFallbackForGenericDelegate);
			fb._fallbackFuncsProvider.Fallback = fallback.ToCancelableAction(convertType, true);
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(fb);
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorParam invokeFallbackPolicyParams, Action<CancellationToken> fallback, bool onlyGenericFallbackForGenericDelegate = false)
		{
			var fb = new FallbackPolicy(onlyGenericFallbackForGenericDelegate);
			fb._fallbackFuncsProvider.Fallback = fallback;
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(fb);
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorParam invokeFallbackPolicyParams, Func<CancellationToken, Task> fallbackAsync, bool onlyGenericFallbackForGenericDelegate = false)
		{
			var fb = new FallbackPolicy(onlyGenericFallbackForGenericDelegate);
			fb._fallbackFuncsProvider.FallbackAsync = fallbackAsync;
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(fb);
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorParam invokeFallbackPolicyParams, Func<CancellationToken, T> fallback)
		{
			return (FallbackPolicy)invokeFallbackPolicyParams.GetValueOrDefault().ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback));
		}
	}
}
