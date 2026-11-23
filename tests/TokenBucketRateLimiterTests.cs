using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class TokenBucketRateLimiterTests
	{
		private static long _now;
		private static long Timestamp() => _now;

		[SetUp]
		public void SetUp() => _now = 0;

		[Test]
		public void Should_AllowBurst_UpToCapacity()
		{
			var limiter = new TokenBucketRateLimiter(3, 1, Timestamp);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);
		}

		[Test]
		public void Should_Return_RetryAfter_ForOneToken_WhenEmpty()
		{
			var limiter = new TokenBucketRateLimiter(1, 1, Timestamp);

			limiter.TryAcquire();
			var denied = limiter.TryAcquire();

			Assert.That(denied.IsPermitted, Is.False);
			Assert.That(denied.RetryAfter.Value.TotalSeconds, Is.EqualTo(1.0).Within(0.001));
		}

		[Test]
		public void Should_RefillOverSimulatedTime()
		{
			var limiter = new TokenBucketRateLimiter(2, 1, Timestamp);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);

			_now += (long)(0.5 * Stopwatch.Frequency);

			var deniedHalf = limiter.TryAcquire();
			Assert.That(deniedHalf.IsPermitted, Is.False);
			Assert.That(deniedHalf.RetryAfter.Value.TotalSeconds, Is.EqualTo(0.5).Within(0.01));

			_now += (long)(0.5 * Stopwatch.Frequency);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);
		}

		[Test]
		public void Should_ClampTokens_ToCapacity_AfterLongIdle()
		{
			var limiter = new TokenBucketRateLimiter(2, 1, Timestamp);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);

			_now += (long)(100 * Stopwatch.Frequency);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);
		}

		[Test]
		public void Should_ThrowOperationCanceled_When_TokenIsCanceled()
		{
			var limiter = new TokenBucketRateLimiter(1, 1, Timestamp);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				Assert.Throws<OperationCanceledException>(() => limiter.TryAcquire(cts.Token));
			}
		}

		[Test]
		public void Should_ThrowArgumentOutOfRange_When_InvalidArguments()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(0.5, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(1, -1));

			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(double.NaN, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(double.PositiveInfinity, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(double.NegativeInfinity, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(1, double.NaN));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(1, double.PositiveInfinity));
			Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(1, double.NegativeInfinity));
		}

		[Test]
		public void Should_WorkAsynchronously()
		{
			var limiter = new TokenBucketRateLimiter(1, 1, Timestamp);

			Assert.That(limiter.TryAcquireAsync().Result.IsPermitted, Is.True);
			Assert.That(limiter.TryAcquireAsync().Result.IsPermitted, Is.False);
		}

		[Test]
		public void Should_RecoverFromBackwardTimeJump()
		{
			var limiter = new TokenBucketRateLimiter(1, 1, Timestamp);

			// Empty the bucket
			_now = 0;
			limiter.TryAcquire(); // tokens = 0, _lastTimestamp = 0

			// Time goes backward (simulating clock adjustment)
			_now = -1000; // ← Negative!
			var result1 = limiter.TryAcquire();
			Assert.That(result1.IsPermitted, Is.False); // Expected: no tokens yet

			// Time moves forward by 1 second from the backward point
			_now = -1000 + Stopwatch.Frequency; // 1 second later
			var result2 = limiter.TryAcquire();

			// WITHOUT FIX: (_lastTimestamp = now) This fails!
			// _lastTimestamp is still 0, so elapsed = (-1000 + Freq - 0) < 0
			// No refill happens

			// WITH FIX: This passes!
			// _lastTimestamp was updated to -1000
			// elapsed = Frequency (1 second), refills 1 token
			Assert.That(result2.IsPermitted, Is.True);
		}

	}
}
