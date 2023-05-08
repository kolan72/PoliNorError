using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackErrorProcessorExtensions
	{
		public static FallbackPolicy ToFallbackPolicy<T>(this FallbackErrorProcessor invokeFallbackPolicyParams, Func<T> fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this FallbackErrorProcessor invokeFallbackPolicyParams, Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this FallbackErrorProcessor invokeFallbackPolicyParams, Func<CancellationToken, Task<T>> fallbackAsync)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFunc(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy(this FallbackErrorProcessor invokeFallbackPolicyParams, Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFuncAndReturnSelf(fallbackAsync, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy(this FallbackErrorProcessor invokeFallbackPolicyParams, Action fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackActionAndReturnSelf(fallback, convertType));
		}

		public static FallbackPolicy ToFallbackPolicy(this FallbackErrorProcessor invokeFallbackPolicyParams, Action<CancellationToken> fallback)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackActionAndReturnSelf(fallback));
		}

		public static FallbackPolicy ToFallbackPolicy(this FallbackErrorProcessor invokeFallbackPolicyParams, Func<CancellationToken, Task> fallbackAsync)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithAsyncFallbackFuncAndReturnSelf(fallbackAsync));
		}

		public static FallbackPolicy ToFallbackPolicy<T>(this FallbackErrorProcessor invokeFallbackPolicyParams, Func<CancellationToken, T> fallback)
		{
			return (invokeFallbackPolicyParams ?? FallbackErrorProcessor.Default()).ConfigurePolicy(new FallbackPolicy().WithFallbackFunc(fallback));
		}
	}
}
