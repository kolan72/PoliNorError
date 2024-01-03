using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyCollectionErrorProcessorRegistration
	{
		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException> actionProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, CancellationToken> actionProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, Task> funcProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the last policy of the <see cref="PolicyCollection"/> to handle an inner exception only if it has <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static PolicyCollection WithInnerErrorProcessorOf<TException>(this PolicyCollection policyCollection, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
				=> policyCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
	}
}
