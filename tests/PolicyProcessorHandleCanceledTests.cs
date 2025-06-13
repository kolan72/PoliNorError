using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyProcessorHandleCanceledTests
	{
		[Test]
		public void Should_RetryT_Break_And_BeFailedCanceled_WhenCanceledAndNativeException()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Func<int> save = CanceledGetAwaiterFunc(cancelTokenSource);
				var polRetryResult = RetryProcessor
									.CreateDefault()
									.Retry(save, 1, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polRetryResult.IsFailed);
				ClassicAssert.IsTrue(polRetryResult.IsCanceled);
				ClassicAssert.AreEqual(0, polRetryResult.Errors.Count());
				Assert.That(polRetryResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		[Test]
		public void Should_RetryT_Break_And_BeFailedCanceled_WhenCanceledAndAggregateException()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Func<int> save = CancelWaiterFunc(cancelTokenSource);

				var polRetryResult = RetryProcessor
									.CreateDefault()
									.Retry(save, 1, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polRetryResult.IsFailed);
				ClassicAssert.IsTrue(polRetryResult.IsCanceled);
				ClassicAssert.AreEqual(0, polRetryResult.Errors.Count());
				Assert.That(polRetryResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		[Test]
		public void Should_RetryT_BeFailedCanceled_WhenCanceledAfterFirstRetry()
		{
			var cancelTokenSource = new CancellationTokenSource();
			int save() { throw new Exception(); }
			var polRetryResult = RetryProcessor
								.CreateDefault()
								.WithErrorProcessorOf(_actCancel(cancelTokenSource))
								.Retry(save, 2, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polRetryResult.IsFailed);
			ClassicAssert.IsTrue(polRetryResult.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_Retry_BeFailedCanceled_WhenCanceledAfterFirstRetry()
		{
			var cancelTokenSource = new CancellationTokenSource();
			void save() { throw new Exception(); }
			var processor = RetryProcessor.CreateDefault().WithErrorProcessorOf(_actCancel(cancelTokenSource));
			var polRetryResult = processor.Retry(save, 2, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polRetryResult.IsFailed);
			ClassicAssert.IsTrue(polRetryResult.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_RetryAsync_BeFailedCanceled_WhenCanceledAfterFirstRetry()
		{
			var cancelTokenSource = new CancellationTokenSource();
			async Task save(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var processor = new DefaultRetryProcessor().WithErrorProcessorOf(_actCancel(cancelTokenSource));
			var polRetryResult = await processor.RetryAsync(save, 2, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polRetryResult.IsFailed);
			ClassicAssert.IsTrue(polRetryResult.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_RetryAsyncT_BeFailedCanceled_WhenCanceledAfterFirstRetry()
		{
			var cancelTokenSource = new CancellationTokenSource();
			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var processor = new DefaultRetryProcessor().WithErrorProcessorOf(_actCancel(cancelTokenSource));
			var polRetryResult = await processor.RetryAsync(save, 2, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polRetryResult.IsFailed);
			ClassicAssert.IsTrue(polRetryResult.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_Retry_Break_And_BeFailedCanceled_WhenCanceledAndNativeException()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Action save = CanceledGetAwaiterAct(cancelTokenSource);
				var processor = new DefaultRetryProcessor();
				var polRetryResult = processor.Retry(save, 1, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polRetryResult.IsFailed);
				ClassicAssert.IsTrue(polRetryResult.IsCanceled);
				ClassicAssert.AreEqual(0, polRetryResult.Errors.Count());
				Assert.That(polRetryResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		[Test]
		public void Should_Retry_Break_And_BeFailedCanceled_WhenCanceledAndAggregateException()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Action save = CancelWaiterAct(cancelTokenSource);
				var processor = new DefaultRetryProcessor();
				var polRetryResult = processor.Retry(save, 1, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polRetryResult.IsFailed);
				ClassicAssert.IsTrue(polRetryResult.IsCanceled);
				ClassicAssert.AreEqual(0, polRetryResult.Errors.Count());
				Assert.That(polRetryResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		[Test]
		public void Should_Fallback_Break_And_BeFailedCanceled_WhenCanceledAndNativeException()
		{
			var cancelTokenSource = new CancellationTokenSource();
			Action save = CanceledGetAwaiterAct(cancelTokenSource);
			var processor = new DefaultFallbackProcessor();
			var polRetryResult = processor.Fallback(save, (_) => Expression.Empty(), cancelTokenSource.Token);
			ClassicAssert.IsTrue(polRetryResult.IsFailed);
			ClassicAssert.IsTrue(polRetryResult.IsCanceled);
			ClassicAssert.AreEqual(0, polRetryResult.Errors.Count());
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_Fallback_Break_And_BeFailedCanceled_WhenCanceledAndAggregateException()
		{
			var cancelTokenSource = new CancellationTokenSource();
			Action save = CancelWaiterAct(cancelTokenSource);
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback(save, (_) => Expression.Empty(), cancelTokenSource.Token);
			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.AreEqual(0, polResult.Errors.Count());
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_FallbackT_Break_And_BeFailedCanceled_WhenCanceledAndNativeException()
		{
			var cancelTokenSource = new CancellationTokenSource();
			Func<int> save = CanceledGetAwaiterFunc(cancelTokenSource);
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback(save, (_) => 1, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.AreEqual(0, polResult.Errors.Count());
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_FallbackT_Break_And_BeFailedCanceled_WhenCanceledAndAggregateException()
		{
			var cancelTokenSource = new CancellationTokenSource();
			Func<int> save = CancelWaiterFunc(cancelTokenSource);
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback(save, (_) => 1, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.AreEqual(0, polResult.Errors.Count());
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_FallbackAsync_Break_And_BeFailedCanceled_WhenCanceledAndNativeException()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Func<CancellationToken, Task> save = CanceledTaskFunc(cancelTokenSource);
				var processor = new DefaultFallbackProcessor();
				var polResult = await processor.FallbackAsync(save, async (_) => await Task.Delay(1), false, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polResult.IsFailed);
				ClassicAssert.IsTrue(polResult.IsCanceled);
				ClassicAssert.AreEqual(0, polResult.Errors.Count());
				Assert.That(polResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		[Test]
		public async Task Should_FallbackAsyncT_Break_And_BeFailedCanceled_WhenCanceledAndNativeException()
		{
			var cancelTokenSource = new CancellationTokenSource();
			Func<CancellationToken, Task<int>> save = CanceledTaskFuncT(cancelTokenSource);
			var processor = new DefaultFallbackProcessor();
			var polResult = await processor.FallbackAsync(save, async (_) => { await Task.Delay(1); return 1; }, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.AreEqual(0, polResult.Errors.Count());
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_RetryAsync_Break_And_BeFailedCanceled_WhenCanceledAndNativeException2()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Func<CancellationToken, Task> save = CanceledTaskFunc(cancelTokenSource);
				var processor = new DefaultRetryProcessor();
				var polResult = await processor.RetryAsync(save, 1, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polResult.IsFailed);
				ClassicAssert.IsTrue(polResult.IsCanceled);
				ClassicAssert.AreEqual(0, polResult.Errors.Count());
				Assert.That(polResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		[Test]
		public async Task Should_RetryAsyncT_Break_And_BeFailedCanceled_WhenCanceledAndNativeException2()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Func<CancellationToken, Task<int>> save = CanceledTaskFuncT(cancelTokenSource);
				var processor = new DefaultRetryProcessor();
				var polResult = await processor.RetryAsync(save, 1, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polResult.IsFailed);
				ClassicAssert.IsTrue(polResult.IsCanceled);
				ClassicAssert.AreEqual(0, polResult.Errors.Count());
				Assert.That(polResult.PolicyCanceledError, Is.Not.Null);
			}
		}

		private Func<CancellationTokenSource, Func<int>> CanceledGetAwaiterFunc => (cts) => () => {
			cts.Cancel();
			return Task.Run(() => 1, cts.Token).GetAwaiter().GetResult();
		};

		private Func<CancellationTokenSource, Action> CanceledGetAwaiterAct => (cts) => () => {
			cts.Cancel();
			Task.Run(() => Expression.Empty(), cts.Token).GetAwaiter().GetResult();
		};

		private Func<CancellationTokenSource, Func<int>> CancelWaiterFunc => (cts) => () => {
			cts.Cancel();
			return Task.Run(() => 1, cts.Token).Result;
		};

		private Func<CancellationTokenSource, Action> CancelWaiterAct => (cts) => () => {
			cts.Cancel();
			Task.Run(() => 1, cts.Token).Wait();
		};

		private Func<CancellationTokenSource, Func<CancellationToken, Task<int>>> CanceledTaskFuncT => (cts) => (_) => {
			cts.Cancel();
			return Task.Run(() => 1, cts.Token);
		};

		private Func<CancellationTokenSource, Func<CancellationToken, Task>> CanceledTaskFunc => (cts) => (_) => {
			cts.Cancel();
			return Task.Run(() => { }, cts.Token);
		};

		private readonly Func<CancellationTokenSource, Action<Exception, CancellationToken>> _actCancel = (cts) => (_, __) => cts.Cancel();
	}
}