using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class DefaultRetryProcessor
	{
		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return RetryWithErrorContext(func, param, RetryCountInfo.Limited(retryCount), token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}
	}
}
