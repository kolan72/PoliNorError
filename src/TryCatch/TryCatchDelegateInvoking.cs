using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	public static class TryCatchDelegateInvoking
	{
		/// <summary>
		/// Invokes the <see cref="Action"/> delegate and attempts to catch an exception using the <paramref name="tryCatch"/>.
		/// </summary>
		/// <param name="action">A delegate to invoke.</param>
		/// <param name="tryCatch"><see cref="ITryCatch"/></param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult"/></returns>
		public static TryCatchResult InvokeWithTryCatch(this Action action, ITryCatch tryCatch, CancellationToken token = default)
		{
			return tryCatch.Execute(action, token);
		}

		/// <summary>
		/// Invokes the <see cref="Func{T}"/> delegate and attempts to catch an exception using the <paramref name="tryCatch"/>.
		/// </summary>
		/// <typeparam name="T">The type of the return value of a delegate.</typeparam>
		/// <param name="func">A delegate to invoke.</param>
		/// <param name="tryCatch"><see cref="ITryCatch"/></param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult{T}"/></returns>
		public static TryCatchResult<T> InvokeWithTryCatch<T>(this Func<T> func, ITryCatch tryCatch, CancellationToken token = default)
		{
			return tryCatch.Execute(func, token);
		}

		/// <summary>
		/// Invokes Func&lt;CancellationToken, Task&gt; delegate and attempts to catch an exception using the <paramref name="tryCatch"/>.
		/// </summary>
		/// <param name="func">A delegate to invoke.</param>
		/// <param name="tryCatch"><see cref="ITryCatch"/></param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		/// <returns>Task&lt;TryCatchResult&gt;</returns>
		public static Task<TryCatchResult> InvokeWithTryCatchAsync(this Func<CancellationToken, Task> func, ITryCatch tryCatch, bool configureAwait = false, CancellationToken token = default)
		{
			return tryCatch.ExecuteAsync(func, configureAwait, token);
		}

		/// <summary>
		/// Invokes Func&lt;CancellationToken, Task&lt;T&gt;&gt; delegate and attempts to catch an exception using the <paramref name="tryCatch"/>.
		/// </summary>
		/// <param name="func">A delegate to invoke.</param>
		/// <param name="tryCatch"><see cref="ITryCatch"/></param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		/// <returns>Task&lt;TryCatchResult&lt;T&gt;&gt;</returns>
		public static Task<TryCatchResult<T>> InvokeWithTryCatchAsync<T>(this Func<CancellationToken, Task<T>> func, ITryCatch tryCatch, bool configureAwait = false, CancellationToken token = default)
		{
			return tryCatch.ExecuteAsync(func, configureAwait, token);
		}
	}
}
