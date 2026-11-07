using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyCollectionErrorProcessorRegistration
	{
		/// <summary>
		/// Adds a typed error processor to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">The action to execute when an exception of the specified type occurs.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
				=> policyCollection.WithTypedErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a typed error processor with cancellation token support to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">The action to execute when an exception of the specified type occurs.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
				=> policyCollection.WithTypedErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a typed error processor with specified cancellation type to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">The action to execute when an exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
				=> policyCollection.WithTypedErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous typed error processor to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of the specified type occurs.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
				=> policyCollection.WithTypedErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous typed error processor with specified cancellation type to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
				=> policyCollection.WithTypedErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous typed error processor with cancellation token support to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of the specified type occurs.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
				=> policyCollection.WithTypedErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a typed error processor implementation to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <typeparam name="TException">The type of exception to process.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="errorProcessor">The typed error processor to add.</param>
		/// <returns>The <see cref="PolicyCollection"/> with the typed error processor added.</returns>
		public static PolicyCollection WithTypedErrorProcessor<TException>(this PolicyCollection policyCollection, DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return policyCollection.WithTypedErrorProcessor(errorProcessor, _addErrorProcessorAction);
		}
	}
}
