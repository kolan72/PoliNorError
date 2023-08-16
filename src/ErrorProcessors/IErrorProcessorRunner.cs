using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IErrorProcessorRunner<T>
	{
		ErrorProcessorRunResul Run(Exception error, T t, CancellationToken token = default);
		Task<ErrorProcessorRunResul> RunAsync(Exception error, T t, bool configAwait = false, CancellationToken token = default);
	}
}