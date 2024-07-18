using System;

namespace PoliNorError
{
	/// <summary>
	/// Contains options for configuring <see cref="RetryCountInfo"/>.
	/// </summary>
	public class RetryCountInfoOptions
	{
		/// <summary>
		/// Func to check if retry can continue with number of retries parameter.
		/// </summary>
		public Func<int, bool> CanRetryInner { get; set; }

		/// <summary>
		///  The number of retries from which we will start.
		/// </summary>
		public int StartTryCount { get; set; }
	}
}
