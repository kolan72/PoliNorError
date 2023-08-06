using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsSimpleTests
    {
        [Test]
        public void Should_InvokeWithSimple_Work()
        {
            int i = 0;
            Action action = () => { i++; throw new Exception(); };

            action.InvokeWithSimple();
            Assert.AreEqual(1, i);

            int i1 = 0;
            void beforeSimpleError(Exception _)
            {
                i1++;
            }

            action.InvokeWithSimple(ErrorProcessorDelegate.From(beforeSimpleError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeSimpleErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            action.InvokeWithSimple(ErrorProcessorDelegate.From(beforeSimpleErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            action.InvokeWithSimple(ErrorProcessorDelegate.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            action.InvokeWithSimple(ErrorProcessorDelegate.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public void Should_InvokeWithSimpleT_Work()
        {
            int i = 0;
            Func<int> func = () => { i++; throw new Exception(); };

            func.InvokeWithSimple();
            Assert.AreEqual(1, i);

            int i1 = 0;
            func.InvokeWithSimple(ErrorProcessorDelegate.From((Exception _) => i1++));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }
            func.InvokeWithSimple(ErrorProcessorDelegate.From(beforeErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            func.InvokeWithSimple(ErrorProcessorDelegate.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Func<Exception, CancellationToken, Task> beforeProcessErrorWithCancelAsync = (_, __) => { i4++; return Task.CompletedTask; };
            func.InvokeWithSimple(beforeProcessErrorWithCancelAsync);
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithSimpleAsync_Work()
        {
            int i = 0;
            Func<CancellationToken, Task> fnAsync = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

            await fnAsync.InvokeWithSimpleAsync();
            Assert.AreEqual(1, i);

            int i1 = 0;
            void beforeError(Exception _) { i1++; }
            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithSyncTAsync_Work()
        {
            int i = 0;
            Func<CancellationToken, Task<int>> fn = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

            await fn.InvokeWithSimpleAsync();
            Assert.AreEqual(1, i);

            int i1 = 0;
            void beforeError(Exception _)
            {
                i1++;
            }

            await fn.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            await fn.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            await fn.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            await fn.InvokeWithSimpleAsync(ErrorProcessorDelegate.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }
    }
}
