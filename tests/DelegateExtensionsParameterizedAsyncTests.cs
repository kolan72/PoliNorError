using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsParameterizedAsyncTests
    {
        [Test]
        public async Task Should_InvokeWithSimpleAsync_ForParameterizedAsyncFunc_Work()
        {
            const int input = 17;
            int calls = 0;
            int received = 0;
            int tokenSeen = 0;

            Func<int, CancellationToken, Task> func = async (p, token) =>
            {
                await Task.Delay(1, token);
                calls++;
                received = p;
                tokenSeen = token.CanBeCanceled ? 1 : 0;
                throw new InvalidOperationException("boom");
            };

            using (var cts = new CancellationTokenSource())
            {
                var result = await func.InvokeWithSimpleAsync(input, token: cts.Token);

                Assert.That(calls, Is.EqualTo(1));
                Assert.That(received, Is.EqualTo(input));
                Assert.That(tokenSeen, Is.EqualTo(1));
                Assert.That(result.IsPolicySuccess, Is.True);
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Should_InvokeWithSimpleAsync_ForParameterizedAsyncFunc_WithConfigureAwait_Work()
        {
            const string input = "param";
            int calls = 0;
            int errorCalls = 0;

            Func<string, CancellationToken, Task> func = (p, _) =>
            {
                calls += p == input ? 1 : 1000;
                throw new Exception("simple");
            };

            var result = await func.InvokeWithSimpleAsync(input, ErrorProcessorParam.From((Exception _) => errorCalls++), configureAwait: true);

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(errorCalls, Is.EqualTo(1));
            Assert.That(result.IsPolicySuccess, Is.True);
        }

        [Test]
        public async Task Should_InvokeWithRetryAsync_ForParameterizedAsyncFunc_KeepSameParameter()
        {
            const int input = 9;
            int calls = 0;
            int mismatches = 0;

            Func<int, CancellationToken, Task> func = (p, _) =>
            {
                calls++;
                if (p != input)
                {
                    mismatches++;
                }
                throw new Exception("retry");
            };

            var result = await func.InvokeWithRetryAsync(input, retryCount: 2);

            Assert.That(calls, Is.EqualTo(3));
            Assert.That(mismatches, Is.EqualTo(0));
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task Should_InvokeWithWaitAndRetryAsync_ForParameterizedAsyncFunc_Work()
        {
            const int input = 4;
            int callsDelay = 0;
            int callsFunc = 0;

            Func<int, CancellationToken, Task> funcDelay = (p, _) =>
            {
                callsDelay += p == input ? 1 : 1000;
                throw new Exception("delay");
            };

            Func<int, CancellationToken, Task> funcWithRetryFunc = (p, _) =>
            {
                callsFunc += p == input ? 1 : 1000;
                throw new Exception("delay-func");
            };

            var resultDelay = await funcDelay.InvokeWithWaitAndRetryAsync(input, retryCount: 1, TimeSpan.Zero);
            var resultFunc = await funcWithRetryFunc.InvokeWithWaitAndRetryAsync(input, retryCount: 1, (_, __) => TimeSpan.Zero);

            Assert.That(callsDelay, Is.EqualTo(2));
            Assert.That(callsFunc, Is.EqualTo(2));
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultFunc.Errors.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsync_ForParameterizedAsyncFunc_PassParameter()
        {
            const int input = 50;
            int calls = 0;
            int fallbackCalls = 0;
            int fallbackParam = 0;
            bool fallbackTokenCancelable = false;
            int errorCalls = 0;

            Func<int, CancellationToken, Task> func = (_, __) =>
            {
                calls++;
                throw new Exception("fallback");
            };

			Task fallback(int p, CancellationToken token)
			{
				fallbackCalls++;
				fallbackParam = p;
				fallbackTokenCancelable = token.CanBeCanceled;
				return Task.CompletedTask;
			}

			using (var cts = new CancellationTokenSource())
            {
                var result = await func.InvokeWithFallbackAsync(input, fallback, ErrorProcessorParam.From((Exception _) => errorCalls++), false, token: cts.Token);

                Assert.That(calls, Is.EqualTo(1));
                Assert.That(fallbackCalls, Is.EqualTo(1));
                Assert.That(fallbackParam, Is.EqualTo(input));
                Assert.That(fallbackTokenCancelable, Is.True);
                Assert.That(errorCalls, Is.EqualTo(1));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Should_InvokeWithRetryInfiniteAsync_ForParameterizedAsyncFunc_Work()
        {
            const string input = "same";
            int calls = 0;
            int mismatches = 0;

            Func<string, CancellationToken, Task> func = (p, _) =>
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
                await func.InvokeWithRetryInfiniteAsync(input, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithWaitAndRetryInfiniteAsync(input, TimeSpan.Zero, token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithWaitAndRetryInfiniteAsync(input, (_, __) => TimeSpan.Zero, token: cts.Token);
            }

            Assert.That(calls, Is.GreaterThan(0));
            Assert.That(mismatches, Is.EqualTo(0));
        }
    }
}
