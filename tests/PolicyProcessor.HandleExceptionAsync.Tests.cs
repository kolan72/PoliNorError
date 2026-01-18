using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.BulkErrorProcessor;

namespace PoliNorError.Tests
{
	[TestFixture]
	public partial class PolicyProcessorTests
	{
		[Test]
        public async Task Should_ReturnHandled_WhenHandlingBehaviorIsHandle()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
        }

        [Test]
        public async Task Should_AddErrorToPolicyResult_WhenErrorSaverIsNull()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(policyResult.NoError, Is.False);
        }

        [Test]
        public async Task Should_CallCustomErrorSaver_WhenProvided()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");
            var errorSaverCalled = false;

			Task errorSaver(PolicyResult _, Exception __, ErrorContext<string> ___, bool ____, CancellationToken _____)
			{
				errorSaverCalled = true;
				return Task.CompletedTask;
			}

			await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                errorSaver,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(errorSaverCalled, Is.True);
        }

        [Test]
        public async Task Should_SetFailedAndCanceled_WhenTokenIsCanceledAfterErrorSaver()
		{
			var processor = new TestPolicyProcessor(null);
			var policyResult = new PolicyResult(true);
			var exception = new Exception("Test exception");
			var errorContext = new TestErrorContext("test");
			using (var cts = new CancellationTokenSource())
			{
				Task errorSaver(PolicyResult _, Exception __, ErrorContext<string> ___, bool ____, CancellationToken _____)
				{
					cts.Cancel();
					return Task.CompletedTask;
				}

				await processor.TestHandleExceptionAsync(
					exception,
					policyResult,
					errorContext,
					errorSaver,
					null,
					ExceptionHandlingBehavior.Handle,
					ErrorProcessingCancellationEffect.Ignore,
					false,
					cts.Token);
			}

			Assert.That(policyResult.IsFailed, Is.True);
			Assert.That(policyResult.IsCanceled, Is.True);
		}

		[Test]
        public async Task Should_ReturnRethrow_WhenPolicyRuleFuncReturnsFalseAndBehaviorIsConditionalRethrow()
        {
            var processor = new TestPolicyProcessor(null);
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

			bool policyRuleFunc(ErrorContext<string> _) => false;

			var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                policyRuleFunc,
                ExceptionHandlingBehavior.ConditionalRethrow,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
        }

        [Test]
        public async Task Should_CallBulkErrorProcessor_WhenExceptionIsAccepted()
        {
            var bulkProcessor = new TestBulkErrorProcessor();
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(policyResult.IsSuccess, Is.True);
        }

        [Test]
        public async Task Should_AddBulkProcessorErrorsToPolicyResult()
        {
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ProcessErrors = new[] { new ErrorProcessorException(new InvalidCastException(), null, ProcessStatus.Faulted) }
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(policyResult.IsSuccess, Is.True);
        }

        [Test]
        public async Task Should_SetFailedAndCanceled_WhenBulkProcessorIsCanceledAndEffectIsPropagate()
        {
            var bulkProcessor = new TestBulkErrorProcessor
            {
                IsCanceled = true
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Propagate,
                false,
                CancellationToken.None);

            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.True);
        }

        [Test]
        public async Task Should_NotSetCanceled_WhenBulkProcessorIsCanceledAndEffectIsIgnore()
        {
            var bulkProcessor = new TestBulkErrorProcessor
            {
                IsCanceled = true
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(policyResult.IsCanceled, Is.False);
        }

        [Test]
        public async Task Should_CallErrorSaverBeforeDeterminingResult_WhenBehaviorIsNotConditionalRethrow()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");
            var callOrder = 0;
            var errorSaverOrder = 0;

			Task errorSaver(PolicyResult _, Exception __, ErrorContext<string> ___, bool ____, CancellationToken _____)
			{
				errorSaverOrder = ++callOrder;
				return Task.CompletedTask;
			}

			await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                errorSaver,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(errorSaverOrder, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_CallErrorSaverAfterDeterminingResult_WhenBehaviorIsConditionalRethrowAndExceptionAccepted()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");
            var errorSaverCallCount = 0;

			Task errorSaver(PolicyResult _, Exception __, ErrorContext<string> ___, bool ____, CancellationToken _____)
			{
				errorSaverCallCount++;
				return Task.CompletedTask;
			}

			await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                errorSaver,
                null,
                ExceptionHandlingBehavior.ConditionalRethrow,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(errorSaverCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_UseConfigureAwaitParameter_WhenCallingAsyncMethods()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

            var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                true,
                CancellationToken.None);

            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
        }

        [Test]
        public async Task Should_PassCancellationTokenToBulkProcessor()
		{
			using (var cts = new CancellationTokenSource())
			{
				var bulkProcessor = new TestBulkErrorProcessor();
				var processor = new TestPolicyProcessor(bulkProcessor);
				var policyResult = new PolicyResult(true);
				var exception = new Exception("Test exception");
				var errorContext = new TestErrorContext("test");

				await processor.TestHandleExceptionAsync(
					exception,
					policyResult,
					errorContext,
					null,
					null,
					ExceptionHandlingBehavior.Handle,
					ErrorProcessingCancellationEffect.Ignore,
					false,
					cts.Token);

				Assert.That(policyResult.IsSuccess, Is.True);
			}
		}

		[Test]
        public async Task Should_SetFailedAndCanceled_WhenTokenCanceledAfterErrorSaverInConditionalRethrow()
		{
			var processor = new TestPolicyProcessor(null);
			var policyResult = new PolicyResult(true);
			var exception = new Exception("Test exception");
			var errorContext = new TestErrorContext("test");
			using (var cts = new CancellationTokenSource())
			{
				Task errorSaver(PolicyResult _, Exception __, ErrorContext<string> ___, bool ____, CancellationToken _____)
				{
					cts.Cancel();
					return Task.CompletedTask;
				}

				await processor.TestHandleExceptionAsync(
					exception,
					policyResult,
					errorContext,
					errorSaver,
					null,
					ExceptionHandlingBehavior.ConditionalRethrow,
					ErrorProcessingCancellationEffect.Ignore,
					false,
					cts.Token);
			}

			Assert.That(policyResult.IsFailed, Is.True);
			Assert.That(policyResult.IsCanceled, Is.True);
		}

		[Test]
        public async Task Should_HandleExceptionAsync_ReturnHandled_WhenPolicyRuleFuncReturnsTrue()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");

			bool policyRuleFunc(ErrorContext<string> _) => true;

			var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                policyRuleFunc,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false,
                CancellationToken.None);

            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
        }
    }
}
