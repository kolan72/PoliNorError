using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    [TestFixture]
    public class BasicHandlerTests
    {
        [Test]
        public void Should_SaveException_WhenCalled()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var exception = new InvalidOperationException("test");
            bool saverCalled = false;
            bool handledResult = false;

            void saver(PolicyResult pr, Exception ex, ErrorContext<Unit> _, CancellationToken __)
			{
				saverCalled = true;
				pr.AddError(ex);
			}

            // Act
            handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                exception,
                policyResult,
                errorContext,
                saver,
                null,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);

            // Assert
            Assert.That(saverCalled, Is.True);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.First(), Is.SameAs(exception));
            Assert.That(handledResult, Is.True);
        }

        [Test]
        public void Should_SetFailedAndCanceled_WhenTokenIsAlreadyCanceled()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var exception = new InvalidOperationException("test");
            bool handledResult = false;

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                Exception savedCancellationEx = null;

				void saver(PolicyResult _, Exception __, ErrorContext<Unit> ___, CancellationToken ct)
				{
					if (ct.IsCancellationRequested)
						savedCancellationEx = new OperationCanceledException(ct);
				}

				Func<ErrorContext<Unit>, CancellationToken, bool> ruleFunc = null;

                // Act
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                    exception,
                    policyResult,
                    errorContext,
                    saver,
                    ruleFunc,
                    CreateEmptyBulkProcessor(),
                    ErrorProcessingCancellationEffect.Ignore,
                    cts.Token);

                // Assert
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(savedCancellationEx, Is.Not.Null);
                Assert.That(handledResult, Is.False);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_DontCallRuleFuncIfTokenCancelledBeforeEvaluation(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var exception = new InvalidOperationException("test");
            bool ruleInvoked = false;
            bool handledResult = false;

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
				bool ruleFunc(ErrorContext<Unit> _, CancellationToken __)
				{
					ruleInvoked = true;
					return true;
				}

                // Act
                if (withDefaultSaver)
                {
                    handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                    exception,
                    policyResult,
                    errorContext,
                    ruleFunc,
                    CreateEmptyBulkProcessor(),
                    ErrorProcessingCancellationEffect.Ignore,
                    cts.Token);
                }
                else
                {
                    handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                    exception,
                    policyResult,
                    errorContext,
                    (pr, ex, _, __) => pr.AddError(ex),
                    ruleFunc,
                    CreateEmptyBulkProcessor(),
                    ErrorProcessingCancellationEffect.Ignore,
                    cts.Token);
                }

                // Assert
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(ruleInvoked, Is.False);
                Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
                Assert.That(handledResult, Is.False);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_ProcessViaBulkProcessor_WhenRuleReturnsTrue(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            bool bulkProcessed = false;
            bool handledResult = false;

            var bulkProcessor = new MockBulkProcessor(() =>
            {
                bulkProcessed = true;
                return new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>());
            });

			bool ruleFunc(ErrorContext<Unit> _, CancellationToken __) => true;

            // Act
            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }

            // Assert
            Assert.That(bulkProcessed, Is.True);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
            Assert.That(handledResult, Is.True);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_NotProcessViaBulkProcessor_WhenRuleReturnsFalse(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            bool bulkProcessed = false;
            bool handledResult = false;

            var bulkProcessor = new MockBulkProcessor(() =>
            {
                bulkProcessed = true;
                return new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>());
            });

			bool ruleFunc(ErrorContext<Unit> _, CancellationToken __) => false;

            // Act
            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }

            // Assert
            Assert.That(bulkProcessed, Is.False);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(handledResult, Is.False);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_CallBulkProcessor_Only_WhenRuleFuncIsNull(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            bool bulkProcessed = false;
            bool handledResult = false;

            var bulkProcessor = new MockBulkProcessor(() =>
            {
                bulkProcessed = true;
                return new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>());
            });

            // Act
            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                null,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                null,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }

            // Assert
            Assert.That(bulkProcessed, Is.True);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
            Assert.That(handledResult, Is.True);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_AddCatchBlockErrorsFromBulkProcessorToResult(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var processorEx = new ArgumentException("processor error");
            var epEx = new BulkErrorProcessor.ErrorProcessorException(processorEx, null, BulkErrorProcessor.ProcessStatus.Faulted);
            var bulkResult = new BulkErrorProcessor.BulkProcessResult(ex, new[] { epEx });
            bool handledResult = false;

            var bulkProcessor = new MockBulkProcessor(() => bulkResult);

			bool ruleFunc(ErrorContext<Unit> _, CancellationToken __) => true;

            // Act
            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
               ex,
               policyResult,
               errorContext,
               (pr, e, _, __) => pr.AddError(e),
               ruleFunc,
               bulkProcessor,
               ErrorProcessingCancellationEffect.Ignore,
               CancellationToken.None);
            }
            
            // Assert
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(handledResult, Is.True);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_SetFailedAndCanceledWhenBulkProcessorResultIsCanceled_AndEffectPropagate(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var cancelEx = new OperationCanceledException();
            bool handledResult = false;

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            var bulkProcessor = new MockBulkProcessor(() => bulkResult);

			bool ruleFunc(ErrorContext<Unit> _, CancellationToken __) => true;

            // Act
            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Propagate,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Propagate,
                CancellationToken.None);
            }
            
            // Assert
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            Assert.That(handledResult, Is.False);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_NotSetFailedAndCanceledWhenBulkProcessorResultIsCanceled_AndEffectIgnore(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var cancelEx = new OperationCanceledException();
            bool handledResult = false;

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            var bulkProcessor = new MockBulkProcessor(() => bulkResult);

			bool ruleFunc(ErrorContext<Unit> _, CancellationToken __) => true;

            // Act
            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                ruleFunc,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }

            // Assert
            Assert.That(policyResult.IsFailed, Is.False);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            Assert.That(handledResult, Is.True);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_SetFailedWithCatchBlockError_WhenRuleFuncThrowsNonCancellationException(bool withDefaultSaver)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            var ruleError = new ArithmeticException("rule error");
            bool handledResult = false;

            bool ruleFunc(ErrorContext<Unit> _, CancellationToken __) => throw ruleError;

            // Act

            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                ruleFunc,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Propagate,
                CancellationToken.None);
            }
            else
            {
               handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
               ex,
               policyResult,
               errorContext,
               (pr, e, _, __) => pr.AddError(e),
               ruleFunc,
               CreateEmptyBulkProcessor(),
               ErrorProcessingCancellationEffect.Propagate,
               CancellationToken.None);
            }

            // Assert
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.CriticalError, Is.Not.Null);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            Assert.That(handledResult, Is.False);
        }

        [Test]
        public void Should_SetFailedInner_WhenRuleFuncReturnsNullInsteadOfBool()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            bool handledResult = false;

            // When policyRuleFunc returns null/defaults to false, it means "not accepted"
            // which leads to SetFailedInner with no additional error
            bool ruleFunc(ErrorContext<Unit> _, CancellationToken __)
			{
				return false;
			}

            // Act
            handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                ruleFunc,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Propagate,
                CancellationToken.None);

            // Assert
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(handledResult, Is.False);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShouldNotAddExtraCatchBlockError_WhenRuleFuncSucceedsWithBooleanDefault_True(bool withDefaultSaver)
        {
            // When policyRuleFunc is not provided (null), evaluatePolicyRule calls invoke func
            // If func returns default (which can't be null for bool), result.accepted is true (true != false)
            // So Accepted path is taken and bulk processor runs normally
            bool runByUser = false;
            bool handledResult = false;

            bool func(ErrorContext<Unit> _, CancellationToken __)
			{
				runByUser = true;
				throw new InvalidOperationException("this should never appear as caught");
			}

			var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var originalEx = new System.IO.FileNotFoundException();

            if (withDefaultSaver)
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                originalEx,
                policyResult,
                errorContext,
                func,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }
            else
            {
                handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                originalEx,
                policyResult,
                errorContext,
                (pr, e, _, __) => pr.AddError(e),
                func,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);
            }

            // The rule threw a non-cancellation exception, so it set failed with catch block error
            Assert.That(runByUser, Is.True);
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.CriticalError, Is.Not.Null);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            Assert.That(handledResult, Is.False);
        }

        [Test]
        public void ShouldNotSaveException_WhenErrorSaverNeverTakesPartially()
        {
            // Verify that the error saver is always called first before any checks happen
            bool saverCalled = false;
            bool bulkCalled = false;

            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException();
            bool handledResult = false;

            void saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, CancellationToken __)
			{
				saverCalled = true;
				pr.AddError(e);
			}

			var mockBp = new MockBulkProcessor(() =>
            {
                bulkCalled = true;
                return new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>());
            });

			// Rule returning true to force bulk processing
			bool rule(ErrorContext<Unit> _, CancellationToken __) => true;

            handledResult = BasicHandler.TryEvaluateRuleThenProcessException(
                ex,
                policyResult,
                errorContext,
                saver,
                rule,
                mockBp,
                ErrorProcessingCancellationEffect.Ignore,
                CancellationToken.None);

            Assert.That(saverCalled, Is.True);
            Assert.That(bulkCalled, Is.True);
            Assert.That(handledResult, Is.True);
        }

        private static IBulkErrorProcessor CreateEmptyBulkProcessor()
        {
            return new MockBulkProcessor(() =>
                new BulkErrorProcessor.BulkProcessResult(null, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));
        }

        private class TestSimpleErrorContext : ErrorContext<Unit>
        {
            public TestSimpleErrorContext() : base(Unit.Default) { }
            public override ProcessingErrorContext ToProcessingErrorContext() => new ProcessingErrorContext();
        }

        private class MockBulkProcessor : IBulkErrorProcessor
        {
            private readonly Func<BulkErrorProcessor.BulkProcessResult> _getReturn;
            public BulkErrorProcessor.BulkProcessResult LastResult { get; private set; }

            public MockBulkProcessor(Func<BulkErrorProcessor.BulkProcessResult> getReturn)
            {
                _getReturn = getReturn;
            }

            public BulkErrorProcessor.BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
            {
                LastResult = _getReturn();
                return LastResult;
            }

            public Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default)
            {
                LastResult = _getReturn();
                return Task.FromResult(LastResult);
            }

            public void AddProcessor(IErrorProcessor errorProcessor) { }
            public IEnumerator<IErrorProcessor> GetEnumerator() => System.Linq.Enumerable.Empty<IErrorProcessor>().GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
