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
		}

		[Test]
		public void Should_WorkAsynchronously()
		{
			var limiter = new TokenBucketRateLimiter(1, 1, Timestamp);

			Assert.That(limiter.TryAcquireAsync().Result.IsPermitted, Is.True);
			Assert.That(limiter.TryAcquireAsync().Result.IsPermitted, Is.False);
		}
	}
}
