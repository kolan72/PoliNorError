using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Defines methods common to all SimplePolicy processors.
	/// </summary>
	public interface ISimplePolicyProcessor : IPolicyProcessor
	{
		/// <summary>
		/// Executes the delegate <paramref name="action"/> within the SimplePolicy processor.
		/// </summary>
		/// <param name="action">The delegate to handle.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns></returns>
		PolicyResult Execute(Action action, CancellationToken token = default);

		/// <summary>
		/// Executes the delegate <paramref name="func"/> within the SimplePolicy processor.
		/// </summary>
		/// <typeparam name="T">The type of the result</typeparam>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns></returns>
		PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default);

		/// <summary>
		/// Executes the delegate <paramref name="func"/> asynchronously within the SimplePolicy processor.
		/// </summary>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns></returns>
		Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		/// Executes the delegate <paramref name="func"/> asynchronously within the SimplePolicy processor.
		/// </summary>
		/// <typeparam name="T">The type of the result</typeparam>
		/// <param name="func">The delegate to handle.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns></returns>
		Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default);
	}
}