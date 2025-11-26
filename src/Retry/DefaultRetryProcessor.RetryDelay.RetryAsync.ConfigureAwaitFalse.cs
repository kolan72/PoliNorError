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

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			return RetryWithErrorContextAsync(func, param, retryCountInfo, retryDelay, false, token);
		}
	}
}
