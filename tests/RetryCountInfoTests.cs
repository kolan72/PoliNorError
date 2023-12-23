using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PoliNorError.Tests
{
	internal class RetryCountInfoTests
	{
		[Test]
		public void Should_Infinite_Work()
		{
			var inf = RetryCountInfo.Infinite();
			ClassicAssert.IsTrue(inf.IsInfinite);
			ClassicAssert.AreEqual(int.MaxValue -1, inf.RetryCount);
			ClassicAssert.IsTrue(inf.CanRetry(int.MaxValue - 2));
			ClassicAssert.IsFalse(inf.CanRetry(int.MaxValue - 1));
		}

		[Test]
		[TestCase(0, 1)]
		[TestCase(-10, 1)]
		[TestCase(1, 1)]
		[TestCase(2, 2)]
		public void Should_Limited_Work(int limitedRetryCount, int expectedRetryCount)
		{
			var limited = RetryCountInfo.Limited(limitedRetryCount);
			ClassicAssert.AreEqual(false, limited.IsInfinite);
			ClassicAssert.AreEqual(expectedRetryCount, limited.RetryCount);
			ClassicAssert.IsTrue(limited.CanRetry(limited.RetryCount-1));
		}

		[Test]
		[TestCase(int.MaxValue - 1)]
		[TestCase(int.MaxValue)]
		public void Should_Limited_Work_ForMaxValue(int maxValue)
		{
			var limited = RetryCountInfo.Limited(maxValue);
			ClassicAssert.AreEqual(true, limited.IsInfinite);
			ClassicAssert.AreEqual(int.MaxValue - 1, limited.RetryCount);
			ClassicAssert.IsTrue(limited.CanRetry(limited.RetryCount - 1));
		}

		[Test]
		public void Should_Limited_MaxRetries_WithNeverFunc_Work()
		{
			var limited = RetryCountInfo.Limited(int.MaxValue, opt => opt.CanRetryInner = (_) => false);
			ClassicAssert.IsTrue(limited.IsInfinite);
			ClassicAssert.IsFalse(limited.CanRetry(0));
		}

		[Test]
		public void Should_WithNotZeroStartTryCount_Work()
		{
			var limited = RetryCountInfo.Limited(1, opt => opt.StartTryCount = int.MaxValue - 1);
			ClassicAssert.IsTrue(limited.CanRetry(int.MaxValue-2));
		}

		[Test]
		public void Should_LimitedFromOptions_Work()
		{
			var limited = RetryCountInfo.Limited(1, opt => { opt.CanRetryInner = (_) => false; opt.StartTryCount = 1; });
			ClassicAssert.IsTrue(limited.StartTryCount == 1);
			ClassicAssert.IsFalse(limited.CanRetry(1));
		}
	}
}