using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IErrorProcessor
	{
		Task<Exception> ProcessAsync(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default);
		Exception Process(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default);
	}
}
