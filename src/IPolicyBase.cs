using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IPolicyBase
	{
		PolicyResult Handle(Action action, CancellationToken token = default);
		PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default);

		Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);
		Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default);

		IPolicyProcessor PolicyProcessor { get; }

		string PolicyName { get; }
	}
}
