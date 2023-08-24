using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides static helper methods to invoke ISimplePolicyProcessor.ExecuteAsync methods when the configureAwait parameter is set to false.
	/// </summary>
	public static class SimplePolicyProcessorAsyncExecuting
	{
		public static Task<PolicyResult> ExecuteAsync(this ISimplePolicyProcessor simplePolicyProcessor, Func<CancellationToken, Task> func, CancellationToken token)
													=> simplePolicyProcessor.ExecuteAsync(func, false, token);

		public static Task<PolicyResult<T>> ExecuteAsync<T>(this ISimplePolicyProcessor simplePolicyProcessor, Func<CancellationToken, Task<T>> func, CancellationToken token)
													=> simplePolicyProcessor.ExecuteAsync(func, false, token);
	}
}
