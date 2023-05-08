using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    internal class DelegateExtensionsTFallbackTests
    {
        [Test]
        public void Should_InvokeWithFallback_Work()
        {
            int i = 0;
            Func<int> action = () => { i++; throw new Exception(); };

            int fallback() => 1;
			var polResult = action.InvokeWithFallback(fallback);
            Assert.AreEqual(1, polResult.Result);

            int i1 = 0;
            var polResult2 = action.InvokeWithFallback(fallback, FallbackErrorProcessor.From((Exception _) => i1++));
            Assert.AreEqual(1, i1);
            Assert.AreEqual(1, polResult2.Result);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __)
            {
				i2++;
			}
			action.InvokeWithFallback(fallback, FallbackErrorProcessor.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, FallbackErrorProcessor.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
            Func<Exception, CancellationToken, Task> beforeProcessErrorWithCancelAsync = (_, __) => { i4++; return Task.CompletedTask; };
            action.InvokeWithFallback(fallback, beforeProcessErrorWithCancelAsync);
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public void Should_InvokeWithFallback__WithCancelTokenParam__Work()
        {
            int i = 0;
            Func<int> action = () => { i++; throw new Exception(); };

			int fallback(CancellationToken _) => 1;
			var polResult = action.InvokeWithFallback(fallback);
            Assert.AreEqual(1, polResult.Result);

            int i1 = 0;
			void beforeFallbackError(Exception _)
			{
				i1++;
			}

			action.InvokeWithFallback(fallback, FallbackErrorProcessor.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __)
			{
				i2++;
			}

			action.InvokeWithFallback(fallback, FallbackErrorProcessor.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, FallbackErrorProcessor.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			action.InvokeWithFallback(fallback, FallbackErrorProcessor.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsync_WithCancelTokenParam_Work()
        {
            int i = 0;
            Func<CancellationToken, Task<int>> fn = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); return 1; }
			var polResult = await fn.InvokeWithFallbackAsync(fallback);
            Assert.AreEqual(1, polResult.Result);

            int i1 = 0;
			void beforeFallbackError(Exception _)
			{
				i1++;
			}

			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __)
			{
				i2++;
			}

			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }

        [Test]
        public async Task Should_InvokeWithFallbackAsync_Work()
        {
            int i = 0;
            Func<CancellationToken, Task<int>> fn = async (_) => { i++; await Task.Delay(1); throw new Exception(); };

			async Task<int> fallback() { await Task.Delay(1); return 1; }
			var polResult = await fn.InvokeWithFallbackAsync(fallback);
            Assert.AreEqual(1, polResult.Result);

            int i1 = 0;
			void beforeFallbackError(Exception _)
			{
				i1++;
			}

			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeFallbackError));
            Assert.AreEqual(1, i1);

            int i2 = 0;
			void beforeFallbackErrorWithError(Exception _, CancellationToken __)
			{
				i2++;
			}

			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeFallbackErrorWithError));
            Assert.AreEqual(1, i2);

            int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeProcessErrorAsync, ConvertToCancelableFuncType.Cancelable));
            Assert.AreEqual(1, i3);

            int i4 = 0;
			Task beforeProcessErrorWithCancelAsync(Exception _, CancellationToken __) { i4++; return Task.CompletedTask; }
			await fn.InvokeWithFallbackAsync(fallback, FallbackErrorProcessor.From(beforeProcessErrorWithCancelAsync));
            Assert.AreEqual(1, i4);

            Assert.AreEqual(5, i);
        }
    }
}