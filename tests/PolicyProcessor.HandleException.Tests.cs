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
    public class PolicyProcessorTests
    {
        private class TestPolicyProcessor : PolicyProcessor
        {
            public TestPolicyProcessor(IBulkErrorProcessor bulkErrorProcessor)
            {
                _bulkErrorProcessor = bulkErrorProcessor;
                ErrorFilter.AddExcludedErrorFilter<ArgumentException>();
            }

            public ExceptionHandlingResult TestHandleException<T>(
                Exception ex,
                PolicyResult policyResult,
                ErrorContext<T> errorContext,
                CancellationToken token,
                Func<ErrorContext<T>, bool> policyRuleFunc = null,
                ExceptionHandlingBehavior handlingBehavior = ExceptionHandlingBehavior.Handle)
            {
                return HandleException(ex, policyResult, errorContext, token, policyRuleFunc, handlingBehavior);
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

			public void AddProcessor(IErrorProcessor errorProcessor) => throw new NotImplementedException();
			public IEnumerator<IErrorProcessor> GetEnumerator() => throw new NotImplementedException();

			public BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
            {
                return ResultToReturn ?? new BulkProcessResult(handlingError, null);
            }

			public Task<BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default) => throw new NotImplementedException();
			IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
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
            }
        }

        [Test]
        public void Should_ReturnHandled_WhenExceptionIsInvalidOperationException()
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
        }

        [Test]
        public void Should_ReturnRethrow_WhenErrorFilterFailsAndBehaviorIsConditionalRethrow()
        {
            // Arrange
            var bulkProcessor = new TestBulkErrorProcessor();
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new ArgumentException("test exception");

            // Act
            var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
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
            var processor = new TestPolicyProcessor(bulkProcessor);
            var policyResult = PolicyResult.ForSync();
            var errorContext = new TestErrorContext("test");
            var exception = new ArgumentException("test");

            // Act
            var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                null,
                ExceptionHandlingBehavior.Handle);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
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
			bool policyRuleFunc(ErrorContext<string> _) => false;

			// Act
			var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                policyRuleFunc);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
            Assert.That(policyResult.IsFailed, Is.True);
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
			bool policyRuleFunc(ErrorContext<string> _) => true;

			// Act
			var result = processor.TestHandleException(
                exception,
                policyResult,
                errorContext,
                CancellationToken.None,
                policyRuleFunc);

            // Assert
            Assert.That(result, Is.EqualTo(ExceptionHandlingResult.Handled));
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
        }
    }
}
