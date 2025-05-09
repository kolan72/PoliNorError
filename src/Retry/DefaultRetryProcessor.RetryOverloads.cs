﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class DefaultRetryProcessor
	{
		public PolicyResult Retry(Action action, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(action, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryInternal(action, retryCountInfo, retryDelay, _retryErrorContextCreator, token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(action.Apply(param), param, retryCountInfo, retryDelay, token);
		}

		public PolicyResult<T> Retry<T>(Func<T> func, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(func, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryInternal(func, retryCountInfo, retryDelay, _retryErrorContextCreator, token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(func.Apply(param), param, retryCountInfo, retryDelay, token);
		}

		public PolicyResult RetryInfinite(Action action, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(action, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public PolicyResult RetryInfinite<TParam>(Action<TParam> action, TParam param, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public PolicyResult<T> RetryInfinite<TParam, T>(Func<TParam, T> func, TParam param, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public PolicyResult<T> RetryInfinite<T>(Func<T> func, RetryDelay retryDelay, CancellationToken token = default)
		{
			return Retry(func, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public Task<PolicyResult> RetryInfiniteAsync(Func<CancellationToken, Task> func, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, RetryCountInfo.Infinite(), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task> action, TParam param, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(action, param, RetryCountInfo.Infinite(), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteAsync<T>(Func<CancellationToken, Task<T>> func, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, RetryCountInfo.Infinite(), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), retryDelay, configureAwait, token);
		}

		public PolicyResult RetryInfiniteWithErrorContext<TErrorContext>(Action action, TErrorContext param, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Infinite(), retryDelay,token);
		}

		public PolicyResult<T> RetryInfiniteWithErrorContext<TErrorContext, T>(Func<T> action, TErrorContext param, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Infinite(), retryDelay, token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(action, retryCountInfo, retryDelay, retryErrorContextCreator, token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, int retryCount, RetryDelay retryDelay, CancellationToken token = default)
		{
			return RetryWithErrorContext(func, param, RetryCountInfo.Limited(retryCount), retryDelay, token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(func, retryCountInfo, retryDelay, retryErrorContextCreator, token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public async Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return await RetryInternalAsync(func, retryCountInfo, retryDelay, retryErrorContextCreator, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public async Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return await RetryInternalAsync(func, retryCountInfo, retryDelay, retryErrorContextCreator, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public Task<PolicyResult> RetryInfiniteWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> action, TParam param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(action, param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> action, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(action.Apply(param), param, retryCountInfo, retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> action, TParam param, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(action.Apply(param), param, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> action, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(action.Apply(param), param, retryCountInfo, retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, retryDelay, _retryErrorContextCreator, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, int retryCount, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, RetryCountInfo.Limited(retryCount), retryDelay, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, retryDelay, _retryErrorContextCreator, configureAwait, token);
		}
	}
}
