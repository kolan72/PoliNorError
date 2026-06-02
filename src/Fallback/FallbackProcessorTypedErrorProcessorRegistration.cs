using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class FallbackProcessorTypedErrorProcessorRegistration
	{
		/// <summary>
		/// Adds a typed error processor to the fallback processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds a typed error processor to the fallback processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds a typed error processor to the fallback processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds a typed error processor to the fallback processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds a typed error processor to the fallback processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds a typed error processor to the fallback processor that uses a Func with CancellationToken.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds a typed error processor to the fallback processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="fallbackProcessor">The fallback processor.</param>
		/// <param name="errorProcessor">The error processor.</param>
		/// <returns>The fallback processor with the added error processor.</returns>
		public static IFallbackProcessor WithTypedErrorProcessor<TException>(this IFallbackProcessor fallbackProcessor, DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
			=> fallbackProcessor.WithTypedErrorProcessor<IFallbackProcessor, TException>(errorProcessor);
	}
}