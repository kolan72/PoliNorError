using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides an abstract base class for implementing custom error processing logic that operates on exceptions
	/// of a specific type <typeparamref name="TException"/>.
	/// </summary>
	/// <typeparam name="TException">
	/// The specific type of <see cref="Exception"/> that this processor is designed to handle.
	/// </typeparam>
	/// <remarks>
	/// <para>
	/// This class implements the <see cref="IErrorProcessor"/> interface and provides a common structure for both
	/// synchronous and asynchronous error processing. It delegates the core processing logic to the abstract
	/// <see cref="Execute"/> method, which derived classes must implement for the specific exception type.
	/// </para>
	/// </remarks>
	public abstract class TypedErrorProcessor<TException> : IErrorProcessor where TException : Exception
	{
		private readonly DefaultTypedErrorProcessor<TException> _errorProcessor;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypedErrorProcessor{TException}"/> class.
		/// </summary>
		/// <remarks>
		/// The constructor wires the <see cref="Execute"/> method into the internal
		/// <see cref="DefaultTypedErrorProcessor{TException}"/> pipeline.
		/// </remarks>
		protected TypedErrorProcessor()
		{
			_errorProcessor = new DefaultTypedErrorProcessor<TException>(Execute);
		}

		/// <summary>
		/// Processes the given exception synchronously by invoking the internal typed processor.
		/// </summary>
		/// <param name="error">The exception to be processed.</param>
		/// <param name="catchBlockProcessErrorInfo">Optional information about the context in which the exception was caught.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The original exception after processing.</returns>
		/// <remarks>
		/// This method serves as a synchronous wrapper that triggers the internal processing pipeline,
		/// eventually calling the overridden <see cref="Execute"/> method if the exception type matches.
		/// </remarks>
		public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			_errorProcessor.Process(error, catchBlockProcessErrorInfo, cancellationToken);
			return error;
		}

		/// <summary>
		/// Processes the given exception synchronously by invoking the <see cref="Process"/> method and returning the result via <c>Task.FromResult(error)</c>.
		/// </summary>
		/// <param name="error">The exception to be processed.</param>
		/// <param name="catchBlockProcessErrorInfo">Optional information about the context in which the exception was caught.</param>
		/// <param name="configAwait">A flag to configure the awaiter (not used in this implementation but available for compatibility).</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>A task that represents the asynchronous operation, containing the original exception after processing.</returns>
		/// <remarks>
		/// This method provides an asynchronous signature for the error processing logic, though the base implementation
		/// performs the work synchronously.
		/// </remarks>
		public Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			Process(error, catchBlockProcessErrorInfo, cancellationToken);
			return Task.FromResult(error);
		}

		/// <summary>
		/// The core processing logic for the specific exception type. This method must be implemented by inheriting classes.
		/// </summary>
		/// <param name="error">The typed exception to be processed.</param>
		/// <param name="catchBlockProcessErrorInfo">Optional information about the context in which the exception was caught.</param>
		/// <param name="token">A cancellation token.</param>
		/// <remarks>
		/// <strong>Inheritors of this class MUST implement this method</strong> to define the specific logic
		/// for handling exceptions of type <typeparamref name="TException"/>, such as specialized logging or alerting.
		/// </remarks>
		public abstract void Execute(TException error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken token = default);
	}
}
