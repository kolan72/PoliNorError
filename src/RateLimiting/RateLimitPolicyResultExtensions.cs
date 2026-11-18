using System;

namespace PoliNorError
{
	/// <summary>
	/// Ergonomic accessors for rate-limiting outcomes stored in a <see cref="PolicyResult"/>.
	/// </summary>
	public static class RateLimitPolicyResultExtensions
	{
		/// <summary>
		/// Indicates whether the result represents a rate-limiting rejection
		/// (<see cref="PolicyResultFailedReason.RateLimited"/>).
		/// </summary>
		/// <param name="result">The policy result to inspect.</param>
		/// <returns><see langword="true"/> if the rate limit gate denied execution.</returns>
		public static bool IsRateLimited(this PolicyResult result) => result?.FailedReason == PolicyResultFailedReason.RateLimited;

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
