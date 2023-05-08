using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class FallbackPolicyBaseExtensions
	{
		internal static TFallback WithFallbackFunc<TFallback, T>(this TFallback fallback, Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where TFallback : FallbackPolicyBase
		{
			return (convertType == ConvertToCancelableFuncType.Precancelable) ? WithFallbackFunc(fallback, fallbackFunc.ToPrecancelableFunc())
																				: WithFallbackFunc(fallback, fallbackFunc.ToCancelableFunc());
		}

		internal static TFallback WithFallbackFunc<TFallback, T>(this TFallback fallback, Func<CancellationToken, T> fallbackFunc) where TFallback : FallbackPolicyBase
		{
			fallback._holders[typeof(T)] = new FallBackFuncHolder<T>(fallbackFunc);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, T>(this TFallback fallback, Func<CancellationToken, Task<T>> fallbackAsync) where TFallback : FallbackPolicyBase
		{
			fallback._asyncHolders[typeof(T)] = new FallBackAsyncFuncHolder<T>(fallbackAsync);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, T>(this TFallback fallback, Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where TFallback : FallbackPolicyBase
		{
			return (convertType == ConvertToCancelableFuncType.Precancelable) ? WithAsyncFallbackFunc(fallback, fallbackAsync.ToPrecancelableFunc())
																	: WithAsyncFallbackFunc(fallback, fallbackAsync.ToCancelableFunc());
		}
	}
}
