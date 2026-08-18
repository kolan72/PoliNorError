using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    [TestFixture]
    public class BasicHandlerTryProcessExceptionThenEvaluateRuleAsyncTests
    {
        [Test]
        public async Task Should_SaveErrorAndProcessViaBulk_WhenAllSucceeds()
        {
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

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                bp,
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(saverCalled, Is.True);
            Assert.That(ruleCalled, Is.True);
            Assert.That(bulkCalled, Is.True);
            Assert.That(policyResult.Errors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.Errors.First(), Is.SameAs(ex));
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_SetFailedAndCanceled_WhenTokenAlreadyCanceled(bool configureAwait)
        {
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

                Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
                {
                    ruleCalled = true;
                    return Task.FromResult(false);
                }

                var bulkTracker = new AsyncTrackingBulkProcessor(
                    () => bulkCalled = true,
                    () => new BulkErrorProcessor.BulkProcessResult(null, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

                bool ret = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    Rule,
                    bulkTracker,
                    ErrorProcessingCancellationEffect.Propagate,
                    configureAwait,
                    cts.Token);

                Assert.That(ret, Is.False);
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.PolicyCanceledError, Is.InstanceOf<OperationCanceledException>());
                Assert.That(ruleCalled, Is.False);
                Assert.That(bulkCalled, Is.False);
            }
        }

        [Test]
        public async Task Should_CallBulkProcessorBeforeRule_WhenBothRun()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
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

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
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

            Assert.That(result, Is.True);
            Assert.That(order.Count, Is.EqualTo(3));
            Assert.That(order[0], Is.EqualTo("Saver"));
            Assert.That(order[1], Is.EqualTo("Bulk"));
            Assert.That(order[2], Is.EqualTo("Rule"));
        }

        [Test]
        public async Task Should_StillRunBulkProcessor_WhenRuleReturnsFalse()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            bool bulkCalled = false;
            bool ruleCalled = false;

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
            {
                ruleCalled = true;
                return Task.FromResult(false);
            }

            var bulkProcessor = new AsyncTrackingBulkProcessor(
                () => bulkCalled = true,
                () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.False);
            Assert.That(bulkCalled, Is.True);
            Assert.That(ruleCalled, Is.True);
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
        }

        [Test]
        public async Task Should_ReturnTrue_WhenPolicyRuleFuncIsNull()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            bool bulkCalled = false;

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            var bulkProcessor = new AsyncTrackingBulkProcessor(
                () => bulkCalled = true,
                () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                null,
                bulkProcessor,
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.True);
            Assert.That(bulkCalled, Is.True);
            Assert.That(policyResult.IsFailed, Is.False);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.Errors.Single(), Is.SameAs(ex));
        }

        [Test]
        public async Task Should_SetFailedAndCanceled_WhenBulkCanceledAndEffectPropagate()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            var cancelEx = new OperationCanceledException();
            bool ruleCalled = false;

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
            {
                ruleCalled = true;
                return Task.FromResult(true);
            }

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                new AsyncTrackingBulkProcessor(() => { }, () => bulkResult),
                ErrorProcessingCancellationEffect.Propagate,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.False);
            Assert.That(ruleCalled, Is.False);
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.True);
            Assert.That(policyResult.PolicyCanceledError, Is.InstanceOf<OperationCanceledException>());
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_ContinueToRule_WhenBulkCanceledAndEffectIgnore()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            var cancelEx = new OperationCanceledException();
            bool ruleCalled = false;

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
            {
                ruleCalled = true;
                return Task.FromResult(true);
            }

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                new AsyncTrackingBulkProcessor(() => { }, () => bulkResult),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.True);
            Assert.That(ruleCalled, Is.True);
            Assert.That(policyResult.IsFailed, Is.False);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_SetFailed_WhenBulkCanceledEffectIgnoreAndRuleReturnsFalse()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            var cancelEx = new OperationCanceledException();
            bool ruleCalled = false;

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(cancelEx, null, BulkErrorProcessor.ProcessStatus.Canceled) },
                isCanceledBetweenProcessors: false);

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
            {
                ruleCalled = true;
                return Task.FromResult(false);
            }

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                new AsyncTrackingBulkProcessor(() => { }, () => bulkResult),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.False);
            Assert.That(ruleCalled, Is.True);
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
        }

        [Test]
        public async Task Should_AddBulkProcessorErrorsToCatchBlockErrors()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            var processorEx = new ArgumentException("processor fault");

            var bulkResult = new BulkErrorProcessor.BulkProcessResult(
                ex,
                new[] { new BulkErrorProcessor.ErrorProcessorException(processorEx, null, BulkErrorProcessor.ProcessStatus.Faulted) });

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                new AsyncTrackingBulkProcessor(() => { }, () => bulkResult),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.True);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.CatchBlockErrors.Single().ExceptionSource, Is.EqualTo(CatchBlockExceptionSource.ErrorProcessor));
        }

        [Test]
        public async Task Should_SetFailedWithCatchBlockError_WhenRuleThrowsNonCancellation()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            Task<bool> ThrowingRule(ErrorContext<Unit> _, CancellationToken __)
            {
                throw new ArithmeticException("rule failed");
            }

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                ThrowingRule,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Propagate,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.False);
            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.CriticalError, Is.InstanceOf<ArithmeticException>());
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_SetFailedAndCanceled_WhenRuleThrowsOperationCanceledException()
        {
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

                async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
                {
                    pr.AddError(e);
                    await Task.Yield();
                }

                bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    CancelRule,
                    CreateEmptyBulkProcessor(),
                    ErrorProcessingCancellationEffect.Propagate,
                    configureAwait: true,
                    cts.Token);

                Assert.That(result, Is.False);
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.PolicyCanceledError, Is.InstanceOf<OperationCanceledException>());
                Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Should_SetFailedAndCanceled_WhenRuleThrowsAggregateCancellation()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            using (var cts = new CancellationTokenSource())
            {
                async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
                {
                    pr.AddError(e);
                    await Task.Yield();
                }

                Task<bool> AggExceptionRule(ErrorContext<Unit> _, CancellationToken token)
                {
                    cts.Cancel();
                    throw new AggregateException(new OperationCanceledException(token));
                }

                bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    AggExceptionRule,
                    CreateEmptyBulkProcessor(),
                    ErrorProcessingCancellationEffect.Propagate,
                    configureAwait: true,
                    cts.Token);

                Assert.That(result, Is.False);
                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Should_PassConfigureAwaitValue_Through_All_Delegates()
        {
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

            await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                bp,
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: false,
                CancellationToken.None);

            Assert.That(savedConfigAwait, Is.False);
            Assert.That(bp.LastConfigAwait, Is.False);
        }

        [Test]
        public async Task Should_PassExceptionAndContextToSaver()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            PolicyResult savedPolicyResult = null;
            Exception savedEx = null;
            ErrorContext<Unit> savedContext = null;

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> ctx, bool __, CancellationToken ___)
            {
                savedPolicyResult = pr;
                savedEx = e;
                savedContext = ctx;
                await Task.Yield();
                pr.AddError(e);
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.True);
            Assert.That(savedPolicyResult, Is.SameAs(policyResult));
            Assert.That(savedEx, Is.SameAs(ex));
            Assert.That(savedContext, Is.SameAs(errorContext));
            Assert.That(policyResult.Errors.Single(), Is.SameAs(ex));
        }

        [Test]
        public async Task Should_PassTokenToBulkProcessorAndRule()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");
            CancellationToken ruleToken = default;

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
                {
                    pr.AddError(e);
                    await Task.Yield();
                }

                Task<bool> Rule(ErrorContext<Unit> _, CancellationToken ct)
                {
                    ruleToken = ct;
                    return Task.FromResult(true);
                }

                var bulkTracker = new AsyncTrackingBulkProcessor(
                    () => { },
                    () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));

                bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    Rule,
                    bulkTracker,
                    ErrorProcessingCancellationEffect.Ignore,
                    configureAwait: true,
                    token);

                Assert.That(result, Is.True);
                Assert.That(ruleToken, Is.EqualTo(token));
            }
        }

        [Test]
        public async Task Should_SetFailedInner_WhenRuleFuncReturnsFalse()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(false);

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Propagate,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(policyResult.IsFailed, Is.True);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Should_DontCallRuleFuncIfTokenCancelledAfterBulkProcessor()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");
            bool ruleInvoked = false;

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
                {
                    pr.AddError(e);
                    await Task.Yield();
                }

                Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __)
                {
                    ruleInvoked = true;
                    return Task.FromResult(true);
                }

                bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                    ex,
                    policyResult,
                    errorContext,
                    Saver,
                    Rule,
                    CreateEmptyBulkProcessor(),
                    ErrorProcessingCancellationEffect.Ignore,
                    configureAwait: true,
                    cts.Token);

                Assert.That(policyResult.IsFailed, Is.True);
                Assert.That(policyResult.IsCanceled, Is.True);
                Assert.That(ruleInvoked, Is.False);
                Assert.That(result, Is.False);
            }
        }

        [Test]
        public async Task Should_PassConfigureAwaitTrue_ThroughToBulkProcessor()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("test");

            async Task Saver(PolicyResult _, Exception __, ErrorContext<Unit> ___, bool ____, CancellationToken _____)
            {
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                new AsyncTrackingBulkProcessor(
                    () => { },
                    () => new BulkErrorProcessor.BulkProcessResult(ex, Array.Empty<BulkErrorProcessor.ErrorProcessorException>())),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Should_ReturnTrue_WhenBulkSucceedsAndRuleAccepts()
        {
            var policyResult = new PolicyResult();
            var errorContext = new TestSimpleErrorContext();
            var ex = new InvalidOperationException("original");

            async Task Saver(PolicyResult pr, Exception e, ErrorContext<Unit> _, bool __, CancellationToken ___)
            {
                pr.AddError(e);
                await Task.Yield();
            }

            Task<bool> Rule(ErrorContext<Unit> _, CancellationToken __) => Task.FromResult(true);

            bool result = await BasicHandler.TryProcessExceptionThenEvaluateRuleAsync(
                ex,
                policyResult,
                errorContext,
                Saver,
                Rule,
                CreateEmptyBulkProcessor(),
                ErrorProcessingCancellationEffect.Ignore,
                configureAwait: true,
                CancellationToken.None);

            Assert.That(result, Is.True);
            Assert.That(policyResult.IsFailed, Is.False);
            Assert.That(policyResult.IsCanceled, Is.False);
            Assert.That(policyResult.Errors.Single(), Is.SameAs(ex));
        }

        private static IBulkErrorProcessor CreateEmptyBulkProcessor()
        {
            return new AsyncTrackingBulkProcessor(
                () => { },
                () => new BulkErrorProcessor.BulkProcessResult(null, Array.Empty<BulkErrorProcessor.ErrorProcessorException>()));
        }

        private class TestSimpleErrorContext : ErrorContext<Unit>
        {
            public TestSimpleErrorContext() : base(Unit.Default) { }
            public override ProcessingErrorContext ToProcessingErrorContext() => new ProcessingErrorContext();
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
            public IEnumerator<IErrorProcessor> GetEnumerator() => Enumerable.Empty<IErrorProcessor>().GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
