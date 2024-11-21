using System;
using System.Threading;

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
	}
}
