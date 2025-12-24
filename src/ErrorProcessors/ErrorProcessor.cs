using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides an abstract base class for processing errors.
	/// </summary>
	/// <remarks>
	/// This class provides a common structure for synchronous and asynchronous error processing methods,
	/// delegating the actual processing logic to the concrete implementation of the <see cref="Execute"/> method.
	/// </remarks>
	public abstract class ErrorProcessor : IErrorProcessor
	{
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
			Execute(error, catchBlockProcessErrorInfo, cancellationToken);
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
			Execute(error, catchBlockProcessErrorInfo, cancellationToken);
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
		public abstract void Execute(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken token = default);
	}
}
