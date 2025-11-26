using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using NUnit.Framework.Legacy;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class DefaultRetryProcessorAsyncTests
	{
		[Test]
		public async Task Should_RetryInfiniteAsync_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();

			async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }

			cancelTokenSource.CancelAfter(1500);

			var processor = new DefaultRetryProcessor();

			var tryResCount = await processor.RetryInfiniteAsync(save, cancelTokenSource.Token);

			ClassicAssert.IsTrue(tryResCount.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_RetryAsyncT_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();

			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }

			cancelTokenSource.CancelAfter(500);

			var processor = new DefaultRetryProcessor();

			var tryResCount = await processor.RetryInfiniteAsync(save, cancelTokenSource.Token);

			ClassicAssert.AreEqual(true, tryResCount.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public async Task Should_RetryAsync_NotBreak_When_ErrorProcessing_Faulted(bool isGeneric)
		{
			var throwingExc = new ApplicationException();
			var bulkProcessor = new BulkErrorProcessor();
			bulkProcessor.AddProcessor(new BasicErrorProcessor((_, __) => throw new Exception("Test")));

			var processor = RetryProcessor.CreateDefault(bulkProcessor);
			PolicyResult tryResCount = null;
			const int plannedRetryCount = 3;

			if (isGeneric)
			{
				async Task<int> save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
				tryResCount = await processor.RetryAsync(save, plannedRetryCount, default);
			}
			else
			{
				async Task save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
				tryResCount = await processor.RetryAsync(save, plannedRetryCount, default);
			}

			ClassicAssert.AreEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public async Task Should_RetryAsync_Break_When_ErrorProcessing_Canceled(bool isGeneric)
		{
			var cancelSource = new CancellationTokenSource();

			var bulkProcessor = new BulkErrorProcessor();

			bulkProcessor.AddProcessor(new BasicErrorProcessor((_, __) => cancelSource.Cancel()));
			bulkProcessor.AddProcessor(new BasicErrorProcessor((_, __) => { }));

			var processor = new DefaultRetryProcessor(bulkProcessor);

			var throwingExc = new ApplicationException();

			PolicyResult tryResCount = null;
			const int plannedRetryCount = 3;

			if (isGeneric)
			{
				async Task<int> save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
				tryResCount = await processor.RetryAsync(save, plannedRetryCount, cancelSource.Token);
			}
			else
			{
				async Task save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
				tryResCount = await processor.RetryAsync(save, plannedRetryCount, cancelSource.Token);
			}

			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
			ClassicAssert.AreEqual(true, tryResCount.IsCanceled);
			ClassicAssert.AreNotEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(1, tryResCount.Errors.Count());
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_RetryAsyncT_ErrorsCountEqualsToRetryCountPlusOneWhenError()
		{
			async Task<int> save(CancellationToken _){await Task.Delay(1); throw new ApplicationException();}

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryAsync(save, 2);

			ClassicAssert.AreEqual(3, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryInfiniteAsync_PolicyResultHasNoError()
		{
			async Task save(CancellationToken _) => await Task.Delay(1);

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryInfiniteAsync(save);

			ClassicAssert.AreEqual(0, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(false, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryAsync_BeExactlyRetryCountWhenError()
		{
			async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryAsync(save, 2);

			ClassicAssert.AreEqual(3, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryInfiniteAsyncT_PolicyResultHasNoError()
		{
			async Task<int> save(CancellationToken _) { await Task.Delay(1); return 34; }

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryInfiniteAsync(save);

			ClassicAssert.AreEqual(0, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(34, tryResCount.Result);
			ClassicAssert.AreEqual(false, tryResCount.IsFailed);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(false, true)]
		public async Task Should_WithInnerErrorProcessor_HandleError_Correctly(bool sync, bool withCancellationType)
		{
			var processor = RetryProcessor.CreateDefault();
			var innerProcessors = new InnerErrorProcessorFuncs();

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action);
				}

				processor.Retry(ActionWithInner, 1);
				processor.Retry(Action, 1);
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc);
				}

				await processor.RetryAsync(AsyncFuncWithInner, 1);
				await processor.RetryAsync(AsyncFunc, 1);
			}

			Assert.That(innerProcessors.I, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken);
				processor.Retry(ActionWithInner, 1);
				processor.Retry(Action, 1);
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken);
				await processor.RetryAsync(AsyncFuncWithInner, 1);
				await processor.RetryAsync(AsyncFunc, 1);
			}

			Assert.That(innerProcessors.J, Is.EqualTo(1));

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo);
				}
				processor.Retry(ActionWithInner, 1);
				processor.Retry(Action, 1);
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo);
				}
				await processor.RetryAsync(AsyncFuncWithInner, 1);
				await processor.RetryAsync(AsyncFunc, 1);
			}

			Assert.That(innerProcessors.K, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken);
				processor.Retry(ActionWithInner, 1);
				processor.Retry(Action, 1);
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken);
				await processor.RetryAsync(AsyncFuncWithInner, 1);
				await processor.RetryAsync(AsyncFunc, 1);
			}

			Assert.That(innerProcessors.L, Is.EqualTo(1));
		}

		[Test]
		public async Task Should_Backoff_Occurs_When_RetryAsync_Method_Has_RetryDelay_Param()
		{
			var delayProvider = new FakeDelayProvider();
			var defProcessor = new DefaultRetryProcessor(delayProvider);
			await defProcessor.RetryAsync(async(_) => {await Task.Delay(1); throw new Exception("Test"); }, 2, new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(2));
		}

		[Test]
		public async Task Should_Backoff_Occurs_When_RetryAsyncT_Method_Has_RetryDelay_Param()
		{
			var delayProvider = new FakeDelayProvider();
			var defProcessor = new DefaultRetryProcessor(delayProvider);
			await defProcessor.RetryAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test"); }, 2, new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(2));
		}

		[Test]
		public async Task Should_Backoff_Occurs_When_RetryInfiniteAsync_Method_Has_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var defProcessor = new DefaultRetryProcessor(delayProvider);
				await defProcessor.RetryInfiniteAsync((_) => throw new Exception("Test"), new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)), false, source.Token);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task Should_Backoff_Occurs_When_RetryInfiniteAsyncT_Method_Has_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var defProcessor = new DefaultRetryProcessor(delayProvider);
				await defProcessor.RetryInfiniteAsync<int>((_) => throw new Exception("Test"), new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)), false, source.Token);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task Should_RetryAsync_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

				async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
				var processor = RetryProcessor.CreateDefault();
				var tryResCount = await processor.RetryAsync(save, 1, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);
			}
		}

		[Test]
		public async Task Should_RetryWithErrorContextAsync_With_RetryCountInfo_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				const int k = 0;
				Task f(CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => { }, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultRetryProcessor();
				var result = await processor.RetryWithErrorContextAsync(f, 1, new RetryCountInfo(), token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_RetryInfiniteWithErrorContextAsync_With_RetryCountInfo_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => { }, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultRetryProcessor();
				var result = await processor.RetryInfiniteWithErrorContextAsync(f, 1, token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_RetryWithErrorContextAsync_With_RetryCountInfo_With_RetryDelay_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => { }, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultRetryProcessor();
				var result = await processor.RetryWithErrorContextAsync(f, 1, new RetryCountInfo(), ConstantRetryDelay.Create(TimeSpan.FromMilliseconds(1)), token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_RetryInfiniteWithErrorContextAsync_With_RetryCountInfo_With_RetryDelay_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => { }, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultRetryProcessor();
				var result = await processor.RetryInfiniteWithErrorContextAsync(f, 1, ConstantRetryDelay.Create(TimeSpan.FromMilliseconds(1)), token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_RetryAsync_With_RetryCountInfo_With_TParam_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(int i, CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => k = i, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultRetryProcessor();
				var result = await processor.RetryAsync(f, 1, new RetryCountInfo(), token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}
	}
}