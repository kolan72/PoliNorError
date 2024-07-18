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
