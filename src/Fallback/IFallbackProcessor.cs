using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Defines methods common to all Fallback processors.
	/// </summary>
	public interface IFallbackProcessor : IPolicyProcessor
	{
		PolicyResult Fallback(Action action, Action<CancellationToken> fallback, CancellationToken token = default);
		PolicyResult<T> Fallback<T>(Func<T> func, Func<CancellationToken, T> fallback, CancellationToken token = default);

		Task<PolicyResult> FallbackAsync(Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallback, bool configureAwait = false, CancellationToken token = default);
		Task<PolicyResult<T>> FallbackAsync<T>(Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallback, bool configureAwait = false, CancellationToken token = default);
	}
}
