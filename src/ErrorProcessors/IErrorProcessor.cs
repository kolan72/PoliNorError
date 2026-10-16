using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
/// <summary>
/// Defines a contract for processing errors.
/// </summary>
	public interface IErrorProcessor
	{
		/// <summary>
		/// Processes the error asynchronously.
		/// </summary>
		/// <param name="error">The error to process.</param>
		/// <param name="catchBlockProcessErrorInfo">Information about the error that triggered processing.</param>
		/// <param name="configAwait">How to await the operation.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The processed exception.</returns>
		Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default);
		/// <summary>
		/// Processes the error synchronously.
		/// </summary>
		/// <param name="error">The error to process.</param>
		/// <param name="catchBlockProcessErrorInfo">Information about the error that triggered processing.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The processed exception.</returns>
		Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default);
	}
}
