using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsParameterizedAsyncTTests
    {
        [Test]
        public async Task Should_InvokeWithSimpleAsyncT_ForParameterizedAsyncFunc_Work()
        {
            const int input = 11;
            int calls = 0;
            int received = 0;
            bool tokenCancelable = false;

            Func<int, CancellationToken, Task<int>> func = async (p, token) =>
            {
                await Task.Delay(1, token);
                calls++;
                received = p;
                tokenCancelable = token.CanBeCanceled;
                return p * 3;
            };

            using (var cts = new CancellationTokenSource())
            {
                var result = await func.InvokeWithSimpleAsync(input, token: cts.Token);

                Assert.That(calls, Is.EqualTo(1));
                Assert.That(received, Is.EqualTo(input));
                Assert.That(tokenCancelable, Is.True);
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.EqualTo(33));
                Assert.That(result.Errors.Count(), Is.EqualTo(0));
            }
        }

        [Test]
        public async Task Should_InvokeWithSimpleAsyncT_ForParameterizedAsyncFunc_WithErrorProcessor_Work()
        {
            const string input = "abc";
            int errorsProcessed = 0;
            int lengthSeen = 0;

            Func<string, CancellationToken, Task<int>> func = (p, _) =>
            {
                lengthSeen = p.Length;
                throw new InvalidOperationException("simple-fail");
            };

            var result = await func.InvokeWithSimpleAsync(input, ErrorProcessorParam.From((Exception _) => errorsProcessed++), configureAwait: true);

            Assert.That(lengthSeen, Is.EqualTo(3));
            Assert.That(errorsProcessed, Is.EqualTo(1));
            Assert.That(result.IsPolicySuccess, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_InvokeWithRetryAsyncT_ForParameterizedAsyncFunc_KeepSameParameter_AndCaptureResult()
        {
            const int input = 6;
            int calls = 0;
            int mismatches = 0;

            Func<int, CancellationToken, Task<int>> func = (p, _) =>
            {
                calls++;
                if (p != input)
                {
                    mismatches++;
                }

                if (calls < 3)
                {
                    throw new Exception("retry");
                }

                return Task.FromResult(p + 100);
            };

            var result = await func.InvokeWithRetryAsync(input, retryCount: 2);

            Assert.That(calls, Is.EqualTo(3));
            Assert.That(mismatches, Is.EqualTo(0));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(106));
        }

        [Test]
        public async Task Should_InvokeWithWaitAndRetryAsyncT_ForParameterizedAsyncFunc_Work()
        {
            const int input = 13;
            int delayCalls = 0;
            int retryFuncCalls = 0;

            Func<int, CancellationToken, Task<int>> funcDelay = (p, _) =>
            {
                delayCalls += p == input ? 1 : 1000;
                throw new Exception("delay");
            };

            Func<int, CancellationToken, Task<int>> funcRetryFunc = (p, _) =>
            {
                retryFuncCalls += p == input ? 1 : 1000;
                throw new Exception("retry-func");
            };

            var resultDelay = await funcDelay.InvokeWithWaitAndRetryAsync(input, retryCount: 1, TimeSpan.Zero);
            var resultRetryFunc = await funcRetryFunc.InvokeWithWaitAndRetryAsync(input, retryCount: 1, (_, __) => TimeSpan.Zero);

            Assert.That(delayCalls, Is.EqualTo(2));
            Assert.That(retryFuncCalls, Is.EqualTo(2));
            Assert.That(resultDelay.IsFailed, Is.True);
            Assert.That(resultRetryFunc.IsFailed, Is.True);
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultRetryFunc.Errors.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsyncT_ForParameterizedAsyncFunc_PassParameter_AndCaptureResult()
        {
            const int input = 90;
            int mainCalls = 0;
            int fallbackCalls = 0;
            int fallbackParam = 0;
            bool fallbackTokenCancelable = false;
            int errorsProcessed = 0;

            Func<int, CancellationToken, Task<int>> func = (_, __) =>
            {
                mainCalls++;
                throw new Exception("fallback");
            };

            Func<int, CancellationToken, Task<int>> fallback = (p, token) =>
            {
                fallbackCalls++;
                fallbackParam = p;
                fallbackTokenCancelable = token.CanBeCanceled;
                return Task.FromResult(p + 5);
            };

            using (var cts = new CancellationTokenSource())
            {
                var result = await func.InvokeWithFallbackAsync(input, fallback, ErrorProcessorParam.From((Exception _) => errorsProcessed++), token: cts.Token);

                Assert.That(mainCalls, Is.EqualTo(1));
                Assert.That(fallbackCalls, Is.EqualTo(1));
                Assert.That(fallbackParam, Is.EqualTo(input));
                Assert.That(fallbackTokenCancelable, Is.True);
                Assert.That(errorsProcessed, Is.EqualTo(1));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.EqualTo(95));
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Should_InvokeWithRetryInfiniteAsyncT_ForParameterizedAsyncFunc_Work()
        {
            const string input = "same-param";
            int calls = 0;
            int mismatches = 0;

            Func<string, CancellationToken, Task<int>> func = (p, _) =>
            {
                calls++;
                if (p != input)
                {
                    mismatches++;
                }
                throw new Exception("inf");
            };

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithRetryInfiniteAsync<string, int>(input, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithWaitAndRetryInfiniteAsync<string, int>(input, TimeSpan.Zero, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithWaitAndRetryInfiniteAsync<string, int>(input, (_, __) => TimeSpan.Zero, token: cts.Token);
            }

            Assert.That(calls, Is.GreaterThan(0));
            Assert.That(mismatches, Is.EqualTo(0));
        }
    }
}
