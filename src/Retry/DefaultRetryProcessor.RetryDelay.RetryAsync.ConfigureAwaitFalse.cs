using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class DefaultRetryProcessor
	{
		public Task<PolicyResult> RetryInfiniteWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, retryCountInfo, retryDelay, false, token);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, retryCountInfo, retryDelay, false, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> action, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryAsync(action, param, retryCountInfo, retryDelay, false, token);
		}

		public Task<PolicyResult> RetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), retryDelay, token);
		}
	}
}
