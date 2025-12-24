using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;

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

        [Test]
        public void Should_CreateLimitedInstance_WithSpecifiedRetryCount()
        {
            var retryInfo = RetryCountInfo.Limited(3);

            Assert.That(retryInfo.RetryCount, Is.EqualTo(3));
            Assert.That(retryInfo.StartTryCount, Is.EqualTo(0));
            Assert.That(retryInfo.IsInfinite, Is.False);
        }

        [Test]
        public void Should_CreateInfiniteInstance_WithCorrectProperties()
        {
            var retryInfo = RetryCountInfo.Infinite();

            Assert.That(retryInfo.IsInfinite, Is.True);
            Assert.That(retryInfo.RetryCount, Is.EqualTo(int.MaxValue - 1));
            Assert.That(retryInfo.StartTryCount, Is.EqualTo(0));
        }

        [Test]
        public void Should_UseCustomStartTryCount_WhenProvided()
        {
            var retryInfo = new RetryCountInfo(3, null, 2);

            Assert.That(retryInfo.StartTryCount, Is.EqualTo(2));
            Assert.That(retryInfo.CanRetry(2), Is.True);  // 0 try 
            Assert.That(retryInfo.CanRetry(3), Is.True);  // 1 try
            Assert.That(retryInfo.CanRetry(4), Is.True);  // 2 try
            Assert.That(retryInfo.CanRetry(5), Is.False); // Third try (exceeds retry count)
        }

        [Test]
        public void Should_ApplyCustomCanRetryFunc_WhenProvided()
        {
			bool customFunc(int attempt) => attempt % 2 == 0; // Only allow even attempts
			var retryInfo = new RetryCountInfo(5, customFunc);

            Assert.That(retryInfo.CanRetry(0), Is.True);
            Assert.That(retryInfo.CanRetry(1), Is.False);
            Assert.That(retryInfo.CanRetry(2), Is.True);
            Assert.That(retryInfo.CanRetry(3), Is.False);
        }

        [Test]
        public void Should_CorrectRetryCount_ForZeroOrNegativeValues()
        {
            var retryInfo1 = RetryCountInfo.Limited(0);
            var retryInfo2 = RetryCountInfo.Limited(-5);

            Assert.That(retryInfo1.RetryCount, Is.EqualTo(1));
            Assert.That(retryInfo2.RetryCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_HandleVeryLargeRetryCount_WithoutBecomingInfinite()
        {
            var retryInfo = RetryCountInfo.Limited(int.MaxValue - 2);

            Assert.That(retryInfo.IsInfinite, Is.False);
        }

        [Test]
        public void Should_UseOptions_WhenCreatingLimitedInstance()
        {
            var retryInfo = RetryCountInfo.Limited(3, options =>
            {
                options.StartTryCount = 1;
                options.CanRetryInner = attempt => attempt <= 3;
            });

            Assert.That(retryInfo.StartTryCount, Is.EqualTo(1));
            Assert.That(retryInfo.CanRetry(1), Is.True);
            Assert.That(retryInfo.CanRetry(3), Is.True);
            Assert.That(retryInfo.CanRetry(4), Is.False);
        }

        [Test]
        public void Should_UseOptions_WhenCreatingInfiniteInstance()
        {
            var retryInfo = RetryCountInfo.Infinite(options =>
            {
                options.StartTryCount = 10;
                options.CanRetryInner = attempt => attempt < 100;
            });

            Assert.That(retryInfo.IsInfinite, Is.True);
            Assert.That(retryInfo.StartTryCount, Is.EqualTo(10));
            Assert.That(retryInfo.CanRetry(50), Is.True);
            Assert.That(retryInfo.CanRetry(100), Is.False);
        }

        [Test]
        public void Should_HandleBoundaryConditions_InCanRetry()
        {
            var retryInfo = RetryCountInfo.Limited(2);

            Assert.That(retryInfo.CanRetry(0), Is.True);
            Assert.That(retryInfo.CanRetry(1), Is.True);
            Assert.That(retryInfo.CanRetry(2), Is.False);
        }

        [Test]
        public void Should_CorrectIntMaxValue_ToRealInfiniteCount()
        {
            var retryInfo = RetryCountInfo.Limited(int.MaxValue);

            Assert.That(retryInfo.RetryCount, Is.EqualTo(int.MaxValue - 1));
        }

        [Test]
        public void Should_MaintainCorrectState_ForInfiniteRetries()
        {
            var retryInfo = RetryCountInfo.Infinite();

            Assert.That(retryInfo.CanRetry(0), Is.True);
            Assert.That(retryInfo.CanRetry(1000), Is.True);
            Assert.That(retryInfo.CanRetry(int.MaxValue - 2), Is.True);
            Assert.That(retryInfo.CanRetry(int.MaxValue - 1), Is.False);
        }
    }
}