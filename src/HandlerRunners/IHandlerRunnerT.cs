using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IHandlerRunnerT : IHandlerRunnerBase
	{
		void Run<T>(PolicyResult<T> policyResult, CancellationToken token = default);
		Task RunAsync<T>(PolicyResult<T> policyResult, CancellationToken token = default);
	}
}
