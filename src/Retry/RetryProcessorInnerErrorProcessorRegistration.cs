using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class RetryProcessorInnerErrorProcessorRegistration
	{
		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException> actionProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, CancellationToken> actionProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, Task> funcProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it have the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor WithInnerErrorProcessorOf<TException>(this IRetryProcessor retryProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> retryProcessor.WithInnerErrorProcessorOf<IRetryProcessor, TException>(funcProcessor);
	}
}
