using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides static extension methods to invoke policy HandleAsync methods when the configureAwait parameter is set to false.
	/// </summary>
	public static class PolicyAsyncHandling
	{
		public static Task<PolicyResult> HandleAsync(this IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token)
		{
			return policyBase.HandleAsync(func, false, token);
		}

		public static Task<PolicyResult<T>> HandleAsync<T>(this IPolicyBase policyBase, Func<CancellationToken, Task<T>> func, CancellationToken token)
		{
			return policyBase.HandleAsync(func, false, token);
		}
	}
}
