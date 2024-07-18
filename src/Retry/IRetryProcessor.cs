using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Specifies methods for a retry processor.
	/// </summary>
	public interface IRetryProcessor : IPolicyProcessor
	{
		/// <summary>
		/// Retries <paramref name="action"/> if an exception occurs.
		/// </summary>
		/// <param name="action">The delegate to handle.</param>
		/// <param name="retryCountInfo"><see cref="RetryCountInfo"/></param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult"/></returns>
		PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, CancellationToken token = default);

		/// <summary>
		/// Retries <paramref name="func"/> if an exception occurs.
		/// </summary>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="retryCountInfo"><see cref="RetryCountInfo"/></param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult{T}"/></returns>
		PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default);

		/// <summary>
		/// Retries <paramref name="func"/> asynchronously if an exception occurs.
		/// </summary>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="retryCountInfo"><see cref="RetryCountInfo"/></param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult"/></returns>
		Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		///  Retries <paramref name="func"/> asynchronously if an exception occurs.
		/// </summary>
		/// <typeparam name="T">The type of the result.</typeparam>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="retryCountInfo"><see cref="RetryCountInfo"/></param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult{T}"/></returns>
		Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func,  RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		/// Specifies <see cref="IErrorProcessor"/> used to store exception not in <see cref="PolicyResult.Errors"/> property
		/// </summary>
		/// <param name="saveErrorProcessor">Error processor</param>
		/// <returns></returns>
		IRetryProcessor UseCustomErrorSaver(IErrorProcessor saveErrorProcessor);
	}
}
