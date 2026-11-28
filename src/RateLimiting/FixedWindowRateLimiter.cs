using System;
using System.Diagnostics;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// A rate limiter that allows a fixed number of permits per fixed time window.
	/// </summary>
	public sealed class FixedWindowRateLimiter : RateLimiterBase
	{
		private readonly object _sync = new object();

		private readonly int _permitLimit;
		private readonly long _windowDurationTicks;
		private readonly Func<long> _timestampProvider;

		private long _windowStart;
		private int _permitCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="FixedWindowRateLimiter"/> class.
		/// </summary>
		/// <param name="permitLimit">The maximum number of permits granted per window; must be greater than zero.</param>
		/// <param name="window">The length of the window; must be greater than zero.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="permitLimit"/> or <paramref name="window"/> is not positive.</exception>
		public FixedWindowRateLimiter(int permitLimit, TimeSpan window)
			: this(permitLimit, window, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FixedWindowRateLimiter"/> class with a custom timestamp provider.
		/// </summary>
		/// <param name="permitLimit">The maximum number of permits granted per window; must be greater than zero.</param>
		/// <param name="window">The length of the window; must be greater than zero.</param>
		/// <param name="timestampProvider">A function that returns the current timestamp in Stopwatch ticks, or null to use the default.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="permitLimit"/> or <paramref name="window"/> is not positive.</exception>
		internal FixedWindowRateLimiter(int permitLimit, TimeSpan window, Func<long> timestampProvider)
		{
			if (permitLimit <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(permitLimit), permitLimit, $"{nameof(permitLimit)} must be greater than zero.");
			}
			if (window <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(window), window, $"{nameof(window)} must be greater than zero.");
			}

			_permitLimit = permitLimit;
			_windowDurationTicks = (long)(window.TotalSeconds * Stopwatch.Frequency);
			_timestampProvider = timestampProvider ?? Stopwatch.GetTimestamp;
			_windowStart = _timestampProvider();
		}

		/// <summary>
		/// Synchronously attempts to acquire one permit within the current window.
		/// </summary>
		/// <param name="token">A cancellation token to cancel the acquisition attempt.</param>
		/// <returns>
		/// A permitted <see cref="RateLimitResult"/> when a permit is available, otherwise a denied result whose
		/// <see cref="RateLimitResult.RetryAfter"/> is the time left until the window ends.
		/// </returns>
		/// <exception cref="OperationCanceledException"><paramref name="token"/> has been canceled.</exception>
		public override RateLimitResult TryAcquire(CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();

			lock (_sync)
			{
				var now = _timestampProvider();
				RotateWindowIfNeeded(now);

				if (_permitCount < _permitLimit)
				{
					_permitCount++;
					return RateLimitResult.Permitted;
				}

				return RateLimitResult.Denied(GetRetryAfter(now));
			}
		}

		private void RotateWindowIfNeeded(long now)
		{
			var elapsed = now - _windowStart;
			
			// Handle backward time jumps: treat as if window just started
			if (elapsed < 0)
			{
				_windowStart = now;
				return;
			}
			
			if (elapsed >= _windowDurationTicks)
			{
				_windowStart = now;
				_permitCount = 0;
			}
		}

		private TimeSpan GetRetryAfter(long now)
		{
			var ticksUntilWindowEnd = _windowStart + _windowDurationTicks - now;
			
			// Should not happen in normal operation, but be defensive
			if (ticksUntilWindowEnd <= 0)
			{
				return TimeSpan.Zero;
			}
			
			return TimeSpan.FromSeconds(ticksUntilWindowEnd / (double)Stopwatch.Frequency);
		}
	}
}
