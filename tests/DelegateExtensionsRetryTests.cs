using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Threading;

namespace PoliNorError.Tests
{
	internal class DelegateExtensionsRetryTests
	{
		[Test]
		public void Should_InvokeWithRetry_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			const int retryCount = 1;
			action.InvokeWithRetry(retryCount);

			Assert.AreEqual(2, i);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			action.InvokeWithRetry(retryCount, ErrorProcessorDelegate.From(actionError));
			Assert.AreEqual(1, i1);

			Assert.AreEqual(4, i);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			action.InvokeWithRetry(retryCount, actionCancelError);
			Assert.AreEqual(1, i2);

			Assert.AreEqual(6, i);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithRetry(retryCount, ErrorProcessorDelegate.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.AreEqual(1, i3);

			Assert.AreEqual(8, i);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			action.InvokeWithRetry(retryCount, onBeforeProcessErrorWithTokenAsync);
			Assert.AreEqual(1, i4);

			Assert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryWithDelay_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			const int retryCount = 1;
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0));
			Assert.AreEqual(2, i);

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorDelegate.From(actionError));
			Assert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), actionCancelError);
			Assert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorDelegate.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask;};
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), onBeforeProcessErrorWithTokenAsync);
			Assert.AreEqual(1, i4);

			Assert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryWithFunc_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			const int retryCount = 1;
			action.InvokeWithWaitAndRetry(retryCount, retryFunc);

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			action.InvokeWithWaitAndRetry(retryCount, retryFunc, ErrorProcessorDelegate.From(actionError));
			Assert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			action.InvokeWithWaitAndRetry(retryCount, retryFunc, actionCancelError);
			Assert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithWaitAndRetry(retryCount, retryFunc, ErrorProcessorDelegate.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			action.InvokeWithWaitAndRetry(retryCount, retryFunc, onBeforeProcessErrorWithTokenAsync);
			Assert.AreEqual(1, i4);

			Assert.AreEqual(10, i);
		}

		[Test]
		public async Task Should_InvokeWithRetryAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			const int retryCount = 1;
			await func.InvokeWithRetryAsync(retryCount);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithRetryAsync(retryCount, ErrorProcessorDelegate.From(actionError));
			Assert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithRetryAsync(retryCount, actionCancelError);
			Assert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithRetryAsync(retryCount, ErrorProcessorDelegate.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithRetryAsync(retryCount, onBeforeProcessErrorWithTokenAsync);
			Assert.AreEqual(1, i4);

			Assert.AreEqual(10, i);
		}

		[Test]
		public async Task Should_InvokeWithRetryWithDelayAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			const int retryCount = 1;
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0));

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorDelegate.From(actionError));
			Assert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), actionCancelError);
			Assert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorDelegate.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithWaitAndRetryAsync(retryCount, TimeSpan.FromSeconds(0), onBeforeProcessErrorWithTokenAsync);
			Assert.AreEqual(1, i4);

			Assert.AreEqual(10, i);
		}

		[Test]
		public async Task Should_InvokeWithRetryWithFuncAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);
			const int retryCount = 1;
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, ErrorProcessorDelegate.From(actionError));
			Assert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, actionCancelError);
			Assert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, ErrorProcessorDelegate.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, onBeforeProcessErrorWithTokenAsync);
			Assert.AreEqual(1, i4);

			Assert.AreEqual(10, i);
		}

		[Test]
		public void Should_Retry_1_For_Action_Be_Correct()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			var polResult = action.InvokeWithRetry(1);

			Assert.AreEqual(2, i);
			Assert.AreEqual(2, polResult.Errors.Count());
			Assert.AreEqual(true, polResult.IsFailed);
		}

		[Test]
		public void Should_Retry_Ping_For_Action_Be_Correct()
		{
			var ping = new Ping();
			Action action = () => ping.Send("google2.com");
			var polResult = action.InvokeWithRetry(3);

			Assert.AreEqual(4, polResult.Errors.Count());
			Assert.AreEqual(true, polResult.IsFailed);
		}

		[Test]
		public void Should_InvokeWithRetryInfinite_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			action.InvokeWithRetryInfinite(cancelTokenSource.Token);
			Assert.IsTrue(i > 0);
			cancelTokenSource.Dispose();

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }
			action.InvokeWithRetryInfinite(ErrorProcessorDelegate.From(actionError), cancelTokenSource2.Token);
			Assert.IsTrue(i1 > 0);
			cancelTokenSource2.Dispose();
		}

		[Test]
		public void Should_InvokeWithWaitRetryInfinite_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			action.InvokeWithWaitAndRetryInfinite(TimeSpan.FromSeconds(0), cancelTokenSource.Token);
			Assert.IsTrue(i > 0);
			cancelTokenSource.Dispose();

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }

			action.InvokeWithWaitAndRetryInfinite(TimeSpan.FromSeconds(0), ErrorProcessorDelegate.From(actionError), cancelTokenSource2.Token);
			Assert.IsTrue(i1 > 0);
			cancelTokenSource2.Dispose();
		}

		[Test]
		public void Should_InvokeWithWaitRetryInfinite_WithDelayFunc_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);

			action.InvokeWithWaitAndRetryInfinite(retryFunc, cancelTokenSource.Token);

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }

			action.InvokeWithWaitAndRetryInfinite(retryFunc, ErrorProcessorDelegate.From(actionError), cancelTokenSource2.Token);

			Assert.IsTrue(i > 0);
			Assert.IsTrue(i1 > 0);

			cancelTokenSource.Dispose();
			cancelTokenSource2.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);

			await func.InvokeWithRetryInfiniteAsync(cancelTokenSource.Token);
			Assert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_WithDelay_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			await func.InvokeWithWaitAndRetryInfiniteAsync(TimeSpan.FromSeconds(0), cancelTokenSource.Token);
			Assert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_WithDelayFunc_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			await func.InvokeWithWaitAndRetryInfiniteAsync(retryFunc, cancelTokenSource.Token);
			Assert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}
	}
}