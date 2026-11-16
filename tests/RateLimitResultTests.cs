using NUnit.Framework;
using System;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class RateLimitResultTests
	{
		[Test]
		public void Should_Permitted_HaveNoRetryAfter()
		{
			var result = RateLimitResult.Permitted;

			Assert.That(result.IsPermitted, Is.True);
			Assert.That(result.RetryAfter, Is.Null);
		}

		[Test]
		public void Should_Denied_WithoutRetryAfter_DefaultToUnknownWait()
		{
			var result = RateLimitResult.Denied();

			Assert.That(result.IsPermitted, Is.False);
			Assert.That(result.RetryAfter, Is.Null);
		}

		[Test]
		public void Should_Denied_WithRetryAfter_PreserveValue()
		{
			var retryAfter = TimeSpan.FromSeconds(2);
			var result = RateLimitResult.Denied(retryAfter);

			Assert.That(result.IsPermitted, Is.False);
			Assert.That(result.RetryAfter, Is.EqualTo(retryAfter));
		}

		[Test]
		public void Should_Denied_ThrowArgumentOutOfRange_WhenRetryAfterIsNegative()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => RateLimitResult.Denied(TimeSpan.FromMilliseconds(-1)));
		}

		[Test]
		public void Should_BeEqual_When_SameState()
		{
			var retryAfter = TimeSpan.FromSeconds(1);

			Assert.That(RateLimitResult.Permitted, Is.EqualTo(RateLimitResult.Permitted));
			Assert.That(RateLimitResult.Denied(retryAfter), Is.EqualTo(RateLimitResult.Denied(retryAfter)));
			Assert.That(RateLimitResult.Denied() == RateLimitResult.Denied(null), Is.True);
		}

		[Test]
		public void Should_NotBeEqual_When_StateDiffers()
		{
			Assert.That(RateLimitResult.Permitted != RateLimitResult.Denied(), Is.True);
			Assert.That(RateLimitResult.Denied(TimeSpan.FromSeconds(1)), Is.Not.EqualTo(RateLimitResult.Denied(TimeSpan.FromSeconds(2))));
			Assert.That(RateLimitResult.Denied(TimeSpan.FromSeconds(1)), Is.Not.EqualTo(RateLimitResult.Denied()));
		}

		[Test]
		public void Should_DefaultBehaveAsDeniedWithUnknownWait()
		{
			var result = default(RateLimitResult);

			Assert.That(result.IsPermitted, Is.False);
			Assert.That(result.RetryAfter, Is.Null);
			Assert.That(result, Is.EqualTo(RateLimitResult.Denied()));
		}

		[Test]
		public void Should_HaveConsistentGetHashCode_ForEqualValues()
		{
			var retryAfter = TimeSpan.FromSeconds(3);

			Assert.That(RateLimitResult.Denied(retryAfter).GetHashCode(), Is.EqualTo(RateLimitResult.Denied(retryAfter).GetHashCode()));
			Assert.That(RateLimitResult.Permitted.GetHashCode(), Is.EqualTo(RateLimitResult.Permitted.GetHashCode()));
		}
	}
}
