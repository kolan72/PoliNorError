using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsErrorContextActionTests
    {
        [Test]
        public void Should_InvokeWithSimple_ForErrorContextAction_PassContextToProcessorOnly()
        {
            const int errorContext = 77;
            int actionCalls = 0;
            var logger = new TestLoggerWithParam();
            var processor = new LogErrorProcessorWithParam(logger);

            Action action = () =>
            {
                actionCalls++;
                throw new InvalidOperationException("simple");
            };

            var result = action.InvokeWithSimple(errorContext, ErrorProcessorParam.From(processor));

            Assert.That(actionCalls, Is.EqualTo(1));
            Assert.That(logger.LastLoggedException, Is.Not.Null);
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(result.IsPolicySuccess, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_InvokeWithRetry_ForErrorContextAction_UseSameContextAcrossRetries()
        {
            const int errorContext = 15;
            const int retryCount = 2;
            int actionCalls = 0;
            var logger = new TestLoggerWithParam();
            var processor = new LogErrorProcessorWithParam(logger);

            Action action = () =>
            {
                actionCalls++;
                throw new Exception("retry");
            };

            var result = action.InvokeWithRetry(errorContext, retryCount, ErrorProcessorParam.From(processor));

            Assert.That(actionCalls, Is.EqualTo(retryCount + 1));
            Assert.That(logger.LastLoggedException, Is.Not.Null);
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.Count(), Is.EqualTo(retryCount + 1));
        }

        [Test]
        public void Should_InvokeWithWaitAndRetry_ForErrorContextAction_Work()
        {
            const int errorContext = 120;
            int actionCallsWithDelay = 0;
            int actionCallsWithRetryFunc = 0;
            var loggerWithDelay = new TestLoggerWithParam();
            var loggerWithRetryFunc = new TestLoggerWithParam();

            Action actionWithDelay = () =>
            {
                actionCallsWithDelay++;
                throw new Exception("delay");
            };

            Action actionWithRetryFunc = () =>
            {
                actionCallsWithRetryFunc++;
                throw new Exception("retry-func");
            };

            var resultDelay = actionWithDelay.InvokeWithWaitAndRetry(errorContext, retryCount: 1, TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(loggerWithDelay)));
            var resultRetryFunc = actionWithRetryFunc.InvokeWithWaitAndRetry(errorContext, retryCount: 1, (_, __) => TimeSpan.Zero, ErrorProcessorParam.From(new LogErrorProcessorWithParam(loggerWithRetryFunc)));

            Assert.That(actionCallsWithDelay, Is.EqualTo(2));
            Assert.That(actionCallsWithRetryFunc, Is.EqualTo(2));
            Assert.That(loggerWithDelay.Param, Is.EqualTo(errorContext));
            Assert.That(loggerWithRetryFunc.Param, Is.EqualTo(errorContext));
            Assert.That(resultDelay.Errors.Count(), Is.EqualTo(2));
            Assert.That(resultRetryFunc.Errors.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Should_InvokeWithFallback_ForErrorContextAction_Work()
        {
            const int errorContext = 501;
            int actionCalls = 0;
            int fallbackWithTokenCalls = 0;
            int fallbackActionCalls = 0;
            bool fallbackTokenCancelable = false;
            var logger = new TestLoggerWithParam();

            Action action = () =>
            {
                actionCalls++;
                throw new Exception("fallback");
            };

            void fallbackWithToken(CancellationToken token)
            {
                fallbackWithTokenCalls++;
                fallbackTokenCancelable = token.CanBeCanceled;
            }

            void fallbackAction()
            {
                fallbackActionCalls++;
            }

            var resultWithToken = action.InvokeWithFallback(errorContext, fallbackWithToken, ErrorProcessorParam.From(new LogErrorProcessorWithParam(logger)));
            var resultAction = action.InvokeWithFallback(errorContext, fallbackAction, ErrorProcessorParam.From(new LogErrorProcessorWithParam(new TestLoggerWithParam())), CancellationType.Precancelable, CancellationToken.None);

            Assert.That(actionCalls, Is.EqualTo(2));
            Assert.That(fallbackWithTokenCalls, Is.EqualTo(1));
            Assert.That(fallbackActionCalls, Is.EqualTo(1));
            Assert.That(fallbackTokenCancelable, Is.False);
            Assert.That(logger.Param, Is.EqualTo(errorContext));
            Assert.That(resultWithToken.IsSuccess, Is.True);
            Assert.That(resultAction.IsSuccess, Is.True);
        }
    }
}
