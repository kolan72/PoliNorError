using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
    /// <summary>
    /// Provides an abstract base class for implementing custom error processing logic that operates on exceptions
    /// and optional contextual information of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <typeparam name="TParam">
    /// The type of the optional contextual parameter provided in <see cref="ProcessingErrorInfo{TParam}"/>
    /// during error processing.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This class implements the <see cref="IErrorProcessor"/> interface and provides a common structure for both
    /// synchronous and asynchronous error processing. It delegates the core processing logic to the abstract
    /// <see cref="Execute"/> method, which derived classes must implement.
    /// </para>
    /// </remarks>
    public abstract class ErrorProcessor<TParam> : IErrorProcessor
    {
        private readonly DefaultErrorProcessor<TParam> _errorProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorProcessor{TParam}"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor wires the <see cref="Execute"/> method into the internal
        /// <see cref="DefaultErrorProcessor{TParam}"/> pipeline.
        /// </remarks>
        protected ErrorProcessor()
        {
            _errorProcessor = new DefaultErrorProcessor<TParam>(Execute);
        }

        /// <summary>
        /// Processes the given exception synchronously by invoking the overridden <see cref="Execute"/> method.
        /// </summary>
        /// <param name="error">The exception to be processed.</param>
        /// <param name="catchBlockProcessErrorInfo">Optional information about the context in which the exception was caught.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The original exception after processing.</returns>
        /// <remarks>
        /// This method serves as a synchronous wrapper around the abstract <see cref="Execute"/> method.
        /// Inheritors should place their processing logic within the <see cref="Execute"/> method.
        /// </remarks>
        public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
        {
            _errorProcessor.Process(error, catchBlockProcessErrorInfo, cancellationToken);
            return error;
        }

        /// <summary>
        /// Processes the given exception synchronously by invoking the overridden <see cref="Execute"/> method and returning the result via <c>Task.FromResult(error)</c>.
        /// </summary>
        /// <param name="error">The exception to be processed.</param>
        /// <param name="catchBlockProcessErrorInfo">Optional information about the context in which the exception was caught.</param>
        /// <param name="configAwait">A flag to configure the awaiter (not used in the base implementation but available for inheritors).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original exception after processing.</returns>
        /// <remarks>
        /// This method serves as an asynchronous wrapper around the abstract <see cref="Execute"/> method.
        /// Inheritors should place their processing logic within the <see cref="Execute"/> method.
        /// </remarks>
        public Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
        {
            Process(error, catchBlockProcessErrorInfo, cancellationToken);
            return Task.FromResult(error);
        }

        /// <summary>
        /// The core processing logic for the exception. This method must be implemented by inheriting classes.
        /// </summary>
        /// <param name="error">The exception to be processed.</param>
        /// <param name="catchBlockProcessErrorInfo">Optional information about the context in which the exception was caught.</param>
        /// <param name="token">A cancellation token.</param>
        /// <remarks>
        /// <strong>Inheritors of this class MUST implement this method</strong> to define the specific error handling logic,
        /// such as logging, reporting, or transforming the exception.
        /// </remarks>
        public abstract void Execute(Exception error, ProcessingErrorInfo<TParam> catchBlockProcessErrorInfo = null, CancellationToken token = default);
    }
}
