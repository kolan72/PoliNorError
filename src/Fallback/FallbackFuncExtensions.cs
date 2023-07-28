using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackFuncExtensions
	{
		public static FallbackFuncExecResult HandleAsFallback(this Action<CancellationToken> fallback, CancellationToken token)
		{
			return fallback.ToDefaultReturnFunc<int>().HandleAsFallbackInner(token).Item1;
		}

		public static FallbackFuncExecResult<T> HandleAsFallback<T>(this Func<CancellationToken, T> fallback, CancellationToken token)
		{
			var resTuple = fallback.HandleAsFallbackInner(token);
			var res = FallbackFuncExecResult<T>.FromFuncExecWithTokenResult(resTuple.Item1);
			if (resTuple.Item1.IsSuccess)
			{
				res.SetResult(resTuple.Item2);
			}
			return res;
		}

		public static async Task<FallbackFuncExecResult> HandleAsFallbackAsync(this Func<CancellationToken, Task> fallback, bool configAwait, CancellationToken token)
		{
			return (await fallback.ToDefaultReturnFunc<int>(configAwait).HandleAsFallbackInnerAsync(configAwait, token).ConfigureAwait(configAwait)).Item1;
		}

		public static async Task<FallbackFuncExecResult<T>> HandleAsFallbackAsync<T>(this Func<CancellationToken, Task<T>> fallback, bool configAwait, CancellationToken token)
		{
			var resTuple = await fallback.HandleAsFallbackInnerAsync(configAwait, token).ConfigureAwait(configAwait);
			var res = FallbackFuncExecResult<T>.FromFuncExecWithTokenResult(resTuple.Item1);
			if (resTuple.Item1.IsSuccess)
			{
				res.SetResult(resTuple.Item2);
			}
			return res;
		}

		internal static (FallbackFuncExecResult, T) HandleAsFallbackInner<T>(this Func<CancellationToken, T> fallback, CancellationToken token)
		{
			try
			{
				var res = fallback(token);
				return (FallbackFuncExecResult.Success(), res);
			}
			catch (OperationCanceledException tce)
			{
				return (FallbackFuncExecResult.FromErrorAndToken(tce, token), default);
			}
			catch (AggregateException ae)
			{
				return (FallbackFuncExecResult.FromErrorAndToken(ae, token), default);
			}
			catch (Exception cex)
			{
				return (FallbackFuncExecResult.FromError(cex), default);
			}
		}

		internal static async Task<(FallbackFuncExecResult, T)> HandleAsFallbackInnerAsync<T>(this Func<CancellationToken, Task<T>> fallback, bool configAwait, CancellationToken token)
		{
			try
			{
				var res = await fallback(token).ConfigureAwait(configAwait);
				return (FallbackFuncExecResult.Success(), res);
			}
			catch (OperationCanceledException tce)
			{
				return (FallbackFuncExecResult.FromErrorAndToken(tce, token), default);
			}
			catch (Exception cex)
			{
				return (FallbackFuncExecResult.FromError(cex), default);
			}
		}
	}
}
