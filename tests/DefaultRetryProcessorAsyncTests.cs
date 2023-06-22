using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static PoliNorError.BulkErrorProcessor;

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

			Assert.IsTrue(tryResCount.IsCanceled);
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

			Assert.AreEqual(true, tryResCount.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_RetryAsync_NotBreak_When_ErrorProcessing_Faulted()
		{
			var throwingExc = new ApplicationException();
			async Task save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.ProcessAsync(It.IsAny<CatchBlockProcessErrorInfo>(), throwingExc, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
				new BulkProcessResult(throwingExc,new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Faulted)})));

			var processor = RetryProcessor.CreateDefault(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = await processor.RetryAsync(save, plannedRetryCount,It.IsAny<CancellationToken>());

			Assert.AreEqual(plannedRetryCount + 1,  tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryAsync_Break_When_ErrorProcessing_Canceled()
		{
			var throwingExc = new ApplicationException();
			async Task save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.ProcessAsync(It.IsAny<CatchBlockProcessErrorInfo>(), throwingExc, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Canceled) })));

			var processor = new DefaultRetryProcessor(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = await processor.RetryAsync(save, plannedRetryCount);

			Assert.AreEqual(true, tryResCount.IsFailed);
			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreNotEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			Assert.AreEqual(1, tryResCount.Errors.Count());
		}

		[Test]
		public async Task Should_RetryAsyncT_NotBreak_When_ErrorProcessing_Faulted()
		{
			var throwingExc = new ApplicationException();
			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.ProcessAsync(It.IsAny<CatchBlockProcessErrorInfo>(), It.IsAny<Exception>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Faulted) })));

			var processor = new DefaultRetryProcessor(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = await processor.RetryAsync(save, plannedRetryCount, default);

			Assert.AreEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryAsyncT_Break_When_ErrorProcessing_Canceled()
		{
			var throwingExc = new ApplicationException();
			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.ProcessAsync(It.IsAny<CatchBlockProcessErrorInfo>(), It.IsAny<Exception>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Canceled) })));

			var processor = new DefaultRetryProcessor(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = await processor.RetryAsync(save, plannedRetryCount) ;

			Assert.AreEqual(true, tryResCount.IsFailed);
			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreNotEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			Assert.AreEqual(1, tryResCount.Errors.Count());
		}

		[Test]
		public async Task Should_RetryAsyncT_ErrorsCountEqualsToRetryCountPlusOneWhenError()
		{
			async Task<int> save(CancellationToken _){await Task.Delay(1); throw new ApplicationException();}

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryAsync(save, 2);

			Assert.AreEqual(3, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryInfiniteAsync_PolicyResultHasNoError()
		{
			async Task save(CancellationToken _) => await Task.Delay(1);

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryInfiniteAsync(save);

			Assert.AreEqual(0, tryResCount.Errors.Count());
			Assert.AreEqual(false, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryAsync_BeExactlyRetryCountWhenError()
		{
			async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryAsync(save, 2);

			Assert.AreEqual(3, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryInfiniteAsyncT_PolicyResultHasNoError()
		{
			async Task<int> save(CancellationToken _) { await Task.Delay(1); return 34; }

			var processor = new DefaultRetryProcessor();
			var tryResCount = await processor.RetryInfiniteAsync(save);

			Assert.AreEqual(0, tryResCount.Errors.Count());
			Assert.AreEqual(34, tryResCount.Result);
			Assert.AreEqual(false, tryResCount.IsFailed);
		}

		[Test]
		public async Task Should_RetryAsync_ShouldNotCallErrorProcess_When_ErrorSavingFailed()
		{
			var processor = RetryProcessor.CreateDefault((pr, _) => pr.SetFailedInner());
			int i = 0;
			processor.WithErrorProcessorOf((_) => i++);
			await processor.RetryAsync(async(_) => { await Task.Delay(1); throw new Exception("Test");}, 2);
			Assert.AreEqual(0, i);
		}

		[Test]
		public async Task Should_RetryAsyncT_ShouldNotCallErrorProcess_When_ErrorSavingFailed()
		{
			var processor = RetryProcessor.CreateDefault((pr, _) => pr.SetFailedInner());
			int i = 0;
			processor.WithErrorProcessorOf((_) => i++);
			await processor.RetryAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test"); }, 2);
			Assert.AreEqual(0, i);
		}
	}
}