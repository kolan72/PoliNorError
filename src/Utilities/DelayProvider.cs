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
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				return BasicResult.Canceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				return BasicResult.Canceled();
			}
			catch (Exception ex)
			{
				return BasicResult.Failure(ex);
			}
		}

		public static async Task<BasicResult> BackoffSafelyAsync(this IDelayProvider delayProvider, TimeSpan delay, bool configAwait = false, CancellationToken token = default)
		{
			try
			{
				await delayProvider.BackoffAsync(delay, configAwait, token).ConfigureAwait(configAwait);
				return BasicResult.Success();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
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
