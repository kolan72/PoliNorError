using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class DefaultRetryProcessor
	{
		public Task<PolicyResult> RetryInfiniteWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, retryCountInfo, false, token);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, retryCountInfo, false, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, RetryCountInfo retryCountInfo, CancellationToken token)
		{
			return RetryAsync(func, param, retryCountInfo, null, false, token);
		}

		public Task<PolicyResult> RetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, CancellationToken token)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), token);
		}
	}
}
