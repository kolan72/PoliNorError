using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using NUnit.Framework.Legacy;

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
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);
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

			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);

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
	}
}