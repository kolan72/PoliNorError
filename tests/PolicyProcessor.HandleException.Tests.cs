using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.BulkErrorProcessor;

namespace PoliNorError.Tests
{
    [TestFixture]
    public partial class PolicyProcessorTests
    {
        private class TestPolicyProcessor : PolicyProcessor
        {
            public TestPolicyProcessor(IBulkErrorProcessor bulkErrorProcessor, bool testFilterUnsatisfied = false)
            {
                _bulkErrorProcessor = bulkErrorProcessor;
                if (testFilterUnsatisfied)
                {
                    ErrorFilter.AddExcludedErrorFilter<ArgumentException>();
                }
            }

            public ExceptionHandlingResult TestHandleException<T>(
                Exception ex,
                PolicyResult policyResult,
                ErrorContext<T> errorContext,
                CancellationToken token,
                ProcessingOrder processingOrder = ProcessingOrder.EvaluateThenProcess,
                Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc = null,
                ExceptionHandlingBehavior handlingBehavior = ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect cancellationEffect = ErrorProcessingCancellationEffect.Propagate,
                Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> errorSaver = null)
            {
                return HandleException(ex, policyResult, errorContext, errorSaver, policyRuleFunc, handlingBehavior, processingOrder, cancellationEffect, token);
            }

            public Task<ExceptionHandlingResult> TestHandleExceptionAsync<T>(
                Exception ex,
                PolicyResult policyResult,
                ErrorContext<T> errorContext,
                Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> errorSaver,
                Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc,
                ExceptionHandlingBehavior handlingBehavior,
                ErrorProcessingCancellationEffect cancellationEffect,
                bool configureAwait,
                CancellationToken token)
            {
                return HandleExceptionAsync(ex, policyResult, errorContext, errorSaver, policyRuleFunc, handlingBehavior, cancellationEffect, configureAwait, token);
            }
        }

        private class TestErrorContext : ErrorContext<string>
        {
            public TestErrorContext(string context) : base(context) { }

            public override ProcessingErrorContext ToProcessingErrorContext()
            {
                return new ProcessingErrorContext();
            }
        }

        private class TestBulkErrorProcessor : IBulkErrorProcessor
        {
            public BulkProcessResult ResultToReturn { get; set; }

            public bool IsCanceled { get; set; }

            public bool IsProcessed { get; private set; }

            public void AddProcessor(IErrorProcessor errorProcessor) => throw new NotImplementedException();
            public IEnumerator<IErrorProcessor> GetEnumerator() => throw new NotImplementedException();

            public BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
            {
                IsProcessed = true;
                return ResultToReturn ?? new BulkProcessResult(handlingError, null);
            }

            public Task<BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default)
            {
                var result = new BulkProcessResult(handlingError, ProcessErrors ?? Array.Empty<ErrorProcessorException>(), IsCanceled);
                return Task.FromResult(result);
            }

            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            public ErrorProcessorException[] ProcessErrors { get; set; }
        }

        [Test]
        public void Should_ReturnHandled_AndSetFailedAndCanceled_WhenTokenIsCanceled()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor();
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new Exception("test exception");
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                // Act
                var result = processor.TestHandleException(exception, policyResult, errorContext, cts.Token);

