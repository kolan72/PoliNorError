using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DelayErrorProcessor : IErrorProcessor
	{
		private readonly Func<int, Exception, TimeSpan> _sleepProvider;
		private readonly IDelayProvider _delayProvider;

		public DelayErrorProcessor(TimeSpan timeSpan) : this((_, __) => timeSpan){}

		public DelayErrorProcessor(Func<int, TimeSpan> delayOnRetryFunc) : this((retryAttempt, _) => delayOnRetryFunc(retryAttempt)){}

		public DelayErrorProcessor(Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg) : this((retryAttempt, exc) => delayOnRetryFunc.Apply(delayFuncArg)(retryAttempt, exc))
		{}

		public DelayErrorProcessor(Func<int, Exception, TimeSpan> sleepProvider) : this(sleepProvider, null)
		{}

		internal DelayErrorProcessor(Func<int, Exception, TimeSpan> sleepProvider, IDelayProvider delayProvider)
		{
			_sleepProvider = sleepProvider;
			_delayProvider = delayProvider ?? new DelayProvider();
		}

		public virtual Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			_delayProvider.Backoff(GetCurDelay(GetRetry(catchBlockProcessErrorInfo), error), cancellationToken);
			return error;
		}

		public virtual async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			await _delayProvider.BackoffAsync(GetCurDelay(GetRetry(catchBlockProcessErrorInfo), error), configAwait, cancellationToken).ConfigureAwait(configAwait);
			return error;
		}

		private static int GetRetry(ProcessingErrorInfo catchBlockProcessErrorInfo)
		{
			switch (catchBlockProcessErrorInfo)
			{
				case null:
					return 0;
				default:
					if (catchBlockProcessErrorInfo.HasContext && catchBlockProcessErrorInfo is RetryProcessingErrorInfo info)
					{
						return info.RetryCount;
					}
					else
					{
						return 0;
					}
			}
		}

		private TimeSpan GetCurDelay(int retry, Exception ex)
		{
			return _sleepProvider(retry, ex);
		}
	}
}
