using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IErrorProcessorRunner<T>
	{
		ErrorProcessorRunResult Run(Exception error, T t, CancellationToken token = default);
		Task<ErrorProcessorRunResult> RunAsync(Exception error, T t, bool configAwait = false, CancellationToken token = default);
	}
}