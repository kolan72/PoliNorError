using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IRetryProcessor : IPolicyProcessor
	{
		PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, CancellationToken token = default);
		PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default);

		Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default);
		Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func,  RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default);
	}
}
