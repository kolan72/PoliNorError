using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackPolicyBaseExtensions
	{
		internal static TFallback WithFallbackFunc<TFallback, T>(this TFallback fallback, Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			return WithFallbackFunc(fallback, (convertType == CancellationType.Precancelable) ? fallbackFunc.ToPrecancelableFunc() : fallbackFunc.ToCancelableFunc());
		}

		internal static TFallback WithFallbackFunc<TFallback, T>(this TFallback fallback, Func<CancellationToken, T> fallbackFunc) where TFallback : FallbackPolicyBase
		{
			fallback._holders[typeof(T)] = new FallBackFuncHolder<T>(fallbackFunc);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, T>(this TFallback fallback, Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			return WithAsyncFallbackFunc(fallback, (convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc() : fallbackAsync.ToCancelableFunc());
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, T>(this TFallback fallback, Func<CancellationToken, Task<T>> fallbackAsync) where TFallback : FallbackPolicyBase
		{
			fallback._asyncHolders[typeof(T)] = new FallBackAsyncFuncHolder<T>(fallbackAsync);
			return fallback;
		}
	}
}
