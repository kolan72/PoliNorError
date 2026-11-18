using System;
using System.Diagnostics;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// A rate limiter that grants permits from a bucket refilled continuously at a fixed rate.
	/// </summary>
	public sealed class TokenBucketRateLimiter : RateLimiterBase
	{
		private readonly object _sync = new object();

		private readonly double _capacity;
		private readonly double _refillRatePerSecond;
		private readonly Func<long> _timestampProvider;

		private double _tokens;
		private long _lastTimestamp;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenBucketRateLimiter"/> class.
		/// The bucket starts full.
		/// </summary>
		/// <param name="capacity">The maximum number of tokens; must be at least 1.</param>
		/// <param name="refillRatePerSecond">The number of tokens added per second; must be greater than zero.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1 or <paramref name="refillRatePerSecond"/> is not positive.</exception>
		public TokenBucketRateLimiter(double capacity, double refillRatePerSecond)
			: this(capacity, refillRatePerSecond, null)
		{
		}

		internal TokenBucketRateLimiter(double capacity, double refillRatePerSecond, Func<long> timestampProvider)
		{
			if (capacity < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"{nameof(capacity)} must be greater than or equal to 1.");
			}
			if (refillRatePerSecond <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(refillRatePerSecond), refillRatePerSecond, $"{nameof(refillRatePerSecond)} must be greater than zero.");
			}

			_capacity = capacity;
			_refillRatePerSecond = refillRatePerSecond;
			_timestampProvider = timestampProvider ?? Stopwatch.GetTimestamp;
			_tokens = capacity;
			_lastTimestamp = _timestampProvider();
		}

		/// <summary>
		/// Synchronously attempts to acquire one token from the bucket.
		/// </summary>
		/// <param name="token">A cancellation token to cancel the acquisition attempt.</param>
		/// <returns>
		/// A permitted <see cref="RateLimitResult"/> when at least one token is available, otherwise a denied result whose
		/// <see cref="RateLimitResult.RetryAfter"/> is the time needed to accumulate one token.
		/// </returns>
		/// <exception cref="OperationCanceledException"><paramref name="token"/> has been canceled.</exception>
		public override RateLimitResult TryAcquire(CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();

			lock (_sync)
			{
				var now = _timestampProvider();
				Refill(now);

				if (_tokens >= 1)
				{
					_tokens -= 1;
					return RateLimitResult.Permitted;
				}

				var secondsUntilToken = (1 - _tokens) / _refillRatePerSecond;
				return RateLimitResult.Denied(TimeSpan.FromSeconds(secondsUntilToken));
			}
		}

		private void Refill(long now)
		{
			var elapsedSeconds = (now - _lastTimestamp) / (double)Stopwatch.Frequency;
			if (elapsedSeconds <= 0)
			{
				return;
			}
			_tokens = Math.Min(_capacity, _tokens + elapsedSeconds * _refillRatePerSecond);
			_lastTimestamp = now;
		}
	}
}
