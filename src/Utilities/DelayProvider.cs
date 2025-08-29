using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IDelayProvider
	{
		void Backoff(TimeSpan delay, CancellationToken cancellationToken = default);
		Task BackoffAsync(TimeSpan delay, bool configAwait = false, CancellationToken cancellationToken = default);
	}

	internal static class IDelayProviderExtensions
	{
		public static BasicResult BackoffSafely(this IDelayProvider delayProvider, TimeSpan delay, CancellationToken token = default)
		{
			try
			{
				delayProvider.Backoff(delay, token);
				return BasicResult.Success();
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				return BasicResult.Canceled();
			}
			catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
			{
				return BasicResult.Canceled();
			}
			catch (Exception ex)
			{
				return BasicResult.Failure(ex);
			}
		}

		public static bool DelayAndCheckIfResultFailed(this IDelayProvider delayProvider, TimeSpan? delay, PolicyResult policyResult, Exception handlingException, CancellationToken token = default)
		{
			if (delay > TimeSpan.Zero)
			{
				try
				{
					delayProvider.Backoff(delay.Value, token);
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled();
				}
				catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
				{
					policyResult.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					policyResult.SetFailedWithCatchBlockError(ex, handlingException, CatchBlockExceptionSource.PolicyRule);
				}
			}
			return policyResult.IsFailed;
		}

		public static async Task<BasicResult> BackoffSafelyAsync(this IDelayProvider delayProvider, TimeSpan delay, bool configAwait = false, CancellationToken token = default)
		{
			try
			{
				await delayProvider.BackoffAsync(delay, configAwait, token).ConfigureAwait(configAwait);
				return BasicResult.Success();
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				return BasicResult.Canceled();
			}
			catch (Exception ex)
			{
				return BasicResult.Failure(ex);
			}
		}
	}

	internal class DelayProvider : IDelayProvider
	{
		public void Backoff(TimeSpan delay, CancellationToken cancellationToken = default)
		{
			bool waitResult = cancellationToken.WaitHandle.WaitOne(delay);
			if (waitResult)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		public async Task BackoffAsync(TimeSpan delay, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			await Task.Delay(delay, cancellationToken).ConfigureAwait(configAwait);
		}
	}
}
