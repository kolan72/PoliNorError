using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class DelegateExtensionsTRetryTests
	{
		[Test]
		public void Should_InvokeWithRetry_Work()
		{
			int i = 0;
			Func<int> func = () => { i++; throw new Exception(); };
			const int retryCount = 1;
			func.InvokeWithRetry(retryCount);

			ClassicAssert.AreEqual(2, i);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			func.InvokeWithRetry(retryCount, ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			func.InvokeWithRetry(retryCount, actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			func.InvokeWithRetry(retryCount, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			func.InvokeWithRetry(retryCount, onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryWithDelay_Work()
		{
			int i = 0;
			Func<int> func = () => { i++; throw new Exception(); };
			const int retryCount = 1;
			func.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0));
			ClassicAssert.AreEqual(2, i);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			func.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			func.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			func.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			func.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryWithFunc_Work()
		{
			int i = 0;
			Func<int> func = () => { i++; throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			const int retryCount = 1;
			func.InvokeWithWaitAndRetry(retryCount, retryFunc);

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			func.InvokeWithWaitAndRetry(retryCount, retryFunc, ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			func.InvokeWithWaitAndRetry(retryCount, retryFunc, actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			func.InvokeWithWaitAndRetry(retryCount, retryFunc, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			func.InvokeWithWaitAndRetry(retryCount, retryFunc, onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryAndDelayWithFunc_Work()
		{
			int i = 0;
			Func<int> func = () => { i++; throw new Exception(); };

			const int retryCount = 1;

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			var retryDelay = new FakeRetryDelay();

			func.InvokeWithRetryDelay(retryCount, retryDelay, ErrorProcessorParam.From(actionError));

			Assert.That(i1, Is.EqualTo(1));
			Assert.That(retryDelay.AttemptsNumber, Is.EqualTo(1));

			func.InvokeWithRetryDelay(retryCount, retryDelay);
			Assert.That(retryDelay.AttemptsNumber, Is.EqualTo(2));
		}

		[Test]
		public async Task Should_InvokeWithRetryAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task<int>> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			const int retryCount = 1;
			await func.InvokeWithRetryAsync(retryCount);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithRetryAsync(retryCount, ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithRetryAsync(retryCount, actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithRetryAsync(retryCount, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithRetryAsync(retryCount, onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public async Task Should_InvokeWithRetryWithDelayAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task<int>> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			const int retryCount = 1;
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0));

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public async Task Should_InvokeWithRetryWithFuncAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task<int>> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);
			const int retryCount = 1;
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc,onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryInfinite_Work()
		{
			int i = 0;
			Func<int> action = () => { i++; throw new Exception(); };
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			action.InvokeWithRetryInfinite(token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }
			action.InvokeWithRetryInfinite(ErrorProcessorParam.From(actionError), token: cancelTokenSource2.Token);
			ClassicAssert.IsTrue(i1 > 0);
			cancelTokenSource.Dispose();
			cancelTokenSource2.Dispose();
		}

		[Test]
		public void Should_InvokeWithWaitRetryInfinite_WithDelay_Work()
		{
			int i = 0;
			Func<int> action = () => { i++; throw new Exception(); };
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			action.InvokeWithWaitAndRetryInfinite(TimeSpan.FromSeconds(0), token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }

			action.InvokeWithWaitAndRetryInfinite(TimeSpan.FromSeconds(0), ErrorProcessorParam.From(actionError), token: cancelTokenSource2.Token);
			ClassicAssert.IsTrue(i1 > 0);
			cancelTokenSource.Dispose();
			cancelTokenSource2.Dispose();
		}

		[Test]
		public void Should_InvokeWithWaitRetryInfinite_WithDelayFunc_Work()
		{
			int i = 0;
			Func<int> action = () => { i++; throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);

			action.InvokeWithWaitAndRetryInfinite(retryFunc, token: cancelTokenSource.Token);

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }

			action.InvokeWithWaitAndRetryInfinite(retryFunc, ErrorProcessorParam.From(actionError), token: cancelTokenSource2.Token);

			ClassicAssert.IsTrue(i > 0);
			ClassicAssert.IsTrue(i1 > 0);

			cancelTokenSource.Dispose();
			cancelTokenSource2.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task<int>> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);

			await func.InvokeWithRetryInfiniteAsync(token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_WithDelay_Work()
		{
			int i = 0;
			Func<CancellationToken, Task<int>> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			await func.InvokeWithWaitAndRetryInfiniteAsync(TimeSpan.FromSeconds(0), token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_WithDelayFunc_Work()
		{
			int i = 0;
			Func<CancellationToken, Task<int>> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			await func.InvokeWithWaitAndRetryInfiniteAsync(retryFunc, token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}
	}
}