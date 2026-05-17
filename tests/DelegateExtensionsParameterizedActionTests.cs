using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Threading;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsParameterizedActionTests
    {
        [Test]
        public void Should_InvokeWithSimple_ForParameterizedAction_Work()
        {
            const int input = 42;
            int callCount = 0;
            int received = 0;
            Action<int> action = p => { callCount++; received = p; throw new InvalidOperationException("boom"); };

            var result = action.InvokeWithSimple(input);

            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(received, Is.EqualTo(input));
            Assert.That(result.IsPolicySuccess, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_InvokeWithRetry_ForParameterizedAction_KeepSameParameter()
        {
            const int input = 7;
            const int retryCount = 2;
            int callCount = 0;
            int mismatches = 0;
            Action<int> action = p =>
            {
                callCount++;
                if (p != input)
                {
                    mismatches++;
                }
                throw new Exception("retry");
            };

            var result = action.InvokeWithRetry(input, retryCount);

            Assert.That(callCount, Is.EqualTo(retryCount + 1));
            Assert.That(mismatches, Is.EqualTo(0));
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(retryCount + 1));
        }

        [Test]
        public void Should_InvokeWithWaitAndRetry_ForParameterizedAction_Work()
        {
            const int input = 99;
            const int retryCount = 1;
            int callCount = 0;
            Action<int> action = p =>
            {
                callCount += p == input ? 1 : 1000;
                throw new Exception("wait");
            };

            var resultDelay = action.InvokeWithWaitAndRetry(input, retryCount, TimeSpan.Zero);
            var resultFunc = action.InvokeWithWaitAndRetry(input, retryCount, (_, __) => TimeSpan.Zero);

            Assert.That(callCount, Is.EqualTo(4));
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultFunc.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultDelay.IsFailed, Is.True);
            Assert.That(resultFunc.IsFailed, Is.True);
        }

        [Test]
        public void Should_InvokeWithRetryInfinite_ForParameterizedAction_Work()
        {
            const string input = "fixed-param";
            int callCount = 0;
            int mismatches = 0;
            Action<string> action = p =>
            {
                callCount++;
                if (p != input)
                {
                    mismatches++;
                }
                throw new Exception("infinite");
            };

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                action.InvokeWithRetryInfinite(input, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                action.InvokeWithWaitAndRetryInfinite(input, TimeSpan.Zero, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                action.InvokeWithWaitAndRetryInfinite(input, (_, __) => TimeSpan.Zero, token: cts.Token);
            }

            Assert.That(callCount, Is.GreaterThan(0));
            Assert.That(mismatches, Is.EqualTo(0));
        }

        [Test]
        public void Should_InvokeWithFallback_ForParameterizedAction_PassParameterToFallback()
        {
            const int input = 314;
            int actionCalls = 0;
            int fallbackCalls = 0;
            int fallbackParam = 0;
            int errorProcessed = 0;

            Action<int> action = _ =>
            {
                actionCalls++;
                throw new Exception("needs fallback");
            };
            Action<int> fallback = p =>
            {
                fallbackCalls++;
                fallbackParam = p;
            };

            var result = action.InvokeWithFallback(input, fallback, ErrorProcessorParam.From((Exception _) => errorProcessed++));

            Assert.That(actionCalls, Is.EqualTo(1));
            Assert.That(fallbackCalls, Is.EqualTo(1));
            Assert.That(fallbackParam, Is.EqualTo(input));
            Assert.That(errorProcessed, Is.EqualTo(1));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_InvokeWithSimple_ForParameterizedAction_WithErrorProcessor_Work()
        {
            const int input = 123;
            int actionCalls = 0;
            int errorCalls = 0;
            Action<int> action = _ =>
            {
                actionCalls++;
                throw new Exception("error");
            };

            var result = action.InvokeWithSimple(input, ErrorProcessorParam.From((Exception _) => errorCalls++));

            ClassicAssert.AreEqual(1, actionCalls);
            ClassicAssert.AreEqual(1, errorCalls);
            ClassicAssert.IsTrue(result.IsPolicySuccess);
            ClassicAssert.AreEqual(1, result.Errors.Count());
        }
    }
}
