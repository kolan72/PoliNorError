using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IHandlerRunner : IHandlerRunnerBase
	{
		void Run(PolicyResult policyResult, CancellationToken token = default);

		Task RunAsync(PolicyResult policyResult, CancellationToken token = default);
	}

	public interface IHandlerRunnerBase
	{
		int CollectionIndex { get; }
		bool SyncRun { get; }
	}
}
