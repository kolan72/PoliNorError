using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides static helper methods to invoke IRetryProcessor.Retry or IRetryProcessor.RetryAsync methods
	/// </summary>
	public static class RetryProcessorExecuting
	{
		public static Task<PolicyResult<T>> RetryAsync<T>(this IRetryProcessor retryProcessor, Func<CancellationToken, Task<T>> func, int retryCount, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Limited(retryCount), token);

		public static Task<PolicyResult> RetryAsync(this IRetryProcessor retryProcessor, Func<CancellationToken, Task> func, int retryCount, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Limited(retryCount), token);

		public static Task<PolicyResult> RetryAsync(this IRetryProcessor retryProcessor, Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, CancellationToken token)
											=> retryProcessor.RetryAsync(func, retryCountInfo, false, token);

		public static Task<PolicyResult<T>> RetryAsync<T>(this IRetryProcessor retryProcessor, Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, CancellationToken token)
													=> retryProcessor.RetryAsync(func, retryCountInfo, false, token);

		public static PolicyResult<T> Retry<T>(this IRetryProcessor retryProcessor, Func<T> func, int retryCount, CancellationToken token = default)
													=> retryProcessor.Retry(func, RetryCountInfo.Limited(retryCount), token);

		public static PolicyResult Retry(this IRetryProcessor retryProcessor, Action action, int retryCount, CancellationToken token = default)
													=> retryProcessor.Retry(action, RetryCountInfo.Limited(retryCount), token);

		public static Task<PolicyResult<T>> RetryInfiniteAsync<T>(this IRetryProcessor retryProcessor, Func<CancellationToken, Task<T>> func, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Infinite(), token);

		public static Task<PolicyResult> RetryInfiniteAsync(this IRetryProcessor retryProcessor, Func<CancellationToken, Task> func, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Infinite(), token);

		public static PolicyResult<T> RetryInfinite<T>(this IRetryProcessor retryProcessor, Func<T> func, CancellationToken token = default)
												=> retryProcessor.Retry(func, RetryCountInfo.Infinite(), token);

		public static PolicyResult RetryInfinite(this IRetryProcessor retryProcessor, Action action, CancellationToken token = default)
													=> retryProcessor.Retry(action, RetryCountInfo.Infinite(), token);
	}
}
