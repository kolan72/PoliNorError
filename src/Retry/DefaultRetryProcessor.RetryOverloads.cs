using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class DefaultRetryProcessor
	{
		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(action, param, retryCountInfo, null, token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryWithErrorContext(func.Apply(param), param, retryCountInfo, token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(action, retryCountInfo, null, retryErrorContextCreator, token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return RetryWithErrorContext(func, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(func, retryCountInfo, null, retryErrorContextCreator, token);
		}

		public PolicyResult RetryInfiniteWithErrorContext<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult<T> RetryInfiniteWithErrorContext<TErrorContext, T>(Func<T> action, TErrorContext param, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult RetryInfinite<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult<T> RetryInfinite<TParam, T>(Func<TParam, T> func, TParam param, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Infinite(), token);
		}

		public Task<PolicyResult> RetryInfiniteWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public async Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return await RetryInternalAsync(func, retryCountInfo, null, retryErrorContextCreator, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public async Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return await RetryInternalAsync(func, retryCountInfo, null, retryErrorContextCreator, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, param, retryCountInfo, null, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func.Apply(param), param, retryCountInfo, configureAwait, token);
		}

		public Task<PolicyResult> RetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}
	}
}
