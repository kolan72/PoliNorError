using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackPolicyBaseExtensions
	{
		internal static TFallback WithFallbackFunc<TFallback, T>(this TFallback fallback, Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetFallbackFunc(fallbackFunc, convertType);
			return fallback;
		}

		internal static TFallback WithFallbackFunc<TFallback, T>(this TFallback fallback, Func<CancellationToken, T> fallbackFunc) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetFallbackFunc(fallbackFunc);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, T>(this TFallback fallback, Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetAsyncFallbackFunc(fallbackAsync, convertType);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, T>(this TFallback fallback, Func<CancellationToken, Task<T>> fallbackAsync) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetAsyncFallbackFunc(fallbackAsync);
			return fallback;
		}
	}
}
