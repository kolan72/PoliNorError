using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IErrorProcessorExtensions
	{
		public static Task<Exception> ProcessAsync(this IErrorProcessor errorProcessor,
														Exception error,
														ProcessingErrorInfo catchBlockProcessErrorInfo,
														CancellationToken cancellationToken = default) =>
			errorProcessor.ProcessAsync(error, catchBlockProcessErrorInfo, false, cancellationToken);
	}
}
