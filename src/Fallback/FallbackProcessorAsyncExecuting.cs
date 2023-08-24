using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	///	Provides extension helper methods to invoke IFallbackProcessor.FallbackAsync methods when the configureAwait parameter is set to false.
	/// </summary>
	public static class FallbackProcessorAsyncExecuting
	{
		public static Task<PolicyResult> FallbackAsync(this IFallbackProcessor fallbackProcessor, Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallback, CancellationToken token)
																=> fallbackProcessor.FallbackAsync(func, fallback, false, token);

		public static Task<PolicyResult<T>> FallbackAsync<T>(this IFallbackProcessor fallbackProcessor, Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallback, CancellationToken token)
															=> fallbackProcessor.FallbackAsync(func, fallback, false, token);
	}
}
