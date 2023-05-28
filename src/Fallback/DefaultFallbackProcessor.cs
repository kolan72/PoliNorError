using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class DefaultFallbackProcessor : PolicyProcessor, IFallbackProcessor
	{
		public DefaultFallbackProcessor(IBulkErrorProcessor bulkErrorProcessor = null) : base(bulkErrorProcessor) { }

		public PolicyResult Fallback(Action action, Action<CancellationToken> fallback, CancellationToken token = default)
		{
			PolicyResult result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				action();
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				HandleCatchBlockAndChangeResult(ex, result, CatchBlockProcessErrorInfo.FromFallback(), token);
				if (!result.IsFailed)
				{
					fallback.HandleAsFallback(token).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public PolicyResult<T> Fallback<T>(Func<T> func, Func<CancellationToken, T> fallback, CancellationToken token = default)
		{
			var result = PolicyResult<T>.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = func();
				result.SetOk();
				result.SetResult(resAction);
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				HandleCatchBlockAndChangeResult(ex, result, CatchBlockProcessErrorInfo.FromFallback(), token);
				if (!result.IsFailed)
				{
					fallback.HandleAsFallback(token).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public async Task<PolicyResult> FallbackAsync(Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			PolicyResult result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				await func(token).ConfigureAwait(configureAwait);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				await HandleCatchBlockAndChangeResultAsync(ex, result, CatchBlockProcessErrorInfo.FromFallback(), token, configureAwait).ConfigureAwait(configureAwait);
				if (!result.IsFailed)
				{
					(await fallback.HandleAsFallbackAsync(configureAwait, token).ConfigureAwait(configureAwait)).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public async Task<PolicyResult<T>> FallbackAsync<T>(Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = await func(token).ConfigureAwait(configureAwait);
				result.SetResult(resAction);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				await HandleCatchBlockAndChangeResultAsync(ex, result, CatchBlockProcessErrorInfo.FromFallback(), token, configureAwait).ConfigureAwait(configureAwait);
				if (!result.IsFailed)
				{
					(await fallback.HandleAsFallbackAsync(configureAwait, token).ConfigureAwait(configureAwait)).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

	}
}
