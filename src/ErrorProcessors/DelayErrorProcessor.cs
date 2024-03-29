﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DelayErrorProcessor : IErrorProcessor
	{
		private readonly Func<int, Exception, TimeSpan> _sleepProvider;

		public DelayErrorProcessor(Func<int, Exception, TimeSpan> sleepProvider) => _sleepProvider = sleepProvider;

		public DelayErrorProcessor(TimeSpan timeSpan) : this((_, __) => timeSpan){}

		public DelayErrorProcessor(Func<int, TimeSpan> delayOnRetryFunc) : this((retryAttempt, _) => delayOnRetryFunc(retryAttempt)){}

		public DelayErrorProcessor(Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg) : this((retryAttempt, exc) => delayOnRetryFunc.Apply(delayFuncArg)(retryAttempt, exc))
		{}

		public virtual Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			bool waitResult = cancellationToken.WaitHandle.WaitOne(GetCurDelay(GetRetryAttempt(catchBlockProcessErrorInfo), error));
			if (waitResult)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			return error;
		}

		public virtual async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			await Task.Delay(GetCurDelay(GetRetryAttempt(catchBlockProcessErrorInfo), error), cancellationToken).ConfigureAwait(configAwait);
			return error;
		}

		private static int GetRetryAttempt(ProcessingErrorInfo catchBlockProcessErrorInfo)
		{
			switch (catchBlockProcessErrorInfo)
			{
				case null:
					return 0;
				default:
					if (catchBlockProcessErrorInfo.HasContext)
					{
						return catchBlockProcessErrorInfo.CurrentRetryCount;
					}
					else
					{
						return 0;
					}
			}
		}

		private int GetCurDelay(int retryAttempt, Exception ex)
		{
			return (int)_sleepProvider(retryAttempt, ex).TotalMilliseconds;
		}
	}
}
