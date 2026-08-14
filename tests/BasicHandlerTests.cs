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

        private class AsyncTrackingBulkProcessor : IBulkErrorProcessor
        {
            private readonly Action _onCall;
            private readonly Func<BulkErrorProcessor.BulkProcessResult> _getResult;
            public BulkErrorProcessor.BulkProcessResult LastResult { get; private set; }
            public bool LastConfigAwait { get; private set; }

            public AsyncTrackingBulkProcessor(Action onCall, Func<BulkErrorProcessor.BulkProcessResult> getResult)
            {
                _onCall = onCall;
                _getResult = getResult;
            }

            public BulkErrorProcessor.BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default) => throw new NotImplementedException();

            public Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default)
            {
                LastConfigAwait = configAwait;
                _onCall();
                LastResult = _getResult();
                return Task.FromResult(LastResult);
            }

            public void AddProcessor(IErrorProcessor errorProcessor) { }
            public IEnumerator<IErrorProcessor> GetEnumerator() => System.Linq.Enumerable.Empty<IErrorProcessor>().GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Test]
        public async Task Should_SaveError_AndProcessViaBulk_WhenAllSucceeds()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            bool saverCalled = false;
            bool ruleCalled = false;
            bool bulkCalled = false;

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                saverCalled = true;
                await Task.Yield();
                pr.AddError(e);
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
            {
                ruleCalled = true;
                return Task.FromResult(true);
            }

            var bp = new AsyncTrackingBulkProcessor(
                () => bulkCalled = true,
                () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

            // Act
            bool result = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                bp,
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(saverCalled, Is.True);
            Assert.That(ruleCalled, Is.True);
            Assert.That(bulkCalled, Is.True);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.First(), Is.SameAs(ex));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Should_PassConfigureAwaitValue_Through_All_Delegates()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            bool savedConfigAwait = true;

            async Task Saver(PolicyResult _, Exception __, ErrorContext<Unit> ___, bool conf, CancellationToken ____)
            {
                savedConfigAwait = conf;
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            var bp = new AsyncTrackingBulkProcessor(
                () => { },
                () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

            // Act
            await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                bp,
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: false,
                CancellationToken.None);

            // Assert
            Assert.That(savedConfigAwait, Is.False);
            Assert.That(bp.LastConfigAwait, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_SetFailedAndCanceled_When_TokenIsAlreadyCanceled(bool configureAwait)
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException();
            bool ruleCalled = false;
            bool bulkCalled = false;

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                async Task Saver(PolicyResult _, Exception __, ErrorContext<Unit> ___, bool ____, CancellationToken _____)
                {
                    await Task.Yield();
                }

                Task<bool> Rule(ErrorContext<Unit> _, CancellationToken ct)
                {
                    if (ct.IsCancellationRequested)
                        ruleCalled = true;
                    return Task.FromResult(false);
                }

                var bulkTracker = new AsyncTrackingBulkProcessor(
                    () => bulkCalled = true,
                    () => new BulkErrorProcessor.BulkProcessResult(null, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

                // Act
                bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    Rule,
                    bulkTracker,
                    ErrorProcessingCancellationEffect.Propagate,
                    configureAwait,
                    cts.Token);

                // Assert
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.PolicyCanceledError, Is.InstanceOf<OperationCanceledException>());
                Assert.That(ruleCalled, Is.False);
                Assert.That(bulkCalled, Is.False);
                Assert.That(ret, Is.False);
            }
        }

        [Test]
        public async Task Should_CallBulkProcessor_Never_When_RuleReturnsFalse()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            bool bulkCalled = false;

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(false);

            // Act
            bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                DummySaver,
                Rule,
                new AsyncTrackingBulkProcessor(
                    () => bulkCalled = true,
                    () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(bulkCalled, Is.False);
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(ret, Is.False);
        }

        [Test]
        public async Task Should_CallBulkProcessor_When_RuleFunc_IsNull()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            bool bulkCalled = false;

            // Act
            bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                DummySaver,
                null,
                new AsyncTrackingBulkProcessor(
                    () => bulkCalled = true,
                    () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(bulkCalled, Is.True);
            Assert.That(ret, Is.True);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
        }

		private static async Task DummySaver(PolicyResult pr, Exception exc, ErrorContext<Unit> _, bool conf, CancellationToken t)
		{
			pr.AddError(exc);
			await Task.Yield();
		}

		[Test]
        public async Task ShouldSetFailed_AndCatchBlockError_WhenRuleThrowsNonCancelException()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            Task<bool> ThrowingRule(ErrorContext<Unit> _, CancellationToken __)
            {
                throw new ArithmeticException("rule break");
            }

            // Act
            bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                DummySaver,
                ThrowingRule,
                new AsyncTrackingBulkProcessor(
                    () => { },
                    () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                ErrorProcessingCancellationEffect.Propagate,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.CriticalError, Is.Not.Null);
            Assert.That(policyResult.CriticalError, Is.InstanceOf<ArithmeticException>());
            Assert.That(policyResult.CatchBlockErrors.Any(), Is.True);
            Assert.That(ret, Is.False);
        }

        [Test]
        public async Task ShouldSetFailedAndCanceled_WhenRuleThrowsOperationCanceledWithToken()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            using (var cts = new CancellationTokenSource())
            {
                Task<bool> CancelRule(ErrorContext<Unit> _, CancellationToken token)
                {
                    cts.Cancel();
                    throw new OperationCanceledException(token);
                }

                // Act
                bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                    ex,
                    policyResult,
                    errorContext,
                    DummySaver,
                    CancelRule,
                    new AsyncTrackingBulkProcessor(
                        () => { },
                        () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                    ErrorProcessingCancellationEffect.Propagate,
                    configureAwait: true,
                    cts.Token);

                // Assert
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.CatchBlockErrors.Any(), Is.True);
                Assert.That(ret, Is.False);
            }
        }

        [Test]
        public async Task ShouldSetFailedAndCanceled_WhenRuleThrowsAggregateException_ContainingCancellation()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            using (var cts = new CancellationTokenSource())
            {
                async Task Saver(PolicyResult _, Exception __, ErrorContext<Unit> ___, bool ____, CancellationToken _____)
                {
                    await Task.Yield();
                }

                Task<bool> AggExceptionRule(ErrorContext<Unit> _, CancellationToken token)
                {
                    cts.Cancel();
                    throw new AggregateException(new OperationCanceledException(token));
                }

                // Act
                bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    AggExceptionRule,
                    new AsyncTrackingBulkProcessor(
                        () => { },
                        () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                    ErrorProcessingCancellationEffect.Propagate,
                    configureAwait: true,
                    cts.Token);

                // Assert
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
                Assert.That(ret, Is.False);
            }
        }

        [Test]
        public async Task ShouldPropagate_CancelFromBulkProcessor_When_CancellationEffectPropagate()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var cancelEx = new OperationCanceledException();

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            // Act
            bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                DummySaver,
                Rule,
                new AsyncTrackingBulkProcessor(
                    () => { },
                    () => bulkResult),
                ErrorProcessingCancellationEffect.Propagate,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.True);
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(ret, Is.False);
        }

        [Test]
        public async Task ShouldNotPropagateCancel_When_BulkCanceledAndCancellationEffectIgnore()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var cancelEx = new OperationCanceledException();

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            // Act
            bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                DummySaver,
                Rule,
                new AsyncTrackingBulkProcessor(
                    () => { },
                    () => bulkResult),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(policyResult.IsFailed, Is.False);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.Count, Is.EqualTo(1));
            Assert.That(ret, Is.True);
        }

        [Test]
        public async Task ShouldAddBulkProcessorErrorstoResult_Always_WhenBulkRuns()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var processorEx = new ArgumentException("processor fault");

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            // Act
            bool ret = await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                DummySaver,
                Rule,
                new AsyncTrackingBulkProcessor(
                    () => { },
                    () => new BulkErrorProcessor.BulkProcessResult(
                        ex,
                        new[] { new BulkErrorProcessor.ErrorProcessorException(processorEx, null, BulkErrorProcessor.ProcessStatus.Faulted) })),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(ret, Is.True);
        }

        [Test]
        public async Task ShouldOrderExecution_Right_SyncLike_Sequence()
        {
            // Arrange
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            var order = new List<string>();

            async Task Saver(PolicyResult p, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                order.Add("Saver");
                p.AddError(e);
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
            {
                order.Add("Rule");
                return Task.FromResult(true);
            }

            // Act
            await BasicHandler.TryEvaluateRuleThenProcessExceptionAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                new AsyncTrackingBulkProcessor(
                    () => order.Add("Bulk"),
                    () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            // Assert
            Assert.That(order[0], Is.EqualTo("Saver"));
            Assert.That(order[1], Is.EqualTo("Rule"));
            Assert.That(order[2], Is.EqualTo("Bulk"));
        }
    }
}
