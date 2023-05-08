using NUnit.Framework;

namespace PoliNorError.Tests
{
	internal class RetryCountInfoTests
	{
		[Test]
		public void Should_Infinite_Work()
		{
			var inf = RetryCountInfo.Infinite();
			Assert.IsTrue(inf.IsInfinite);
			Assert.AreEqual(int.MaxValue -1, inf.RetryCount);
			Assert.IsTrue(inf.CanRetry(int.MaxValue - 2));
			Assert.IsFalse(inf.CanRetry(int.MaxValue - 1));
		}

		[Test]
		[TestCase(0, 1)]
		[TestCase(-10, 1)]
		[TestCase(1, 1)]
		[TestCase(2, 2)]
		public void Should_Limited_Work(int limitedRetryCount, int expectedRetryCount)
		{
			var limited = RetryCountInfo.Limited(limitedRetryCount);
			Assert.AreEqual(false, limited.IsInfinite);
			Assert.AreEqual(expectedRetryCount, limited.RetryCount);
			Assert.IsTrue(limited.CanRetry(limited.RetryCount-1));
		}

		[Test]
		[TestCase(int.MaxValue - 1)]
		[TestCase(int.MaxValue)]
		public void Should_Limited_Work_ForMaxValue(int maxValue)
		{
			var limited = RetryCountInfo.Limited(maxValue);
			Assert.AreEqual(true, limited.IsInfinite);
			Assert.AreEqual(int.MaxValue - 1, limited.RetryCount);
			Assert.IsTrue(limited.CanRetry(limited.RetryCount - 1));
		}

		[Test]
		public void Should_Limited_MaxRetries_WithNeverFunc_Work()
		{
			var limited = RetryCountInfo.Limited(int.MaxValue, opt => opt.CanRetryInner = (_) => false);
			Assert.IsTrue(limited.IsInfinite);
			Assert.IsFalse(limited.CanRetry(0));
		}

		[Test]
		public void Should_WithNotZeroStartTryCount_Work()
		{
			var limited = RetryCountInfo.Limited(1, opt => opt.StartTryCount = int.MaxValue - 1);
			Assert.IsTrue(limited.CanRetry(int.MaxValue-2));
		}

		[Test]
		public void Should_LimitedFromOptions_Work()
		{
			var limited = RetryCountInfo.Limited(1, opt => { opt.CanRetryInner = (_) => false; opt.StartTryCount = 1; });
			Assert.IsTrue(limited.StartTryCount == 1);
			Assert.IsFalse(limited.CanRetry(1));
		}
	}
}