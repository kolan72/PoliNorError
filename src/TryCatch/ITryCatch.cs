using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	public interface ITryCatch
	{
		TryCatchResult Execute(Action action, CancellationToken token = default);
		Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);
	}
}
