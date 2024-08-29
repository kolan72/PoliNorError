using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Threading;
using NUnit.Framework.Legacy;

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

			ClassicAssert.AreEqual(2, i);

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			action.InvokeWithRetry(retryCount, ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			ClassicAssert.AreEqual(4, i);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			action.InvokeWithRetry(retryCount, actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			ClassicAssert.AreEqual(6, i);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithRetry(retryCount, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			ClassicAssert.AreEqual(8, i);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			action.InvokeWithRetry(retryCount, onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithWaitAndRetry_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			const int retryCount = 1;
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0));
			ClassicAssert.AreEqual(2, i);

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask;};
			action.InvokeWithWaitAndRetry(retryCount, TimeSpan.FromSeconds(0), onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
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

			action.InvokeWithWaitAndRetry(retryCount, retryFunc, ErrorProcessorParam.From(actionError));
			ClassicAssert.AreEqual(1, i1);

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			action.InvokeWithWaitAndRetry(retryCount, retryFunc, actionCancelError);
			ClassicAssert.AreEqual(1, i2);

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			action.InvokeWithWaitAndRetry(retryCount, retryFunc, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			ClassicAssert.AreEqual(1, i3);

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			action.InvokeWithWaitAndRetry(retryCount, retryFunc, onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_InvokeWithRetryAndDelayWithAction_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };

			const int retryCount = 1;

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			var retryDelay = new FakeRetryDelay();

			action.InvokeWithRetryDelay(retryCount, retryDelay, ErrorProcessorParam.From(actionError));

			Assert.That(i1, Is.EqualTo(1));
			Assert.That(retryDelay.AttemptsNumber, Is.EqualTo(1));

			action.InvokeWithRetryDelay(retryCount, retryDelay);
			Assert.That(retryDelay.AttemptsNumber, Is.EqualTo(2));
		}

		[Test]
		public async Task Should_InvokeWithRetryAndDelayWithFuncAsync_Work()
		{
			var retryDelay = new FakeRetryDelay();

			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

			const int retryCount = 1;

			int i1 = 0;
			void actionError(Exception _) { i1++; }
			await func.InvokeWithRetryDelayAsync(retryCount, retryDelay, ErrorProcessorParam.From(actionError));
			Assert.That(i1, Is.EqualTo(1));

			int i2 = 0;
			Action<Exception, CancellationToken> actionCancelError = (_, __) => i2++;
			await func.InvokeWithRetryDelayAsync(retryCount, retryDelay, actionCancelError);
			Assert.That(i2, Is.EqualTo(1));

			int i3 = 0;
			Task beforeProcessErrorAsync(Exception _) { i3++; return Task.CompletedTask; }
			await func.InvokeWithRetryDelayAsync(retryCount, retryDelay, ErrorProcessorParam.From(beforeProcessErrorAsync, CancellationType.Cancelable));
			Assert.That(i3, Is.EqualTo(1));

			int i4 = 0;
			Func<Exception, CancellationToken, Task> onBeforeProcessErrorWithTokenAsync = (_, __) => { i4++; return Task.CompletedTask; };
			await func.InvokeWithRetryDelayAsync(retryCount, retryDelay, onBeforeProcessErrorWithTokenAsync);
			Assert.That(i4, Is.EqualTo(1));

			Assert.That(i,Is.EqualTo(8));
			Assert.That(retryDelay.AttemptsNumber, Is.EqualTo(4));
		}

		[Test]
		public void Should_InvokeWithRetryAndDelayInfiniteWithAction_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };

			int i1 = 0;
			void actionError(Exception _)
			{
				i1++;
			}

			var retryDelay = new FakeRetryDelay();

			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.CancelAfter(100);
				action.InvokeWithRetryDelayInfinite(retryDelay, ErrorProcessorParam.From(actionError), token: cancelTokenSource.Token);

				Assert.That(i > 0, Is.True);
				Assert.That(i1 > 0, Is.True);
			}

			int k = 0;
			Action action2 = () => { k++; throw new Exception(); };
			using (var cancelTokenSource2 = new CancellationTokenSource())
			{
				cancelTokenSource2.CancelAfter(100);
				action2.InvokeWithRetryDelayInfinite(retryDelay, token: cancelTokenSource2.Token);

				Assert.That(k > 0, Is.True);
			}

			Assert.That(retryDelay.AttemptsNumber > 0, Is.True);
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
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
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
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
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
			await func.InvokeWithWaitAndRetryAsync(retryCount, retryFunc, onBeforeProcessErrorWithTokenAsync);
			ClassicAssert.AreEqual(1, i4);

			ClassicAssert.AreEqual(10, i);
		}

		[Test]
		public void Should_Retry_1_For_Action_Be_Correct()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			var polResult = action.InvokeWithRetry(1);

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(2, polResult.Errors.Count());
			ClassicAssert.AreEqual(true, polResult.IsFailed);
		}

		[Test]
		public void Should_Retry_Ping_For_Action_Be_Correct()
		{
			var ping = new Ping();
			Action action = () => ping.Send("google2.com");
			var polResult = action.InvokeWithRetry(3);

			ClassicAssert.AreEqual(4, polResult.Errors.Count());
			ClassicAssert.AreEqual(true, polResult.IsFailed);
		}

		[Test]
		public void Should_InvokeWithRetryInfinite_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			action.InvokeWithRetryInfinite(token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }
			action.InvokeWithRetryInfinite(ErrorProcessorParam.From(actionError), token: cancelTokenSource2.Token);
			ClassicAssert.IsTrue(i1 > 0);
			cancelTokenSource2.Dispose();
		}

		[Test]
		public void Should_InvokeWithWaitRetryInfinite_Work()
		{
			int i = 0;
			Action action = () => { i++; throw new Exception(); };
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			action.InvokeWithWaitAndRetryInfinite(TimeSpan.FromSeconds(0), token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();

			int i1 = 0;
			var cancelTokenSource2 = new CancellationTokenSource();
			cancelTokenSource2.CancelAfter(100);
			void actionError(Exception _) { i1++; }

			action.InvokeWithWaitAndRetryInfinite(TimeSpan.FromSeconds(0), ErrorProcessorParam.From(actionError), token: cancelTokenSource2.Token);
			ClassicAssert.IsTrue(i1 > 0);
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
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);

			await func.InvokeWithRetryInfiniteAsync(token:cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_InvokeWithRetryInfiniteAsync_WithDelay_Work()
		{
			int i = 0;
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };

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
			Func<CancellationToken, Task> func = async (_) => { i++; await Task.Delay(0); throw new Exception(); };
			TimeSpan retryFunc(int _, Exception __) => TimeSpan.FromSeconds(0);

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			await func.InvokeWithWaitAndRetryInfiniteAsync(retryFunc, token: cancelTokenSource.Token);
			ClassicAssert.IsTrue(i > 0);
			cancelTokenSource.Dispose();
		}

		private class FakeRetryDelay : RetryDelay
		{
			public int AttemptsNumber { get; private set; }

			public override TimeSpan GetDelay(int attempt)
			{
				AttemptsNumber++;
				return TimeSpan.Zero;
			}
		}
	}
}