                // Assert
                Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            }
        }

        [Test]
        [TestCase(ErrorProcessingCancellationEffect.Ignore)]
        [TestCase(ErrorProcessingCancellationEffect.Propagate)]
        public void Should_Set_PolicyResult_IsCanceled_DependOn_CancellationEffect(ErrorProcessingCancellationEffect cancellationEffect)
        {
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new Exception("test exception");
            using (var cts = new CancellationTokenSource())
            {
                var bulkProcessor = new BulkErrorProcessor()
                            .WithErrorProcessorOf((Exception _, CancellationToken __) => cts.Cancel());

                var processor = new TestPolicyProcessor(bulkProcessor);

                var result = processor.TestHandleException(exception, policyResult, errorContext, cts.Token, ProcessingOrder.EvaluateThenProcess, null, ExceptionHandlingBehavior.Handle, cancellationEffect);

                Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));

                Assert.That(policyResult.IsFailed, Is.EqualTo(cancellationEffect == ErrorProcessingCancellationEffect.Propagate));
                Assert.That(policyResult.IsCanceled, Is.EqualTo(cancellationEffect == ErrorProcessingCancellationEffect.Propagate));

                Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Should_Not_Set_PolicyResult_IsCanceled_When_ProcessingOrder_ProcessThenEvaluate_And_CancellationEffectIgnore()
        {
            // Arrange
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new Exception("test exception");
            using (var cts = new CancellationTokenSource())
            {
                var bulkProcessor = new BulkErrorProcessor()
                           .WithErrorProcessorOf((Exception _, CancellationToken __) => cts.Cancel());

                var processor = new TestPolicyProcessor(bulkProcessor);

                // Act
                var result = processor.TestHandleException(exception, policyResult, errorContext, cts.Token, ProcessingOrder.ProcessThenEvaluate, cancellationEffect: ErrorProcessingCancellationEffect.Ignore);

                // Assert
                Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Accepted));
                Assert.That(policyResult.IsFailed, Is.False);
                Assert.That(policyResult.IsCanceled, Is.False);
                Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Should_ReturnHandled_WhenNullPolicyRuleFunc()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = new BulkProcessResult(new InvalidOperationException(), null)
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");

            // Act
            var result = processor.TestHandleException(exception, policyResult, errorContext, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_ReturnRethrow_WhenErrorFilterFailsAndBehaviorIsConditionalRethrow()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor();
            var processor = new TestPolicyProcessor(bulkProcessor, true);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new ArgumentException("test exception");

            // Act
            var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                null,
                ExceptionHandlingBehavior.ConditionalRethrow);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Rethrow));
        }

        [Test]
        public void Should_ReturnHandled_WhenErrorFilterFailsAndBehaviorIsHandle()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = new BulkProcessResult(new ArgumentException("test"), null)
            };
            var processor = new TestPolicyProcessor(bulkProcessor, true);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new ArgumentException("test");

            // Act
            var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                null,
                ExceptionHandlingBehavior.Handle);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_ReturnHandled_AndSetFailed_WhenPolicyRuleFuncReturnsFalse()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor();
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");
			bool policyRuleFunc(ErrorContext<string> _, CancellationToken __) => false;

			// Act
			var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                policyRuleFunc);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_ReturnHandled_WhenPolicyRuleFuncReturnsTrue()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = new BulkProcessResult(new InvalidOperationException(), null)
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");
			bool policyRuleFunc(ErrorContext<string> _, CancellationToken __) => true;

			// Act
			var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                policyRuleFunc);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_SetFailedAndCanceled_WhenBulkProcessResultIsCanceled()
        {
            // Arrange
            var bulkProcessResult = new BulkProcessResult(new InvalidOperationException(), null, true);

            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = bulkProcessResult
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");

            // Act
            var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_AddBulkProcessorErrors_WhenBulkProcessorReturnsErrors()
        {
            // Arrange
            var processorException = new ErrorProcessorException(new InvalidCastException(), null, ProcessStatus.Faulted);
            var bulkProcessResult = new BulkProcessResult(
                new InvalidOperationException(),
                new[] { processorException });
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = bulkProcessResult
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");

            // Act
            processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None);

            // Assert
            Assert.That(policyResult.IsFailed, Is.False); // No critical errors
            Assert.That(bulkProcessor.IsProcessed, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_AddExceptionToPolicyResult_WhenHandlingException()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = new BulkProcessResult(new InvalidOperationException(), null)
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");

            // Act
            processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None);

            // Assert - exception is added internally
            Assert.That(policyResult.IsFailed, Is.False);
            Assert.That(bulkProcessor.IsProcessed, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }

		[Test]
		public void Should_UseCustomErrorSaver_WhenProvided()
		{
			// Arrange
			var bulkProcessor = new TestBulkErrorProcessor
			{
				ResultToReturn = new BulkProcessResult(new InvalidOperationException(), null)
			};
			var processor = new TestPolicyProcessor(bulkProcessor);
			var policyResult = PolicyResult.ForSync();
			var errorContext = new TestErrorContext("test");
			var exception = new InvalidOperationException("test exception");

			var customErrorSaverCalled = false;
			void customErrorSaver(PolicyResult pr, Exception _, ErrorContext<string> __, CancellationToken ___)
			{
				customErrorSaverCalled = true;
				pr.AddError(new Exception("Custom error"));
			}

			// Act
			var result = processor.TestHandleException(
				exception,
				policyResult,
				errorContext,
				CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                null,
                ExceptionHandlingBehavior.Handle,
                ErrorProcessingCancellationEffect.Propagate,
				errorSaver:customErrorSaver);

			// Assert
			Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
			Assert.That(customErrorSaverCalled, Is.True);
			Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
		}

		[Test]
        [TestCase(ExceptionHandlingBehavior.Handle)]
        [TestCase(ExceptionHandlingBehavior.ConditionalRethrow)]
        public void Should_HandleErrorFilterThrowingException_WhenHandleOrRethrowBehavior(ExceptionHandlingBehavior handlingBehavior)
		{
			// Arrange
			var bulkProcessor = new TestBulkErrorProcessor();
            var processor = new TestPolicyProcessor(bulkProcessor);

            // Make the error filter throw an exception
            processor.ErrorFilter.AddIncludedErrorFilter((ex) => Save(ex));

			var policyResult = PolicyResult.ForSync();
			var errorContext = new TestErrorContext("test");
			var exception = new InvalidOperationException("test exception");

			// Act
			var result = processor.TestHandleException(
				exception,
				policyResult,
				errorContext,
				CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                null,
                handlingBehavior
               );
			// Assert
			Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
			Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.CriticalError, Is.Not.Null);
            Assert.That(policyResult.CatchBlockErrors.Count, Is.EqualTo(1));
        }

#pragma warning disable S1172 // Unused method parameters should be removed
		private bool Save(Exception _) => throw new InvalidOperationException("Filter error");
#pragma warning restore S1172 // Unused method parameters should be removed

		[Test]
        public void Should_ReturnHandled_WhenConditionalRethrowWithValidFilterAndPolicyRule()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor
            {
                ResultToReturn = new BulkProcessResult(new InvalidOperationException(), null)
            };
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new InvalidOperationException("test exception");

            bool policyRuleFunc(ErrorContext<string> _, CancellationToken __) => true;

            // Act
            var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                ProcessingOrder.EvaluateThenProcess,
                policyRuleFunc,
                ExceptionHandlingBehavior.ConditionalRethrow);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
        }
    }
}
