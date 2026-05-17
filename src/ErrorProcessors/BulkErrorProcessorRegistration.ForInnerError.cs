using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class BulkErrorProcessorRegistration
	{
		/// <summary>
		/// Adds a synchronous error processor for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with cancellation token support for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with specified cancellation type for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with specified cancellation type for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with cancellation token support for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with processing error info for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with processing error info and cancellation token support for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with processing error info and specified cancellation type for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with processing error info for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with processing error info and specified cancellation type for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with processing error info and cancellation token support for inner exceptions of a specific type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <returns>The bulk error processor with the inner error processor added.</returns>
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}
	}
}
