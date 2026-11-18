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
	}
}
