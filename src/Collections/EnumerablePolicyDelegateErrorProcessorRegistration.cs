using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class EnumerablePolicyDelegateErrorProcessorRegistration
	{
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) => ((IEnumerable<PolicyDelegateBase>)pr).Select(pd => pd.Policy).LastOrDefault()?.PolicyProcessor.AddErrorProcessor(erPr);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Action<Exception> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
				=> policyDelegateCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Action<Exception, CancellationToken> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, Task> funcProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, CancellationToken, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="actionProcessor"/>.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="funcProcessor"/> and the <paramref name="actionProcessor"/>.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="actionProcessor"/>.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="funcProcessor"/> and the <paramref name="actionProcessor"/>.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessorOf<T>(this T policyDelegateCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		///Adds an error processor to the policy of the last element in a collection of PolicyDelegates.
		/// </summary>
		/// <typeparam name="T">The type of collection of PolicyDelegates.</typeparam>
		/// <param name="policyDelegateCollection">A collection of PolicyDelegates.</param>
		/// <param name="errorProcessor">An error processor to add.</param>
		/// <returns>A collection of PolicyDelegates.</returns>
		public static T WithErrorProcessor<T>(this T policyDelegateCollection, IErrorProcessor errorProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyDelegateCollection.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);
	}
}
