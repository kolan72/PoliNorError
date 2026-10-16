using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
    public static partial class BulkErrorProcessorRegistration
    {
        /// <summary>
        /// Adds a synchronous error processor for typed exceptions to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="actionProcessor">The action to execute when an exception of the specified type occurs.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
        }

        /// <summary>
        /// Adds a synchronous error processor with cancellation token support for typed exceptions to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="actionProcessor">The action to execute when an exception of the specified type occurs.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
        }

        /// <summary>
        /// Adds a synchronous error processor with specified cancellation type for typed exceptions to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="actionProcessor">The action to execute when an exception of the specified type occurs.</param>
        /// <param name="cancellationType">The type of cancellation handling.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
        }

        /// <summary>
        /// Adds an asynchronous error processor for typed exceptions to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="funcProcessor">The asynchronous function to execute when an exception of the specified type occurs.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
        }

        /// <summary>
        /// Adds an asynchronous error processor with specified cancellation type for typed exceptions to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="funcProcessor">The asynchronous function to execute when an exception of the specified type occurs.</param>
        /// <param name="cancellationType">The type of cancellation handling.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
        }

        /// <summary>
        /// Adds an asynchronous error processor with cancellation token support for typed exceptions to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="funcProcessor">The asynchronous function to execute when an exception of the specified type occurs.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
        }

        /// <summary>
        /// Adds a typed error processor implementation to the bulk error processor.
        /// </summary>
        /// <typeparam name="TException">The type of exception to process.</typeparam>
        /// <param name="policyProcessor">The bulk error processor to add the error processor to.</param>
        /// <param name="errorProcessor">The typed error processor to add.</param>
        /// <returns>The bulk error processor with the typed error processor added.</returns>
        public static BulkErrorProcessor WithTypedErrorProcessor<TException>(this BulkErrorProcessor policyProcessor, DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
        {
            return policyProcessor.WithTypedErrorProcessor(errorProcessor, _addErrorProcessorAction);
        }
    }
}