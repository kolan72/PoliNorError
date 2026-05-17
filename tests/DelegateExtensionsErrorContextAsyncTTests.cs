using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsErrorContextAsyncTTests
    {
        [Test]
        public async Task Should_InvokeWithSimpleAsyncT_ForErrorContextAsyncFunc_PassContextToProcessorOnly_AndCaptureResult()
        {
            const int errorContext = 140;
            int calls = 0;
            bool tokenCancelable = false;
            var logger = new TestLoggerWithParam();

            Func<CancellationToken, Task<int>> func = async token =>
            {
                await Task.Delay(1, token);
                calls++;
                tokenCancelable = token.CanBeCanceled;
                throw new InvalidOperationException("simple");
            };

            using (var cts = new CancellationTokenSource())
            {
                var result = await func.InvokeWithSimpleAsync<int, int>(errorContext, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)), token: cts.Token);

                Assert.That(calls, Is.EqualTo(1));
                Assert.That(tokenCancelable, Is.True);
                Assert.That(logger.LastLoggedException, Is.Not.Null);
                Assert.That(logger.Param, Is.EqualTo(errorContext));
                Assert.That(result.IsPolicySuccess, Is.True);
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Should_InvokeWithSimpleAsyncT_ForErrorContextAsyncFunc_SuccessResultWithoutParameterToDelegate()
        {
            const string errorContext = "ctx";
            int calls = 0;
            Func<CancellationToken, Task<int>> func = _ =>
            {
                calls++;
                return Task.FromResult(123);
            };

            var result = await func.InvokeWithSimpleAsync<string, int>(errorContext);

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(123));
            Assert.That(result.Errors.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task Should_InvokeWithSimpleAsyncT_ForErrorContextAsyncFunc_WithConfigureAwait_Work()
        {
            const int errorContext = 141;
            int calls = 0;
            var logger = new TestLoggerWithParam();

            Func<CancellationToken, Task<int>> func = _ =>
            {
                calls++;
                throw new Exception("cfg");
            };

            var result = await func.InvokeWithSimpleAsync<int, int>(errorContext, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)), configureAwait: true);

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(result.IsPolicySuccess, Is.True);
        }

        [Test]
        public async Task Should_InvokeWithRetryAsyncT_ForErrorContextAsyncFunc_UseSameContextAcrossRetries_AndCaptureResult()
        {
            const int errorContext = 201;
            const int retryCount = 2;
            int calls = 0;
            var logger = new TestLoggerWithParam();

            Func<CancellationToken, Task<int>> func = _ =>
            {
                calls++;
                if (calls < 3)
                {
                    throw new Exception("retry");
                }
                return Task.FromResult(77);
            };

            var result = await func.InvokeWithRetryAsync<int, int>(errorContext, retryCount, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)));

            Assert.That(calls, Is.EqualTo(3));
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(77));
        }

        [Test]
        public async Task Should_InvokeWithWaitAndRetryAsyncT_ForErrorContextAsyncFunc_Work()
        {
            const int errorContext = 202;
            int callsDelay = 0;
            int callsRetryFunc = 0;
            var loggerDelay = new TestLoggerWithParam();
            var loggerRetryFunc = new TestLoggerWithParam();

            Func<CancellationToken, Task<int>> funcDelay = _ =>
            {
                callsDelay++;
                throw new Exception("delay");
            };

            Func<CancellationToken, Task<int>> funcRetryFunc = _ =>
            {
                callsRetryFunc++;
                throw new Exception("delay-func");
            };

            var resultDelay = await funcDelay.InvokeWithWaitAndRetryAsync<int, int>(errorContext, retryCount: 1, TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(loggerDelay)));
            var resultRetryFunc = await funcRetryFunc.InvokeWithWaitAndRetryAsync<int, int>(errorContext, retryCount: 1, (_, __) => TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(loggerRetryFunc)));

            Assert.That(callsDelay, Is.EqualTo(2));
            Assert.That(callsRetryFunc, Is.EqualTo(2));
            Assert.That(loggerDelay.Param, Is.EqualTo(errorContext));
            Assert.That(loggerRetryFunc.Param, Is.EqualTo(errorContext));
            Assert.That(resultDelay.IsFailed, Is.True);
            Assert.That(resultRetryFunc.IsFailed, Is.True);
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultRetryFunc.Errors.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsyncT_ForErrorContextAsyncFunc_Work()
        {
            const int errorContext = 303;
            int mainCalls = 0;
            int fallbackWithTokenCalls = 0;
            int fallbackSimpleCalls = 0;
            bool fallbackTokenCancelable = false;
            var logger = new TestLoggerWithParam();

            Func<CancellationToken, Task<int>> func = _ =>
            {
                mainCalls++;
                throw new Exception("fallback");
            };

            Task<int> fallbackWithToken(CancellationToken token)
            {
                fallbackWithTokenCalls++;
                fallbackTokenCancelable = token.CanBeCanceled;
                return Task.FromResult(10);
            }

            Task<int> fallbackSimple()
            {
                fallbackSimpleCalls++;
                return Task.FromResult(20);
            }

            using (var cts = new CancellationTokenSource())
            {
                var resultWithToken = await func.InvokeWithFallbackAsync<int, int>(errorContext, fallbackWithToken, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)), token: cts.Token);
                var resultSimple = await func.InvokeWithFallbackAsync<int, int>(errorContext, fallbackSimple, ErrorProcessorParam.From(new LogErrorProcessorWithParam(new TestLoggerWithParam())), CancellationType.Precancelable, CancellationToken.None);

                Assert.That(mainCalls, Is.EqualTo(2));
                Assert.That(fallbackWithTokenCalls, Is.EqualTo(1));
                Assert.That(fallbackSimpleCalls, Is.EqualTo(1));
                Assert.That(fallbackTokenCancelable, Is.True);
                Assert.That(logger.Param, Is.EqualTo(errorContext));
                Assert.That(resultWithToken.IsSuccess, Is.True);
                Assert.That(resultWithToken.Result, Is.EqualTo(10));
                Assert.That(resultSimple.IsSuccess, Is.True);
                Assert.That(resultSimple.Result, Is.EqualTo(20));
            }
        }

        [Test]
        public async Task Should_InvokeWithRetryInfiniteAsyncT_ForErrorContextAsyncFunc_Work()
        {
            const int errorContext = 404;
            int calls = 0;
            var logger = new TestLoggerWithParam();

            Func<CancellationToken, Task<int>> func = _ =>
            {
                calls++;
                throw new Exception("inf");
            };

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithRetryInfiniteAsync<int, int>(errorContext, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)), token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithWaitAndRetryInfiniteAsync<int, int>(errorContext, TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(new TestLoggerWithParam())), token: cts.Token);
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);
                await func.InvokeWithWaitAndRetryInfiniteAsync<int, int>(errorContext, (_, __) => TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(new TestLoggerWithParam())), token: cts.Token);
            }

            Assert.That(calls, Is.GreaterThan(0));
            Assert.That(logger.Param, Is.EqualTo(errorContext));
        }
    }
}
