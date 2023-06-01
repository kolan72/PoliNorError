using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface ISimplePolicyProcessor
	{
		PolicyResult Execute(Action action, CancellationToken token = default);
		PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default);
		Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);
		Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default);
	}
}