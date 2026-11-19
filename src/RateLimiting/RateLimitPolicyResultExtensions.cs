using System;

namespace PoliNorError
{
	/// <summary>
	/// Ergonomic accessors for rate-limiting outcomes stored in a <see cref="PolicyResult"/>.
	/// </summary>
	public static class RateLimitPolicyResultExtensions
	{
		/// <summary>
		/// Indicates whether the policy declined to execute the delegate
		/// (<see cref="PolicyResultFailedReason.ExecutionDenied"/>).
		/// </summary>
		/// <param name="result">The policy result to inspect.</param>
		/// <returns><see langword="true"/> if the policy declined to execute the delegate.</returns>
		public static bool IsExecutionDenied(this PolicyResult result) => result?.FailedReason == PolicyResultFailedReason.ExecutionDenied;

		/// <summary>
		/// Gets the wait time carried by the <see cref="RateLimitException"/> of a rate-limiting rejection.
		/// </summary>
		/// <param name="result">The policy result to inspect.</param>
		/// <returns>
		/// The <see cref="RateLimitException.RetryAfter"/> value of the recorded rejection,
		/// or <see langword="null"/> if the result is not a rate-limiting rejection or the wait time is unknown.
		/// </returns>
		public static TimeSpan? GetRetryAfter(this PolicyResult result) => (result?.LastError as RateLimitException)?.RetryAfter;
	}
}
