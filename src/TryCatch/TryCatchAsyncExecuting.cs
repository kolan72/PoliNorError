using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	/// <summary>
	///	Provides extension helper methods to invoke ITryCatch.ExecuteAsync methods when the configureAwait parameter is set to false.
	/// </summary>
	public static class TryCatchAsyncExecuting
	{
		public static Task<TryCatchResult> ExecuteAsync(this ITryCatch tryCatch, Func<CancellationToken, Task> func, CancellationToken token = default)
									=> tryCatch.ExecuteAsync(func, false, token);

		public static Task<TryCatchResult<T>> ExecuteAsync<T>(this ITryCatch tryCatch, Func<CancellationToken, Task<T>> func, CancellationToken token = default)
									=> tryCatch.ExecuteAsync(func, false, token);
	}
}
