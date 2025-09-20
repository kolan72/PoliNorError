using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FallbackFuncExtensions
	{
		public static FallbackFuncExecResult HandleAsFallback(this Action<CancellationToken> fallback, CancellationToken token)
		{
			try
			{
				fallback(token);
				return FallbackFuncExecResult.Success();
			}
			catch (OperationCanceledException tce) when (token.IsCancellationRequested)
			{
				return FallbackFuncExecResult.FromCanceledError(tce);
			}
			catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
			{
				return FallbackFuncExecResult.FromCanceledError(ae.GetCancellationException());
			}
			catch (Exception cex)
			{
				return FallbackFuncExecResult.FromError(cex);
			}
		}

		public static FallbackFuncExecResult<T> HandleAsFallback<T>(this Func<CancellationToken, T> fallback, CancellationToken token)
		{
			try
			{
				var res = fallback(token);
				return FallbackFuncExecResult<T>.Success(res);
			}
			catch (OperationCanceledException tce) when (token.IsCancellationRequested)
			{
				return FallbackFuncExecResult<T>.FromCanceledError(tce);
			}
			catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
			{
				return FallbackFuncExecResult<T>.FromCanceledError(ae.GetCancellationException());
			}
			catch (Exception cex)
			{
				return FallbackFuncExecResult<T>.FromError(cex);
			}
		}

		public static async Task<FallbackFuncExecResult> HandleAsFallbackAsync(this Func<CancellationToken, Task> fallback, bool configAwait, CancellationToken token)
		{
			try
			{
				await fallback(token).ConfigureAwait(configAwait);
				return FallbackFuncExecResult.Success();
			}
			catch (OperationCanceledException tce) when (token.IsCancellationRequested)
			{
				return FallbackFuncExecResult.FromCanceledError(tce);
			}
			catch (Exception cex)
			{
				return FallbackFuncExecResult.FromError(cex);
			}
		}

		public static async Task<FallbackFuncExecResult<T>> HandleAsFallbackAsync<T>(this Func<CancellationToken, Task<T>> fallback, bool configAwait, CancellationToken token)
		{
			try
			{
				var res = await fallback(token).ConfigureAwait(configAwait);
				return FallbackFuncExecResult<T>.Success(res);
			}
			catch (OperationCanceledException tce) when (token.IsCancellationRequested)
			{
				return FallbackFuncExecResult<T>.FromCanceledError(tce);
			}
			catch (Exception cex)
			{
				return FallbackFuncExecResult<T>.FromError(cex);
			}
		}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This method is obsolete")]
#pragma warning restore S1133 // Deprecated code should be removed
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

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This method is obsolete")]
#pragma warning restore S1133 // Deprecated code should be removed
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
