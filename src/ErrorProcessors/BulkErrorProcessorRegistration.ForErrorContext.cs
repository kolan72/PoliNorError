using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class BulkErrorProcessorRegistration
	{
		/// <summary>
		/// Adds a synchronous error processor with error context to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with error context and specified cancellation type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a synchronous error processor with error context and cancellation token support to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with error context to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with error context and specified cancellation type to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds an asynchronous error processor with error context and cancellation token support to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		/// <summary>
		/// Adds a default error processor with error context to the bulk error processor.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
		/// <param name="errorProcessor">The default error processor to add.</param>
		/// <returns>The bulk error processor with the error context processor added.</returns>
		public static BulkErrorProcessor WithErrorContextProcessor<TErrorContext>(this BulkErrorProcessor policyProcessor, DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			return policyProcessor.WithErrorContextProcessor(errorProcessor, _addErrorProcessorAction);
		}
	}
}
