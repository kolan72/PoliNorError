using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides extension helper methods to invoke IErrorProcessor.ProcessAsync method when the configureAwait parameter is set to false.
	/// </summary>
	public static class ErrorProcessorAsyncProcessing
	{
		public static Task<Exception> ProcessAsync(this IErrorProcessor errorProcessor,
														Exception error,
														ProcessingErrorInfo catchBlockProcessErrorInfo,
														CancellationToken cancellationToken = default) =>
			errorProcessor.ProcessAsync(error, catchBlockProcessErrorInfo, false, cancellationToken);
	}
}
