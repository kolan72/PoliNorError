using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.CancellationTests;

namespace PoliNorError.Tests
{
	internal class FallbackFuncExtensionsTests
    {
        [Test]
        public void Should_HandleAsFallback_ForAction_ConvertedFromAsync_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            var action = (Func<CancellationToken, Task>)(async (ct) => await Task.Delay(1, ct));
            var funcRes = action.ToSyncFunc().HandleAsFallback(cancelTokenSource.Token);
            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public void Should_HandleAsFallback_ForAction_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            Action<CancellationToken> act = (ctx) => ctx.ThrowIfCancellationRequested();
            var funcRes = act.HandleAsFallback(cancelTokenSource.Token);
            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public void Should_HandleAsFallback_ForAction_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body_And_Token_Is_Other()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            Action<CancellationToken> act = (_) => cancelTokenSource.Token.ThrowIfCancellationRequested();
            var funcRes = act.HandleAsFallback(default);
            ClassicAssert.IsFalse(funcRes.IsCanceled);
            ClassicAssert.IsNotNull(funcRes.Error);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public void Should_HandleAsFallback_ForAction_ForSuccess_Work()
        {
            Action<CancellationToken> act = (_) => { };
            var funcRes = act.HandleAsFallback(default);
            ClassicAssert.IsTrue(funcRes.IsSuccess);
        }

        [Test]
        public void Should_HandleAsFallback_ForFunc_ConvertedFromAsync_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            Func<CancellationToken, Task<int>> func = async (ct) => { await Task.Delay(1, ct); return 1; };
            var funcRes = func.ToSyncFunc().HandleAsFallback(cancelTokenSource.Token);

            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public void Should_HandleAsFallback_ForFunc_ConvertedFromAsync_If_Cancel_And_CancelToken_Is_Not_Native_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();

            Func<CancellationToken, Task<int>> func = async (ct) => { var cancelTokenS1 = new CancellationTokenSource(); cancelTokenS1.Cancel(); cancelTokenS1.Token.ThrowIfCancellationRequested(); await Task.Delay(1, ct); return 1; };
            var funcRes = func.ToSyncFunc().HandleAsFallback(cancelTokenSource.Token);

            ClassicAssert.IsFalse(funcRes.IsCanceled);
            ClassicAssert.IsNotNull(funcRes.Error);
            ClassicAssert.IsFalse(funcRes.IsSuccess);

            cancelTokenSource.Dispose();
        }

        [Test]
        public void Should_HandleAsFallback_ForFunc_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            Func<CancellationToken, int> func = (ctx) => { ctx.ThrowIfCancellationRequested(); return 1; };
            var funcRes = func.HandleAsFallback(cancelTokenSource.Token);
            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public void Should_HandleAsFallback_ForFunc_ForSuccess_Work()
        {
            Func<CancellationToken, int> func = (_) => 1;
            var funcRes = func.HandleAsFallback(default);
            ClassicAssert.IsTrue(funcRes.IsSuccess);
            ClassicAssert.AreEqual(1, funcRes.Result);
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForNoGenericFunc_ConvertedFromSync_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            var action = (Action<CancellationToken>)((_) => Expression.Empty());
            var func = action.ToAsyncFunc();

            var funcRes = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);

            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForNoGenericFunc_ConvertedFromSync_If_Cancel_And_CancelToken_Is_Not_Native_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();

            var action = (Action<CancellationToken>)((_) => { var cancelTokenS1 = new CancellationTokenSource(); cancelTokenS1.Cancel(); cancelTokenS1.Token.ThrowIfCancellationRequested(); });
            var func = action.ToAsyncFunc();

