using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides extension methods for registering error processors with bulk error processor implementations.
	/// </summary>
	public static partial class BulkErrorProcessorRegistration
	{
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) => ((IBulkErrorProcessor)pr).AddProcessor(erPr);

		/// <summary>
		/// Adds a synchronous error processor to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor) where T : IBulkErrorProcessor
				=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a synchronous error processor with cancellation token support to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a synchronous error processor with specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous error processor to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous error processor with specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous error processor with cancellation token support to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors with specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors with specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds a synchronous error processor with processing error info to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a synchronous error processor with processing error info and cancellation token support to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a synchronous error processor with processing error info and specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous error processor with processing error info to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous error processor with processing error info and specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an asynchronous error processor with processing error info and cancellation token support to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors with processing error info to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors with processing error info and specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors with processing error info to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds both asynchronous and synchronous error processors with processing error info and specified cancellation type to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processors to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionProcessor">The synchronous action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The policy processor with the error processors added.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor implementation to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the error processor to.</param>
		/// <param name="errorProcessor">The error processor to add.</param>
		/// <returns>The policy processor with the error processor added.</returns>
		public static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds a delay error processor with a factory function to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the delay error processor to.</param>
		/// <param name="delayFactory">The factory function that determines the delay based on retry count and exception.</param>
		/// <returns>The policy processor with the delay error processor added.</returns>
		public static T WithDelayBetweenRetries<T>(this T policyProcessor, Func<int, Exception, TimeSpan> delayFactory) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessor(new DelayErrorProcessor(delayFactory), _addErrorProcessorAction);

		/// <summary>
		/// Adds a delay error processor with a fixed time span to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor that implements <see cref="IBulkErrorProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to add the delay error processor to.</param>
		/// <param name="time">The fixed time span to delay between retries.</param>
		/// <returns>The policy processor with the delay error processor added.</returns>
		public static T WithDelayBetweenRetries<T>(this T policyProcessor, TimeSpan time) where T : IBulkErrorProcessor
						=> policyProcessor.WithErrorProcessor(new DelayErrorProcessor(time), _addErrorProcessorAction);
	}
}
