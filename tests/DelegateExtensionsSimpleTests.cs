﻿using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.AreEqual(1, i);

            int i1 = 0;
            void beforeSimpleError(Exception _)
            {
                i1++;
            }

            action.InvokeWithSimple(ErrorProcessorParam.From(beforeSimpleError));
            ClassicAssert.AreEqual(1, i1);

            int i2 = 0;
            void beforeSimpleErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            action.InvokeWithSimple(ErrorProcessorParam.From(beforeSimpleErrorWithError));
            ClassicAssert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            action.InvokeWithSimple(ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
            ClassicAssert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            action.InvokeWithSimple(ErrorProcessorParam.From(beforeProcessErrorWithCancelAsync));
            ClassicAssert.AreEqual(1, i4);

            ClassicAssert.AreEqual(5, i);
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void Should_InvokeWithSimple_WithErrorFilter_Work(bool errorFilterUnsatisfied, bool notEmpty)
        {
            CatchBlockFilter errorFilter = null;
            if (errorFilterUnsatisfied)
            {
                errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentNullException>() : new CatchBlockFilter().IncludeError<ArgumentNullException>();
            }
            else
            {
                errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>() : new CatchBlockFilter().IncludeError<ArgumentException>();
            }

            int i = 0;
            Action action = () => { i++; throw new ArgumentException("Test"); };
            var policyResult = action.InvokeWithSimple(errorFilter, ErrorProcessorParam.From((_) => { }));
            Assert.That(policyResult.ErrorFilterUnsatisfied, Is.EqualTo(errorFilterUnsatisfied));
            Assert.That(i, Is.EqualTo(1));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_InvokeWithSimple_WithCatchBlockHandler_Work(bool include)
        {
            CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByExcluding(_ => true));
            var exception = new ArgumentException("Test");
            CatchBlockHandler handler = null;
            int k = 0;
            if (include)
            {
                handler = CatchBlockHandlerFactory
                          .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>());
            }
            else
            {
                handler = CatchBlockHandlerFactory
                         .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>());
            }
            handler.WithErrorProcessorOf((_) => k++);
            int i = 0;
            Action action = () => { i++; throw exception; };
            var policyResult = action.InvokeWithSimple(handler);
            Assert.That(i, Is.EqualTo(1));
            if (include)
            {
                Assert.That(k, Is.EqualTo(1));
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.False);
            }
            else
            {
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.True);
            }
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public async Task Should_InvokeWithSimpleAsync_WithErrorFilter_Work(bool errorFilterUnsatisfied, bool notEmpty)
        {
            CatchBlockFilter errorFilter = null;
            if (errorFilterUnsatisfied)
            {
                errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentNullException>() : new CatchBlockFilter().IncludeError<ArgumentNullException>();
            }
            else
            {
                errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>() : new CatchBlockFilter().IncludeError<ArgumentException>();
            }
            int i = 0;
            Func<CancellationToken, Task> fun = async(_) => { await Task.Delay(1); i++;  throw new ArgumentException("Test"); };
            var policyResult =  await fun.InvokeWithSimpleAsync(errorFilter, ErrorProcessorParam.From((_) => { }));
            Assert.That(policyResult.ErrorFilterUnsatisfied, Is.EqualTo(errorFilterUnsatisfied));
            Assert.That(i, Is.EqualTo(1));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_InvokeWithSimpleAsync_WithCatchBlockHandler_Work(bool include)
        {
            var exception = new ArgumentException("Test");
            CatchBlockHandler handler = null;
            int k = 0;
            if (include)
            {
                handler = CatchBlockHandlerFactory
                          .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>());
            }
            else
            {
                handler = CatchBlockHandlerFactory
                         .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>());
            }
            handler.WithErrorProcessorOf((_) => k++);
            int i = 0;
            Func<CancellationToken, Task> fun = async(_) => {await Task.Delay(1); i++; throw exception; };
            var policyResult = await fun.InvokeWithSimpleAsync(handler);
            Assert.That(i, Is.EqualTo(1));
            if (include)
            {
                Assert.That(k, Is.EqualTo(1));
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.False);
            }
            else
            {
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.True);
            }
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public async Task Should_InvokeWithSimpleAsyncT_WithErrorFilter_Work(bool errorFilterUnsatisfied, bool notEmpty)
        {
            CatchBlockFilter errorFilter = null;
            if (errorFilterUnsatisfied)
            {
				errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentNullException>() : new CatchBlockFilter().IncludeError<ArgumentNullException>();
			}
			else
			{
				errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>() : new CatchBlockFilter().IncludeError<ArgumentException>();
			}
            int i = 0;
            Func<CancellationToken, Task<int>> fun = async (_) => { await Task.Delay(1); i++; throw new ArgumentException("Test"); };
            var policyResult = await fun.InvokeWithSimpleAsync(errorFilter, ErrorProcessorParam.From((_) => { }));
            Assert.That(policyResult.ErrorFilterUnsatisfied, Is.EqualTo(errorFilterUnsatisfied));
            Assert.That(i, Is.EqualTo(1));
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void Should_InvokeWithSimpleT_WithErrorFilter_Work(bool errorFilterUnsatisfied, bool notEmpty)
        {
            CatchBlockFilter errorFilter = null;
            if (errorFilterUnsatisfied)
            {
				errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentNullException>() : new CatchBlockFilter().IncludeError<ArgumentNullException>();
			}
			else
			{
				errorFilter = notEmpty ? NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>() : new CatchBlockFilter().IncludeError<ArgumentException>();
			}

            int i = 0;
            Func<int> fun = () => { i++; throw new ArgumentException("Test"); };
            var policyResult = fun.InvokeWithSimple(errorFilter, ErrorProcessorParam.From((_) => { }));
            Assert.That(policyResult.ErrorFilterUnsatisfied, Is.EqualTo(errorFilterUnsatisfied));
            Assert.That(i, Is.EqualTo(1));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_InvokeWithSimpleT_WithCatchBlockHandler_Work(bool include)
        {
            var exception = new ArgumentException("Test");
            CatchBlockHandler handler = null;
            int k = 0;
            if (include)
            {
                handler = CatchBlockHandlerFactory
                          .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>());
            }
            else
            {
                handler = CatchBlockHandlerFactory
                         .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>());
            }
            handler.WithErrorProcessorOf((_) => k++);
            int i = 0;
            Func<int> fun = () => { i++; throw exception; };
            var policyResult = fun.InvokeWithSimple(handler);
            Assert.That(i, Is.EqualTo(1));
            if (include)
            {
                Assert.That(k, Is.EqualTo(1));
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.False);
            }
            else
            {
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.True);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_InvokeWithSimpleAsyncT_WithCatchBlockHandler_Work(bool include)
        {
            var exception = new ArgumentException("Test");
            CatchBlockHandler handler = null;
            int k = 0;
            if (include)
            {
                handler = CatchBlockHandlerFactory
                          .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>());
            }
            else
            {
                handler = CatchBlockHandlerFactory
                         .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>());
            }
            handler.WithErrorProcessorOf((_) => k++);
            int i = 0;
            Func<CancellationToken, Task<int>> fun = async (_) => { await Task.Delay(1); i++; throw exception; };
            var policyResult = await fun.InvokeWithSimpleAsync(handler);
            Assert.That(i, Is.EqualTo(1));
            if (include)
            {
                Assert.That(k, Is.EqualTo(1));
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.False);
            }
            else
            {
                Assert.That(policyResult.ErrorFilterUnsatisfied, Is.True);
            }
        }

        [Test]
        public void Should_InvokeWithSimpleT_Work()
        {
            int i = 0;
            Func<int> func = () => { i++; throw new Exception(); };

            func.InvokeWithSimple();
            ClassicAssert.AreEqual(1, i);

            int i1 = 0;
            func.InvokeWithSimple(ErrorProcessorParam.From((Exception _) => i1++));
            ClassicAssert.AreEqual(1, i1);

            int i2 = 0;
            void beforeErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }
            func.InvokeWithSimple(ErrorProcessorParam.From(beforeErrorWithError));
            ClassicAssert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            func.InvokeWithSimple(ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
            ClassicAssert.AreEqual(1, i3);

            int i4 = 0;
            Func<Exception, CancellationToken, Task> beforeProcessErrorWithCancelAsync = (_, __) => { i4++; return Task.CompletedTask; };
            func.InvokeWithSimple(beforeProcessErrorWithCancelAsync);
            ClassicAssert.AreEqual(1, i4);

            ClassicAssert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithSimpleAsync_Work()
        {
            int i = 0;
            Func<CancellationToken, Task> fnAsync = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

            await fnAsync.InvokeWithSimpleAsync();
            ClassicAssert.AreEqual(1, i);

            int i1 = 0;
            void beforeError(Exception _) { i1++; }
            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeError));
            ClassicAssert.AreEqual(1, i1);

            int i2 = 0;
            void beforeErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeErrorWithError));
            ClassicAssert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
            ClassicAssert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            await fnAsync.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeProcessErrorWithCancelAsync));
            ClassicAssert.AreEqual(1, i4);

            ClassicAssert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithSyncTAsync_Work()
        {
            int i = 0;
            Func<CancellationToken, Task<int>> fn = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

            await fn.InvokeWithSimpleAsync();
            ClassicAssert.AreEqual(1, i);

            int i1 = 0;
            void beforeError(Exception _)
            {
                i1++;
            }

            await fn.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeError));
            ClassicAssert.AreEqual(1, i1);

            int i2 = 0;
            void beforeErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            await fn.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeErrorWithError));
            ClassicAssert.AreEqual(1, i2);

            int i3 = 0;
            Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
            await fn.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
            ClassicAssert.AreEqual(1, i3);

            int i4 = 0;
            Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
            await fn.InvokeWithSimpleAsync(ErrorProcessorParam.From(beforeProcessErrorWithCancelAsync));
            ClassicAssert.AreEqual(1, i4);

            ClassicAssert.AreEqual(5, i);
        }

        [Test]
        public void Should_Invoke_Work_When_Called_From_BasicErrorProcessor()
        {
            int i = 0;
            Func<int> func = () => { i++; throw new Exception(); };

            int i1 = 0;
            var res = func.InvokeWithSimple(new BasicErrorProcessor((_, __) => ++i1));

            ClassicAssert.AreEqual(1, i1);
            ClassicAssert.AreEqual(1, i);
            ClassicAssert.IsTrue(res.Errors.Count() == 1);
        }

        [Test]
        public void Should_Invoke_Work_When_Called_From_DefaultErrorProcessor()
        {
            int i = 0;
            Func<int> func = () => { i++; throw new Exception(); };

            int i1 = 0;
            var res = func.InvokeWithSimple(new DefaultErrorProcessor((_, __) => ++i1));

            ClassicAssert.AreEqual(1, i1);
            ClassicAssert.AreEqual(1, i);
            ClassicAssert.IsTrue(res.Errors.Count() == 1);
        }
    }
}
