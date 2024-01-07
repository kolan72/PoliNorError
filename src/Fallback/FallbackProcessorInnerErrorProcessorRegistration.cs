using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class FallbackProcessorInnerErrorProcessorRegistration
	{
		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException> actionProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, CancellationToken> actionProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, Task> funcProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor WithInnerErrorProcessorOf<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> fallbackProcessor.WithInnerErrorProcessorOf<IFallbackProcessor, TException>(funcProcessor);
	}
}
