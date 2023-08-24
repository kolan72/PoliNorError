using System;

namespace PoliNorError
{
	/// <summary>
	/// Provides static methods to add DelayErrorProcessor to IRetryProcessor.
	/// </summary>
	public static class DelayErrorProcessorRegistration
	{
		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, TimeSpan delay)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(delay));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, Func<int, TimeSpan> delayOnRetryFunc)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(delayOnRetryFunc));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(delayOnRetryFunc, delayFuncArg));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, Func<int, Exception, TimeSpan> retryFunc)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(retryFunc));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, DelayErrorProcessor delayErrorProcessor)
		{
			retryProcessor.AddErrorProcessor(delayErrorProcessor);
			return retryProcessor;
		}
	}
}
