using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IErrorProcessor
	{
		Task<Exception> ProcessAsync(Exception error, ProcessErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default);
		Exception Process(Exception error, ProcessErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default);
	}
}
