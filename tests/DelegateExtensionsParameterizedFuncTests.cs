using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Threading;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsParameterizedFuncTests
    {
        [Test]
        public void Should_InvokeWithSimple_ForParameterizedFunc_Work()
        {
            const int input = 21;
            int callCount = 0;
            int received = 0;
            Func<int, int> func = p =>
            {
                callCount++;
                received = p;
                return p * 2;
            };

            var result = func.InvokeWithSimple(input);

            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(received, Is.EqualTo(input));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(42));
            Assert.That(result.Errors.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Should_InvokeWithSimple_ForParameterizedFunc_CaptureErrorPolicyResult()
        {
            const string input = "abc";
            int errorProcessed = 0;
            int receivedLength = 0;
            Func<string, int> func = p =>
            {
                receivedLength = p.Length;
                throw new InvalidOperationException("failure");
            };

            var result = func.InvokeWithSimple(input, ErrorProcessorParam.From((Exception _) => errorProcessed++));

            Assert.That(receivedLength, Is.EqualTo(3));
            Assert.That(errorProcessed, Is.EqualTo(1));
            Assert.That(result.IsPolicySuccess, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_InvokeWithRetry_ForParameterizedFunc_KeepSameParameter_AndCaptureResult()
        {
            const int input = 5;
            int callCount = 0;
            int mismatches = 0;
            Func<int, int> func = p =>
            {
                callCount++;
                if (p != input)
                {
                    mismatches++;
                }

                if (callCount < 3)
                {
                    throw new Exception("retry");
                }

                return p + 10;
            };

            var result = func.InvokeWithRetry(input, retryCount: 2);

            Assert.That(callCount, Is.EqualTo(3));
            Assert.That(mismatches, Is.EqualTo(0));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(15));
        }

        [Test]
        public void Should_InvokeWithWaitAndRetry_ForParameterizedFunc_Work()
        {
            const int input = 8;
            const int retryCount = 1;
            int callsDelayOverload = 0;
            Func<int, int> funcDelay = p =>
            {
                callsDelayOverload++;
                throw new Exception($"delay-{p}");
            };

            int callsFuncOverload = 0;
            Func<int, int> funcRetryFunc = p =>
            {
                callsFuncOverload++;
                throw new Exception($"func-{p}");
            };

            var resultDelay = funcDelay.InvokeWithWaitAndRetry(input, retryCount, TimeSpan.Zero);
            var resultFunc = funcRetryFunc.InvokeWithWaitAndRetry(input, retryCount, (_, __) => TimeSpan.Zero);

            Assert.That(callsDelayOverload, Is.EqualTo(2));
            Assert.That(callsFuncOverload, Is.EqualTo(2));
            Assert.That(resultDelay.IsFailed, Is.True);
            Assert.That(resultFunc.IsFailed, Is.True);
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultFunc.Errors.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Should_InvokeWithRetryInfinite_ForParameterizedFunc_Work()
        {
            const string input = "token";
            int callCount = 0;
            int mismatches = 0;
            Func<string, int> func = p =>
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
                func.InvokeWithRetryInfinite<string, int>(input, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                func.InvokeWithWaitAndRetryInfinite<string, int>(input, TimeSpan.Zero, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                func.InvokeWithWaitAndRetryInfinite<string, int>(input, (_, __) => TimeSpan.Zero, token: cts.Token);
            }

            Assert.That(callCount, Is.GreaterThan(0));
            Assert.That(mismatches, Is.EqualTo(0));
        }

        [Test]
        public void Should_InvokeWithFallback_ForParameterizedFunc_PassParameterAndCaptureResult()
        {
            const int input = 300;
            int actionCalls = 0;
            int fallbackCalls = 0;
            int fallbackParam = 0;
            int errorProcessed = 0;

            Func<int, int> func = _ =>
            {
                actionCalls++;
                throw new Exception("fallback");
            };
            Func<int, int> fallback = p =>
            {
                fallbackCalls++;
                fallbackParam = p;
                return p + 1;
            };

            var result = func.InvokeWithFallback(input, fallback, ErrorProcessorParam.From((Exception _) => errorProcessed++));

            Assert.That(actionCalls, Is.EqualTo(1));
            Assert.That(fallbackCalls, Is.EqualTo(1));
            Assert.That(fallbackParam, Is.EqualTo(input));
            Assert.That(errorProcessed, Is.EqualTo(1));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(301));
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }
    }
}
