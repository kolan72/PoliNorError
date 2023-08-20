using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackInvokeParamsExtensions
	{
		public static FallbackPolicy ToFallbackPolicy<T>(this PolicyErrorProcessor invokeFallbackPolicyParams, Func<T> fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this PolicyErrorProcessor invokeFallbackPolicyParams, Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this PolicyErrorProcessor invokeFallbackPolicyParams, Func<CancellationToken, Task<T>> fallbackAsync)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy(this PolicyErrorProcessor invokeFallbackPolicyParams, Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFuncAndReturnSelf(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy(this PolicyErrorProcessor invokeFallbackPolicyParams, Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackActionAndReturnSelf(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy(this PolicyErrorProcessor invokeFallbackPolicyParams, Action<CancellationToken> fallback)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackActionAndReturnSelf(fallback));
		}

		public static FallbackPolicy ToFallbackPolicy(this PolicyErrorProcessor invokeFallbackPolicyParams, Func<CancellationToken, Task> fallbackAsync)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFuncAndReturnSelf(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this PolicyErrorProcessor invokeFallbackPolicyParams, Func<CancellationToken, T> fallback)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback));
		}
	}
}
