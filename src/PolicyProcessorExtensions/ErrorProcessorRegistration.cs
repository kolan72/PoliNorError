using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of extension methods to add error processor to policy processor.
	/// </summary>
	public static partial class ErrorProcessorRegistration
	{
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) =>((IPolicyProcessor)pr).AddErrorProcessor(erPr);

		/// <summary>
		/// Adds an error processor that handles exceptions using a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using a synchronous action with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with cancellation token.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using a synchronous action with specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using an asynchronous function.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using an asynchronous function with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using an asynchronous function with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with cancellation token.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with processing error information.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using a synchronous action with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with processing error information and cancellation token.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using a synchronous action with specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with processing error information.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using an asynchronous function.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using an asynchronous function with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using an asynchronous function with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information and cancellation token.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information and cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information and cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		/// <summary>
		/// Adds a custom error processor implementation to the policy processor.
		/// </summary>
		/// <typeparam name="T">The type of policy processor that implements <see cref="IPolicyProcessor"/>.</typeparam>
		/// <param name="policyProcessor">The policy processor to which the error processor will be added.</param>
		/// <param name="errorProcessor">The custom error processor implementation.</param>
		/// <returns>The policy processor instance for method chaining.</returns>
		public static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);
	}
}
