using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class SimplePolicyProcessorTypedErrorProcessorRegistration
	{
		/// <summary>
		/// Adds a typed error processor to the simple policy processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithTypedErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds a typed error processor to the simple policy processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		=> simplePolicyProcessor.WithTypedErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor, cancellationType);

		/// <summary>
		/// Adds a typed error processor to the simple policy processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="actionProcessor">The action processor.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithTypedErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		/// <summary>
		/// Adds a typed error processor to the simple policy processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> simplePolicyProcessor.WithTypedErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds a typed error processor to the simple policy processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> simplePolicyProcessor.WithTypedErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor, cancellationType);

		/// <summary>
		/// Adds a typed error processor to the simple policy processor that uses a Func with CancellationToken.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="funcProcessor">The function processor.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> simplePolicyProcessor.WithTypedErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		/// <summary>
		/// Adds a typed error processor to the simple policy processor.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="simplePolicyProcessor">The simple policy processor.</param>
		/// <param name="errorProcessor">The error processor.</param>
		/// <returns>The simple policy processor with the added error processor.</returns>
		public static ISimplePolicyProcessor WithTypedErrorProcessor<TException>(this ISimplePolicyProcessor simplePolicyProcessor, DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
			=> simplePolicyProcessor.WithTypedErrorProcessor<ISimplePolicyProcessor, TException>(errorProcessor);
	}
}