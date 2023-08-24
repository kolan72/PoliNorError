using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides extension methods to add custom error saver to IRetryProcessor.
	/// </summary>
	public static class RetryProcessorCustomErrorSaverRegistration
	{
		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Action<Exception> actionProcessor)
			=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(actionProcessor));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Action<Exception, CancellationToken> actionProcessor)
						=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(actionProcessor));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(actionProcessor, cancellationType));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, Task> funcProcessor)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor, cancellationType));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, CancellationToken, Task> funcProcessor)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor, cancellationType));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor));

		public static IRetryProcessor UseCustomErrorSaverOf(this IRetryProcessor retryProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
					=> UseCustomErrorSaver(retryProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor, cancellationType));

		private static IRetryProcessor UseCustomErrorSaver(IRetryProcessor retryProcessor, IErrorProcessor errorProcessor)
		{
			retryProcessor.UseCustomErrorSaver(errorProcessor);
			return retryProcessor;
		}
	}
}
