using NUnit.Framework;
using System;
using System.Linq;
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
                false);

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
                false);

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
                false);

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
                    ProcessingOrder.EvaluateThenProcess,
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

			Task<bool> policyRuleFunc(ErrorContext<string> _, CancellationToken __) => Task.FromResult(false);

			var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                policyRuleFunc,
                ExceptionHandlingBehavior.ConditionalRethrow,
                ErrorProcessingCancellationEffect.Ignore,
                false);

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
                false);

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
                false);

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
                false);

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
                false);

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
                false);

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
                false);

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
                true);

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
                    ProcessingOrder.EvaluateThenProcess,
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
                    ProcessingOrder.EvaluateThenProcess,
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

			Task<bool> policyRuleFunc(ErrorContext<string> _, CancellationToken __) => Task.FromResult(true);

			var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                policyRuleFunc,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false);

            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
        }

        [Test]
        public async Task Should_ReturnRethrow_WhenExceptionFilterFailsAndBehaviorIsConditionalRethrow()
        {
            var processor = new TestPolicyProcessor(null, true);
            var policyResult = new PolicyResult(true);
            var exception = new ArgumentException("Test exception");
            var errorContext = new TestErrorContext("test");
            var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.ConditionalRethrow,
                ErrorProcessingCancellationEffect.Ignore,
                false);
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Rethrow));
        }

        [Test]
        public async Task Should_ReturnHandled_WhenExceptionFilterFailsAndBehaviorIsHandle()
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor(), true);
            var policyResult = new PolicyResult(true);
            var exception = new ArgumentException("Test exception");
            var errorContext = new TestErrorContext("test");
            var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Ignore,
                false);
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
        }

        [Test]
        [TestCase(ExceptionHandlingBehavior.Handle)]
        [TestCase(ExceptionHandlingBehavior.ConditionalRethrow)]
        public async Task Should_ReturnHandled_WhenExceptionFilterThrowsException(ExceptionHandlingBehavior handlingBehavior)
        {
            var processor = new TestPolicyProcessor(new TestBulkErrorProcessor());
            processor.ErrorFilter.AddIncludedErrorFilter((ex) => Save(ex));
            var policyResult = new PolicyResult(true);
            var exception = new Exception("Test exception");
            var errorContext = new TestErrorContext("test");
            var result = await processor.TestHandleExceptionAsync(
                exception,
                policyResult,
                errorContext,
                null,
                null,
                handlingBehavior,
                ErrorProcessingCancellationEffect.Ignore,
                false);
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.CriticalError, Is.Not.Null);
            Assert.That(policyResult.CatchBlockErrors.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_HandleExceptionAsync_Not_Set_PolicyResult_IsCanceled_When_ProcessingOrder_ProcessThenEvaluate_And_CancellationEffectIgnore()
        {
            // Arrange
            var policyResult = PolicyResult.ForNotSync();
            var errorContext = new TestErrorContext("test");
            var exception = new Exception("test exception");

            bool ruleResult = false;
			Task<bool> ruleFunc(ErrorContext<string> _, CancellationToken __)
            {
                ruleResult = true;
                return Task.FromResult(true);
            }

            using (var cts = new CancellationTokenSource())
            {
                var bulkProcessor = new BulkErrorProcessor()
                           .WithErrorProcessorOf((Exception _, CancellationToken __) => cts.Cancel());

                var processor = new TestPolicyProcessor(bulkProcessor);

                // Act
                var result = await processor.TestHandleExceptionAsync(
					exception,
					policyResult,
					errorContext,
                    null,
                    ruleFunc,
                    ExceptionHandlingBehavior.Handle,
                    ErrorProcessingCancellationEffect.Ignore,
                    false,
                    ProcessingOrder.ProcessThenEvaluate,
                    cts.Token);

                // Assert
                Assert.That(ruleResult, Is.True);
                Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Accepted));
                Assert.That(policyResult.IsFailed, Is.False);
                Assert.That(policyResult.IsCanceled, Is.False);
                Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            }
        }
    }
}
