using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class ISimplePolicyProcessorExtensions
	{
		public static Task<PolicyResult> ExecuteAsync(this ISimplePolicyProcessor simplePolicyProcessor, Func<CancellationToken, Task> func, CancellationToken token)
															=> simplePolicyProcessor.ExecuteAsync(func, false, token);

		public static Task<PolicyResult<T>> ExecuteAsync<T>(this ISimplePolicyProcessor simplePolicyProcessor, Func<CancellationToken, Task<T>> func, CancellationToken token)
													=> simplePolicyProcessor.ExecuteAsync(func, false, token);
	}
}
