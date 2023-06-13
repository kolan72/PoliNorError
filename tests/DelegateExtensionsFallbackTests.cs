using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsFallbackTests
    {
        [Test]
        public void Should_InvokeWithFallback_Work()
        {
            int i = 0;
            Action action = () => { i++; throw new Exception(); };
            void fallback() => Expression.Empty();
			action.InvokeWithFallback(fallback);

            int i1 = 0;
			void beforeFallbackError(Exception _)
			{
				i1++;
			}

			action.InvokeWithFallback(fallback, InvokeParams.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            Action<Exception, CancellationToken>  beforeFallbackErrorWithError = (Exception _, CancellationToken __) => i2++;
			action.InvokeWithFallback(fallback, beforeFallbackErrorWithError);
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, InvokeParams.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, InvokeParams.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public void Should_InvokeWithFallback_WithCancelTokenParam_Work()
        {
            int i = 0;
            Action action = () => { i++; throw new Exception(); };

            void fallback(CancellationToken _) => Expression.Empty();
			action.InvokeWithFallback(fallback);

            int i1 = 0;
			void beforeFallbackError(Exception _)
			{
				i1++;
			}

			action.InvokeWithFallback(fallback, InvokeParams.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __)
			{
				i2++;
			}

			action.InvokeWithFallback(fallback, InvokeParams.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, InvokeParams.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, InvokeParams.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsync_Work()
        {
            int i = 0;
            Func<CancellationToken, Task> fnAsync = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

			async Task fallbackAsync() { await Task.Delay(1); }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync);

            int i1 = 0;
			void beforeFallbackError(Exception _) { i1++; }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __)
			{
				i2++;
			}

			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsync_WithCancelTokenParam_Work()
        {
            int i = 0;
            Func<CancellationToken, Task> fnAsync = async (_) => { i++; await Task.Delay(1);  throw new Exception(); };

			async Task fallbackAsync(CancellationToken _) { await Task.Delay(1); }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync);

            int i1 = 0;
			void beforeFallbackError(Exception _)
			{
				i1++;
			}

			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __) { i2++; }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			await fnAsync.InvokeWithFallbackAsync(fallbackAsync, InvokeParams.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public void Should_InvokeWithSimple_Work()
        {
            int i = 0;
            Action action = () => { i++; throw new Exception(); };

            action.InvokeWithSimple();

            int i1 = 0;
            void beforeFallbackError(Exception _)
            {
                i1++;
            }

            action.InvokeWithSimple(InvokeParams.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
            void beforeFallbackErrorWithError(Exception _, CancellationToken __)
            {
                i2++;
            }

            action.InvokeWithSimple(InvokeParams.From(beforeFallbackErrorWithError));
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
    }
}