using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IFallbackProcessorExtensions
	{
		public static Task<PolicyResult> FallbackAsync(this IFallbackProcessor fallbackProcessor, Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallback, CancellationToken token)
																=> fallbackProcessor.FallbackAsync(func, fallback, false, token);

		public static Task<PolicyResult<T>> FallbackAsync<T>(this IFallbackProcessor fallbackProcessor, Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallback, CancellationToken token)
															=> fallbackProcessor.FallbackAsync(func, fallback, false, token);

		public static IFallbackProcessor IncludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.IncludeError<IFallbackProcessor, TException>(func);

		public static IFallbackProcessor ExcludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.ExcludeError<IFallbackProcessor, TException>(func);
	}
}
