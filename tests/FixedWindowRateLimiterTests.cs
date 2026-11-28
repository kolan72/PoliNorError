using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class FixedWindowRateLimiterTests
	{
		private static long _now;
		private static long Timestamp() => _now;

		[SetUp]
		public void SetUp() => _now = 0;

		[Test]
		public void Should_Enforce_PermitLimit_WithinWindow()
		{
			var limiter = new FixedWindowRateLimiter(2, TimeSpan.FromSeconds(10), Timestamp);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);

			var denied = limiter.TryAcquire();
			Assert.That(denied.IsPermitted, Is.False);
		}

		[Test]
		public void Should_Return_RetryAfter_EqualToTimeUntilWindowEnd()
		{
			var limiter = new FixedWindowRateLimiter(1, TimeSpan.FromSeconds(10), Timestamp);

			limiter.TryAcquire();
			var denied = limiter.TryAcquire();

			Assert.That(denied.RetryAfter, Is.EqualTo(TimeSpan.FromSeconds(10)));
		}

		[Test]
		public void Should_GrantFreshWindow_AfterRollover()
		{
			var limiter = new FixedWindowRateLimiter(1, TimeSpan.FromSeconds(10), Timestamp);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);

			_now = (long)(10 * Stopwatch.Frequency);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);
		}

		[Test]
		public void Should_StayDenied_BeforeWindowEnds()
		{
			var limiter = new FixedWindowRateLimiter(1, TimeSpan.FromSeconds(10), Timestamp);

			limiter.TryAcquire();

			_now = (long)(9.5 * Stopwatch.Frequency);

			var denied = limiter.TryAcquire();
			Assert.That(denied.IsPermitted, Is.False);
			Assert.That(denied.RetryAfter.Value, Is.GreaterThanOrEqualTo(TimeSpan.Zero));
			Assert.That(denied.RetryAfter.Value.TotalSeconds, Is.EqualTo(0.5).Within(0.01));

			_now = (long)(10 * Stopwatch.Frequency);

			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
		}

		[Test]
		public void Should_ThrowOperationCanceled_When_TokenIsCanceled()
		{
			var limiter = new FixedWindowRateLimiter(1, TimeSpan.FromSeconds(1), Timestamp);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				Assert.Throws<OperationCanceledException>(() => limiter.TryAcquire(cts.Token));
			}
		}

		[Test]
		public void Should_ThrowArgumentOutOfRange_When_InvalidArguments()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new FixedWindowRateLimiter(0, TimeSpan.FromSeconds(1)));
			Assert.Throws<ArgumentOutOfRangeException>(() => new FixedWindowRateLimiter(-1, TimeSpan.FromSeconds(1)));
			Assert.Throws<ArgumentOutOfRangeException>(() => new FixedWindowRateLimiter(1, TimeSpan.Zero));
			Assert.Throws<ArgumentOutOfRangeException>(() => new FixedWindowRateLimiter(1, TimeSpan.FromSeconds(-1)));
		}

		[Test]
		public void Should_WorkAsynchronously()
		{
			var limiter = new FixedWindowRateLimiter(1, TimeSpan.FromSeconds(1), Timestamp);

			Assert.That(limiter.TryAcquireAsync().Result.IsPermitted, Is.True);
			Assert.That(limiter.TryAcquireAsync().Result.IsPermitted, Is.False);
		}

		[Test]
		public void Should_RecoverFromBackwardTimeJump()
		{
			var limiter = new FixedWindowRateLimiter(2, TimeSpan.FromSeconds(10), Timestamp);

			// Use up all permits
			_now = 0;
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False);

			// Time goes backward (simulating clock adjustment)
			_now = -1000; // ← Negative!
			var result1 = limiter.TryAcquire();
			Assert.That(result1.IsPermitted, Is.False); // Still denied, permits not reset

			// Time moves forward by 10 seconds from the backward point
#pragma warning disable S1905 // Redundant casts should not be used
			_now = -1000 + (long)(10 * Stopwatch.Frequency);
#pragma warning restore S1905 // Redundant casts should not be used
			var result2 = limiter.TryAcquire();

			// WITHOUT FIX: This would fail - the window would never rotate
			// WITH FIX: The window start is updated to the backward time,
			// so after the window duration, permits are granted again
			Assert.That(result2.IsPermitted, Is.True);
		}

		[Test]
		public void Should_HandleBackwardTimeJump_WithoutResettingPermits()
		{
			var limiter = new FixedWindowRateLimiter(3, TimeSpan.FromSeconds(5), Timestamp);

			// Use one permit
			_now = 0;
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);

			// Time goes backward
			_now = -100;
			
			// Should still have permits available (we only used 1 of 3)
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.True);
			Assert.That(limiter.TryAcquire().IsPermitted, Is.False); // All 3 used
		}
	}
}
