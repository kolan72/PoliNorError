using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackInvokeParamsExtensions
	{
		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorDelegate invokeFallbackPolicyParams, Func<T> fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorDelegate invokeFallbackPolicyParams, Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorDelegate invokeFallbackPolicyParams, Func<CancellationToken, Task<T>> fallbackAsync)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorDelegate invokeFallbackPolicyParams, Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFuncAndReturnSelf(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorDelegate invokeFallbackPolicyParams, Action fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackActionAndReturnSelf(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorDelegate invokeFallbackPolicyParams, Action<CancellationToken> fallback)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackActionAndReturnSelf(fallback));
		}

		public static FallbackPolicy ToFallbackPolicy(this ErrorProcessorDelegate invokeFallbackPolicyParams, Func<CancellationToken, Task> fallbackAsync)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFuncAndReturnSelf(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this ErrorProcessorDelegate invokeFallbackPolicyParams, Func<CancellationToken, T> fallback)
		{
			return (FallbackPolicy)(invokeFallbackPolicyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback));
		}
	}
}
