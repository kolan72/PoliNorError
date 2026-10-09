using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Defines methods for handling delegates synchronously and asynchronously using error processing policies.
	/// </summary>
	public interface IPolicyBase
	{
		/// <summary>
		/// Synchronously handles the specified <paramref name="action"/> delegate.
		/// </summary>
		/// <param name="action">The delegate to handle.</param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult"/></returns>
		PolicyResult Handle(Action action, CancellationToken token = default);

		/// <summary>
		/// Synchronously handles the specified <paramref name="func"/> delegate.
		/// </summary>
		/// <typeparam name="T">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult{T}"/></returns>
		PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default);

		/// <summary>
		/// Asynchronously handles the specified <paramref name="func"/> delegate.
		/// </summary>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult"/></returns>
		Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		/// Asynchronously handles the specified <paramref name="func"/> delegate.
		/// </summary>
		/// <typeparam name="T">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token">A cancellation token to cancel handling.</param>
		/// <returns><see cref="PolicyResult{T}"/></returns>
		Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		/// Gets the <see cref="IPolicyProcessor"/> associated with this policy.
		/// </summary>
		IPolicyProcessor PolicyProcessor { get; }

		/// <summary>
		/// Gets the name of this policy.
		/// </summary>
		string PolicyName { get; }
	}
}
