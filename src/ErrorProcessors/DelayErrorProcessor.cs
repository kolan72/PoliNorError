using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DelayErrorProcessor : IErrorProcessor
	{
		private readonly Func<int, Exception, TimeSpan> _sleepProvider;
		private readonly IDelayProvider _delayProvider;

		public DelayErrorProcessor(TimeSpan timeSpan) : this((_, __) => timeSpan)
		{
		}

		public DelayErrorProcessor(Func<int, TimeSpan> delayOnRetryFunc) : this((retryAttempt, _) => delayOnRetryFunc(retryAttempt))
		{
		}

		public DelayErrorProcessor(Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg)
			: this((retryAttempt, exc) => delayOnRetryFunc(delayFuncArg, retryAttempt, exc))
		{
		}

		public DelayErrorProcessor(RetryDelay retryDelay) : this((retryAttempt, _) => retryDelay.GetDelay(retryAttempt))
		{
		}

		public DelayErrorProcessor(Func<int, Exception, TimeSpan> sleepProvider) : this(sleepProvider, null)
		{
		}

		internal DelayErrorProcessor(Func<int, Exception, TimeSpan> sleepProvider, IDelayProvider delayProvider)
		{
			_sleepProvider = sleepProvider;
			_delayProvider = delayProvider ?? new DelayProvider();
		}

		public virtual Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			var delay = GetDelay(catchBlockProcessErrorInfo, error);
			_delayProvider.Backoff(delay, cancellationToken);
			return error;
		}

		public virtual async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			var delay = GetDelay(catchBlockProcessErrorInfo, error);
			await _delayProvider.BackoffAsync(delay, configAwait, cancellationToken).ConfigureAwait(configAwait);
			return error;
		}

		private TimeSpan GetDelay(ProcessingErrorInfo info, Exception ex)
		{
			return _sleepProvider(info.GetRetryCount(), ex);
		}
	}
}
