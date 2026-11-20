using System;

namespace PoliNorError
{
	/// <summary>
	/// The error recorded in a <see cref="PolicyResult"/> when a <see cref="RateLimitPolicy"/> denies execution.
	/// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
	public class RateLimitException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RateLimitException"/> class.
		/// </summary>
		/// <param name="retryAfter">The time to wait before the next attempt may succeed; <see langword="null"/> when unknown.</param>
		public RateLimitException(TimeSpan? retryAfter) : this(null, retryAfter) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="RateLimitException"/> class with a message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="retryAfter">The time to wait before the next attempt may succeed; <see langword="null"/> when unknown.</param>
		public RateLimitException(string message, TimeSpan? retryAfter) : base(message)
		{
			RetryAfter = retryAfter;
		}

		/// <summary>
		/// Gets the time to wait before the next attempt may succeed; <see langword="null"/> when unknown.
		/// </summary>
		public TimeSpan? RetryAfter { get; }
	}
}
