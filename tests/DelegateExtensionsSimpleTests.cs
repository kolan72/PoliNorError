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

            action.InvokeWithSimple(InvokeParams.From(beforeSimpleError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeSimpleErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            action.InvokeWithSimple(InvokeParams.From(beforeSimpleErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            action.InvokeWithSimple(InvokeParams.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            action.InvokeWithSimple(InvokeParams.From(beforeProcessErrorWithCancelAsync));
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
            func.InvokeWithSimple(InvokeParams.From((Exception _) => i1++));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeFallbackErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }
            func.InvokeWithSimple(InvokeParams.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            func.InvokeWithSimple(InvokeParams.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Func<Exception, CancellationToken, Task> beforeProcessErrorWithCancelAsync = (_, __) => { i4++; return Task.CompletedTask; };
            func.InvokeWithSimple(beforeProcessErrorWithCancelAsync);
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }
    }
}