            var funcRes = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);

            ClassicAssert.IsFalse(funcRes.IsCanceled);
            ClassicAssert.IsNotNull(funcRes.Error);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForNoGenericFunc_If_Cancel_And_CancelToken_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            var func = (Func<CancellationToken, Task>)(async(ct) => { await Task.Delay(1); ct.ThrowIfCancellationRequested();});

            var funcRes = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);

            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForNoGenericFunc_ForSuccess_Work()
        {
            Func<CancellationToken, Task> func = async (_) => await Task.Delay(1);
            var funcRes = await func.HandleAsFallbackAsync(false, default);
            ClassicAssert.IsTrue(funcRes.IsSuccess);
        }

        [Test]
        public async Task Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_HandleAsFallbackAsync_Throws_DueTo_InnerToken()
        {
            using (var cancelTokenSource = new CancellationTokenSource())
            {
                Func<CancellationToken, Task> func = async (ct) => await CancelableActions.ActionThatCanceledOnOuterAndThrowOnInner(ct, cancelTokenSource);
                var fallbackResult = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);
                Assert.That(fallbackResult.IsCanceled, Is.True);
            }
        }

        [Test]
        public async Task Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_GenericHandleAsFallbackAsync_Throws_DueTo_InnerToken()
        {
            using (var cancelTokenSource = new CancellationTokenSource())
            {
                Func<CancellationToken, Task<int>> func = async (ct) => await CancelableActions.GenericActionThatCanceledOnOuterAndThrowOnInner(ct, cancelTokenSource);
                var fallbackResult = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);
                Assert.That(fallbackResult.IsCanceled, Is.True);
            }
        }

        [Test]
        [TestCase(TestCancellationMode.OperationCanceled)]
        [TestCase(TestCancellationMode.Aggregate)]
        public void Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_HandleAsFallback_Throws_DueTo_InnerToken(TestCancellationMode cancellationMode)
        {
            using (var cancelTokenSource = new CancellationTokenSource())
            {
                Action<CancellationToken> action = null;
                if (cancellationMode == TestCancellationMode.OperationCanceled)
                {
                    action = (ct) => CancelableActions.SyncActionThatCanceledOnOuterAndThrowOnInner(ct, cancelTokenSource);
                }
                else
                {
                    action = (ct) => CancelableActions.SyncActionThatCanceledOnOuterAndThrowOnInnerAndThrowAgregateExc(ct, cancelTokenSource);
                }
                var fallbackResult = action.HandleAsFallback(cancelTokenSource.Token);
                Assert.That(fallbackResult.IsCanceled, Is.True);
            }
        }

        [Test]
        [TestCase(TestCancellationMode.OperationCanceled)]
        [TestCase(TestCancellationMode.Aggregate)]
        public void Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_GenericHandleAsFallback_Throws_DueTo_InnerToken(TestCancellationMode cancellationMode)
        {
            using (var cancelTokenSource = new CancellationTokenSource())
            {
                Func<CancellationToken, int> action = null;
                if (cancellationMode == TestCancellationMode.OperationCanceled)
                {
                    action = (ct) => CancelableActions.GenericSyncActionThatCanceledOnOuterAndThrowOnInner(ct, cancelTokenSource);
                }
                else
                {
                    action = (ct) => CancelableActions.GenericSyncActionThatCanceledOnOuterAndThrowOnInnerAndThrowAgregateExc(ct, cancelTokenSource);
                }
                var fallbackResult = action.HandleAsFallback(cancelTokenSource.Token);
                Assert.That(fallbackResult.IsCanceled, Is.True);
            }
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForGenericFunc_ConvertedFromSync_If_Cancel_And_CancelToken_Is_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            var action = (Func<CancellationToken, int>)((_) => 1);
            var func = action.ToAsyncFunc();

            var funcRes = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);

            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForGenericFunc_ConvertedFromSync_If_Cancel_And_CancelToken_Is_Not_Native_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();

            var action = (Func<CancellationToken, int>)((_) => { var cancelTokenS1 = new CancellationTokenSource(); cancelTokenS1.Cancel(); cancelTokenS1.Token.ThrowIfCancellationRequested(); return 1; });
            var func = action.ToAsyncFunc();

            var funcRes = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);

            ClassicAssert.IsFalse(funcRes.IsCanceled);
            ClassicAssert.IsNotNull(funcRes.Error);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForGenericFunc_If_Cancel_And_CancelToken_In_FallbackMethod_Body()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            var func = (Func<CancellationToken, Task<int>>)(async (ct) => { await Task.Delay(1); ct.ThrowIfCancellationRequested(); return 1; });

            var funcRes = await func.HandleAsFallbackAsync(false, cancelTokenSource.Token);

            ClassicAssert.IsTrue(funcRes.IsCanceled);
            ClassicAssert.IsTrue(funcRes.Error is null);
            ClassicAssert.IsFalse(funcRes.IsSuccess);
            cancelTokenSource.Dispose();
        }

        [Test]
        public async Task Should_HandleAsFallbackAsync_ForGenericFunc_ForSuccess_Work()
        {
            Func<CancellationToken, Task<int>> func = async (_) => { await Task.Delay(1); return 1; };
            var funcRes = await func.HandleAsFallbackAsync(false, default);
            ClassicAssert.IsTrue(funcRes.IsSuccess);
        }
    }
}