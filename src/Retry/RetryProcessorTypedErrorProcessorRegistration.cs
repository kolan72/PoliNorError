using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class RetryProcessorTypedErrorProcessorRegistration
	{
		/// <summary>
		/// Adds a typed error processor to the retry processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessorOf<IRetryProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds a typed error processor to the retry processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessorOf<IRetryProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds a typed error processor to the retry processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessorOf<IRetryProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds a typed error processor to the retry processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessorOf<IRetryProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds a typed error processor to the retry processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessorOf<IRetryProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds a typed error processor to the retry processor that uses a Func with CancellationToken.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessorOf<IRetryProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds a typed error processor to the retry processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="retryProcessor">The retry processor.</param>
		/// <param name="errorProcessor">The error processor.</param>
		/// <returns>The retry processor with the added error processor.</returns>
		public static IRetryProcessor WithTypedErrorProcessor<TException>(this IRetryProcessor retryProcessor, DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
			=> retryProcessor.WithTypedErrorProcessor<IRetryProcessor, TException>(errorProcessor);
	}
}