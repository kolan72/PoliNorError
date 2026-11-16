using System;

namespace PoliNorError
{
	/// <summary>
	/// Represents the outcome of an attempt to acquire a permit from an <see cref="IRateLimiter"/>.
	/// </summary>
	public readonly struct RateLimitResult : IEquatable<RateLimitResult>
	{
		private RateLimitResult(bool isPermitted, TimeSpan? retryAfter)
		{
			IsPermitted = isPermitted;
			RetryAfter = retryAfter;
		}

		/// <summary>
		/// Gets a <see cref="RateLimitResult"/> that grants permission to execute.
		/// </summary>
		public static RateLimitResult Permitted { get; } = new RateLimitResult(true, null);

		/// <summary>
		/// Creates a <see cref="RateLimitResult"/> that denies permission to execute.
		/// </summary>
		/// <param name="retryAfter">
		/// The time to wait before the next attempt may succeed.
		/// <see langword="null"/> means the wait time is unknown.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="retryAfter"/> is negative.</exception>
		public static RateLimitResult Denied(TimeSpan? retryAfter = null)
		{
			if (retryAfter < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(retryAfter), retryAfter, "RetryAfter cannot be negative.");
			}
			return new RateLimitResult(false, retryAfter);
		}

		/// <summary>
		/// Indicates whether the permit was granted.
		/// </summary>
		public bool IsPermitted { get; }

		/// <summary>
		/// Gets the time to wait before the next attempt may succeed.
		/// <see langword="null"/> means either the permit was granted or the wait time is unknown.
		/// Ignored when <see cref="IsPermitted"/> is <see langword="true"/>.
		/// </summary>
		public TimeSpan? RetryAfter { get; }

		public bool Equals(RateLimitResult other) => IsPermitted == other.IsPermitted && RetryAfter == other.RetryAfter;

		public override bool Equals(object obj) => obj is RateLimitResult other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = IsPermitted.GetHashCode();
				hashCode = (hashCode * 397) ^ (RetryAfter.HasValue ? RetryAfter.Value.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(RateLimitResult left, RateLimitResult right) => left.Equals(right);

		public static bool operator !=(RateLimitResult left, RateLimitResult right) => !left.Equals(right);
	}
}
