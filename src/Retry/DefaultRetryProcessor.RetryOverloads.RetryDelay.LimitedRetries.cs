using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class DefaultRetryProcessor
	{
		public PolicyResult Retry(Action action, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(action, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult<T> Retry<T>(Func<T> func, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(func, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(func, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> action, TParam param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(action, param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> action, TParam param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(action.Apply(param), param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}
	}
}
