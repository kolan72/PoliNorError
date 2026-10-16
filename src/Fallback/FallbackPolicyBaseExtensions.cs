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

		internal static TFallback WithFallbackFunc<TFallback, TParam, T>(this TFallback fallback, Func<TParam, T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetFallbackFunc(fallbackFunc, convertType);
			return fallback;
		}

		internal static TFallback WithFallbackFunc<TFallback, TParam, T>(this TFallback fallback, Func<TParam, CancellationToken, T> fallbackFunc) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetFallbackFunc(fallbackFunc);
			return fallback;
		}

		internal static TFallback WithFallbackAction<TFallback, TParam>(this TFallback fallback, Action<TParam> fallbackAction, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetFallbackAction(fallbackAction, convertType);
			return fallback;
		}

		internal static TFallback WithFallbackAction<TFallback, TParam>(this TFallback fallback, Action<TParam, CancellationToken> fallbackAction) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetFallbackAction(fallbackAction);
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

		internal static TFallback WithAsyncFallbackFunc<TFallback, TParam, T>(this TFallback fallback, Func<TParam, Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetAsyncFallbackFunc(fallbackAsync, convertType);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, TParam, T>(this TFallback fallback, Func<TParam, CancellationToken, Task<T>> fallbackAsync) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetAsyncFallbackFunc(fallbackAsync);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, TParam>(this TFallback fallback, Func<TParam, Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetAsyncFallbackFunc(fallbackAsync, convertType);
			return fallback;
		}

		internal static TFallback WithAsyncFallbackFunc<TFallback, TParam>(this TFallback fallback, Func<TParam, CancellationToken, Task> fallbackAsync) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.SetAsyncFallbackFunc(fallbackAsync);
			return fallback;
		}

		internal static TFallback WithFallbackBehavior<TFallback, T>(this TFallback fallback, FallbackBehavior<T> fallbackBehavior) where TFallback : FallbackPolicyBase
		{
			fallback._fallbackFuncsProvider.AddOrReplaceFallbackBehavior(fallbackBehavior);
			return fallback;
		}
	}
}
