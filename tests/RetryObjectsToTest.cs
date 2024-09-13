using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FakeDelayProvider : IDelayProvider
	{
		private readonly CancellationTokenSource _source;
		public FakeDelayProvider(CancellationTokenSource source = null)
		{
			_source = source;
		}

		public int NumOfCalls { get; private set; }

		public void Backoff(TimeSpan delay, CancellationToken cancellationToken = default)
		{
			NumOfCalls++;
			_source?.Cancel();
		}

		public async Task BackoffAsync(TimeSpan delay, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			await Task.Delay(1);
			NumOfCalls++;
			_source?.Cancel();
		}
	}

	internal class FakeRetryDelay : RetryDelay
	{
		public int AttemptsNumber { get; private set; }

		public override TimeSpan GetDelay(int attempt)
		{
			AttemptsNumber++;
			return TimeSpan.Zero;
		}
	}

	internal class DelayProviderThatAlreadyCanceled : IDelayProvider
	{
		private readonly CancellationTokenSource _cts;
		public DelayProviderThatAlreadyCanceled(CancellationTokenSource cts)
		{
			_cts = cts;
		}

		public void Backoff(TimeSpan delay, CancellationToken cancellationToken = default)
		{
			_cts.Cancel();
			bool waitResult = cancellationToken.WaitHandle.WaitOne(delay);
			if (waitResult)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		public async Task BackoffAsync(TimeSpan delay, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			_cts.Cancel();
			await Task.Delay(delay, cancellationToken).ConfigureAwait(configAwait);
		}
	}
}
