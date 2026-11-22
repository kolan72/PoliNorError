using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		[TestCase(double.NaN, 1, "capacity", TestName = "Should_RejectNaNCapacity")]
		[TestCase(double.PositiveInfinity, 1, "capacity", TestName = "Should_RejectInfiniteCapacity")]
		[TestCase(double.NegativeInfinity, 1, "capacity", TestName = "Should_RejectNegativeInfiniteCapacity")]
		[TestCase(1, double.NaN, "refillRatePerSecond", TestName = "Should_RejectNaNRefillRate")]
		[TestCase(1, double.PositiveInfinity, "refillRatePerSecond", TestName = "Should_RejectInfiniteRefillRate")]
		[TestCase(1, double.NegativeInfinity, "refillRatePerSecond", TestName = "Should_RejectNegativeInfiniteRefillRate")]
		public void Should_ThrowArgumentOutOfRange_When_ArgumentIsNotFinite(double capacity, double refillRatePerSecond, string parameterName)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(capacity, refillRatePerSecond));
			Assert.That(exception.ParamName, Is.EqualTo(parameterName));
		}

		[TestCase(double.Epsilon, TestName = "Should_ReturnUnknownRetryAfter_When_InfiniteRateProducesOverflow")]
		public void Should_ReturnUnknownRetryAfter_When_DelayCannotBeRepresented(double refillRatePerSecond)
		{
			var limiter = new TokenBucketRateLimiter(1, refillRatePerSecond, Timestamp);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);

			var denied = limiter.TryAcquire();

			Assert.That(denied.IsPermitted, Is.False);
			Assert.That(denied.RetryAfter, Is.Null);
		}

		[Test]
		public void Should_ReturnOneTickRetryAfter_When_PositiveDelayRoundsBelowOneTick()
		{
			var limiter = new TokenBucketRateLimiter(1, TimeSpan.TicksPerSecond * 2d, Timestamp);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);

			var denied = limiter.TryAcquire();

			Assert.That(denied.IsPermitted, Is.False);
			Assert.That(denied.RetryAfter, Is.EqualTo(TimeSpan.FromTicks(1)));
		}

		[Test]
		public void Should_NotOverspendTokens_UnderConcurrentAcquisition()
		{
			const int capacity = 5;
			const int callers = 32;
			var limiter = new TokenBucketRateLimiter(capacity, 1, Timestamp);

			var results = new RateLimitResult[callers];
			using (var start = new ManualResetEventSlim(false))
			{
				var tasks = Enumerable.Range(0, callers).Select(i => Task.Run(() =>
				{
					Assert.That(start.Wait(TimeSpan.FromSeconds(10)), Is.True, "Timed out waiting to start concurrent acquisition.");
					results[i] = limiter.TryAcquire();
				})).ToArray();

				start.Set();
				Task.WaitAll(tasks);
			}

			Assert.That(results.Count(result => result.IsPermitted), Is.EqualTo(capacity));
			Assert.That(results.Count(result => !result.IsPermitted), Is.EqualTo(callers - capacity));
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);
		}

		[Test]
		public async Task Should_WorkAsynchronously()
		{
			var limiter = new TokenBucketRateLimiter(1, 1, Timestamp);

			var permittedTask = limiter.TryAcquireAsync();
			Assert.That(permittedTask.IsCompleted, Is.True);
			Assert.That((await permittedTask.ConfigureAwait(false)).IsPermitted, Is.True);

			var deniedTask = limiter.TryAcquireAsync();
			Assert.That(deniedTask.IsCompleted, Is.True);
			Assert.That((await deniedTask.ConfigureAwait(false)).IsPermitted, Is.False);
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
