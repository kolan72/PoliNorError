using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IHandlerRunner : IHandlerRunnerBase
	{
		void Run(PolicyResult policyResult, CancellationToken token = default);

		Task RunAsync(PolicyResult policyResult, CancellationToken token = default);
	}
}
