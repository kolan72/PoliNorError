using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyCollectionErrorProcessorRegistration
	{
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) => ((PolicyCollection)pr).LastOrDefault()?.PolicyProcessor.AddErrorProcessor(erPr);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, CancellationToken> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">>A cancellation type.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, CancellationToken, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="actionProcessor"/>.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="funcProcessor"/> and the <paramref name="actionProcessor"/>.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, ProcessingErrorInfo> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="actionProcessor"/>.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="funcProcessor"/>.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="actionProcessor"/>.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor made up of delegates to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type for the <paramref name="funcProcessor"/> and the <paramref name="actionProcessor"/>.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds error processor to the last policy of the <see cref="PolicyCollection"/>.
		/// </summary>
		/// <param name="policyCollection">A collection of policies.</param>
		/// <param name="errorProcessor">An error processor to add.</param>
		/// <returns><see cref="PolicyCollection"/></returns>
		public static PolicyCollection WithErrorProcessor(this PolicyCollection policyCollection, IErrorProcessor errorProcessor)
						=> policyCollection.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);
	}
}
