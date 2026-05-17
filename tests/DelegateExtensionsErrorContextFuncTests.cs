using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsErrorContextFuncTests
    {
        [Test]
        public void Should_InvokeWithSimple_ForErrorContextFunc_PassContextToProcessorOnly_AndCaptureResult()
        {
            const int errorContext = 33;
            int funcCalls = 0;
            var logger = new TestLoggerWithParam();
            var processor = new LogErrorProcessorWithParam(logger);

            Func<int> func = () =>
            {
                funcCalls++;
                throw new InvalidOperationException("simple");
            };

            var result = func.InvokeWithSimple<int, int>(errorContext, ErrorProcessorParam.From(processor));

            Assert.That(funcCalls, Is.EqualTo(1));
            Assert.That(logger.LastLoggedException, Is.Not.Null);
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(result.IsPolicySuccess, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_InvokeWithSimple_ForErrorContextFunc_ReturnValueWithoutParameterToDelegate()
        {
            const string errorContext = "ctx";
            int funcCalls = 0;
            Func<int> func = () =>
            {
                funcCalls++;
                return 42;
            };

            var result = func.InvokeWithSimple<string, int>(errorContext);

            Assert.That(funcCalls, Is.EqualTo(1));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Result, Is.EqualTo(42));
            Assert.That(result.Errors.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Should_InvokeWithRetry_ForErrorContextFunc_UseSameContextAcrossRetries()
        {
            const int errorContext = 88;
            const int retryCount = 2;
            int funcCalls = 0;
            var logger = new TestLoggerWithParam();

            Func<int> func = () =>
            {
                funcCalls++;
                throw new Exception("retry");
            };

            var result = func.InvokeWithRetry<int, int>(errorContext, retryCount, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)));

            Assert.That(funcCalls, Is.EqualTo(retryCount + 1));
            Assert.That(logger.LastLoggedException, Is.Not.Null);
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(retryCount + 1));
        }

        [Test]
        public void Should_InvokeWithWaitAndRetry_ForErrorContextFunc_Work()
        {
            const int errorContext = 101;
            int callsDelay = 0;
            int callsRetryFunc = 0;
            var loggerDelay = new TestLoggerWithParam();
            var loggerRetryFunc = new TestLoggerWithParam();

            Func<int> funcDelay = () =>
            {
                callsDelay++;
                throw new Exception("delay");
            };

            Func<int> funcRetryFunc = () =>
            {
                callsRetryFunc++;
                throw new Exception("retry-func");
            };

            var resultDelay = funcDelay.InvokeWithWaitAndRetry<int, int>(errorContext, retryCount: 1, TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(loggerDelay)));
            var resultRetryFunc = funcRetryFunc.InvokeWithWaitAndRetry<int, int>(errorContext, retryCount: 1, (_, __) => TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(loggerRetryFunc)));

            Assert.That(callsDelay, Is.EqualTo(2));
            Assert.That(callsRetryFunc, Is.EqualTo(2));
            Assert.That(loggerDelay.Param, Is.EqualTo(errorContext));
            Assert.That(loggerRetryFunc.Param, Is.EqualTo(errorContext));
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultRetryFunc.Errors.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Should_InvokeWithFallback_ForErrorContextFunc_Work()
        {
            const int errorContext = 777;
            int funcCalls = 0;
            int fallbackWithTokenCalls = 0;
            int fallbackSimpleCalls = 0;
            bool fallbackTokenCancelable = false;
            var logger = new TestLoggerWithParam();

            Func<int> func = () =>
            {
                funcCalls++;
                throw new Exception("fallback");
            };

            int fallbackWithToken(CancellationToken token)
            {
                fallbackWithTokenCalls++;
                fallbackTokenCancelable = token.CanBeCanceled;
                return 10;
            }

            int fallbackSimple()
            {
                fallbackSimpleCalls++;
                return 20;
            }

            var resultWithToken = func.InvokeWithFallback<int, int>(errorContext, fallbackWithToken, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)));
            var resultSimple = func.InvokeWithFallback<int, int>(errorContext, fallbackSimple, ErrorProcessorParam.From(new LogErrorProcessorWithParam(new TestLoggerWithParam())), CancellationType.Precancelable, CancellationToken.None);

            Assert.That(funcCalls, Is.EqualTo(2));
            Assert.That(fallbackWithTokenCalls, Is.EqualTo(1));
            Assert.That(fallbackSimpleCalls, Is.EqualTo(1));
            Assert.That(fallbackTokenCancelable, Is.False);
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(resultWithToken.IsSuccess, Is.True);
            Assert.That(resultWithToken.Result, Is.EqualTo(10));
            Assert.That(resultSimple.IsSuccess, Is.True);
            Assert.That(resultSimple.Result, Is.EqualTo(20));
        }
    }
}
