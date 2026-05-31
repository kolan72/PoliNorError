using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of extension methods to add error processor to policy.
	/// </summary>
	public static partial class PolicyErrorProcessorRegistration
	{
		/// <summary>
		/// Adds an error processor that handles exceptions using a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using a synchronous action with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using a synchronous action with specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using an asynchronous function.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using an asynchronous function with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <param name="convertToCancelableFuncType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using an asynchronous function with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <param name="convertToCancelableFuncType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions.</param>
		/// <param name="convertToCancelableFuncType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with processing error information.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using a synchronous action with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with processing error information and cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using a synchronous action with specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="actionProcessor">The action to process exceptions with processing error information.</param>
		/// <param name="cancellationType">The type of cancellation handling to apply.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using an asynchronous function.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using an asynchronous function with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <param name="convertToCancelableFuncType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using an asynchronous function with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information and cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information and cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information and cancellation token.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <param name="convertToCancelableFuncType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an error processor that handles exceptions with processing error information using both an asynchronous function and a synchronous action with specified cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="funcProcessor">The asynchronous function to process exceptions with processing error information.</param>
		/// <param name="actionProcessor">The synchronous action to process exceptions with processing error information.</param>
		/// <param name="convertToCancelableFuncType">The type of cancellation handling to apply when converting to cancelable function.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds a custom error processor implementation to the policy.
		/// </summary>
		/// <typeparam name="T">The type of policy that implements <see cref="IPolicyBase"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the error processor will be added.</param>
		/// <param name="errorProcessor">The custom error processor implementation.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T WithErrorProcessor<T>(this T errorPolicyBase, IErrorProcessor errorProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.AddErrorProcessor(errorProcessor);
			return errorPolicyBase;
		}
	}
}
