using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class SimplePolicyProcessorInnerErrorProcessorRegistration
	{
		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, CancellationToken> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		///  Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, Task> funcProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		///  Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		///  Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		/// <summary>
		///  Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException"></typeparam>
		/// <param name="simplePolicyProcessor"></param>
		/// <param name="funcProcessor"></param>
		/// <returns>A processor for Simple policy.</returns>
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);
	}
}
