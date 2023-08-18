using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class BulkErrorProcessorTests
	{
		[Test]
		public async Task Should_ProcessAsync_Return_Status_None_When_No_Processors()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);
			var res =  await bulkProcessor.ProcessAsync(new Exception(), ProcessingErrorContext.FromRetry(1), CancellationToken.None);
			Assert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public async Task Should_ProcessAsync_Return_Status_ProcessorException_When_ProcessorWithError()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);

			var mockedErrorProcessor = new Mock<IErrorProcessor>();
			mockedErrorProcessor.Setup((t) => t.ProcessAsync(It.IsAny<Exception>(), It.IsAny<ProcessingErrorInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Throws(new Exception());

			bulkProcessor.AddProcessor(mockedErrorProcessor.Object);

			var res = await bulkProcessor.ProcessAsync(new Exception(), ProcessingErrorContext.FromRetry(1), It.IsAny<CancellationToken>());
			Assert.IsTrue(res.ProcessErrors.Count() == 1);
		}

		[Test]
		public async Task Should_ProcessAsync_Return_Status_Success_When_NoTimeUp_And_CorrectProcessor()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);

			var mockedErrorProcessor = new Mock<IErrorProcessor>();
			mockedErrorProcessor.Setup((t) => t.ProcessAsync(It.IsAny<Exception>(), It.IsAny<ProcessingErrorInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Exception()));

			bulkProcessor.AddProcessor(mockedErrorProcessor.Object);
			var res = await bulkProcessor.ProcessAsync(new Exception(), ProcessingErrorContext.FromRetry(1), It.IsAny<CancellationToken>());
			Assert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public void Should_Process_Return_Status_None_When_No_Processors()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);
			var res = bulkProcessor.Process(new Exception(), ProcessingErrorContext.FromRetry(1), CancellationToken.None);
			Assert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public void Should_Process_Return_Status_ProcessorException_When_ProcessorWithError()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);

			var mockedErrorProcessor = new Mock<IErrorProcessor>();
			mockedErrorProcessor.Setup((t) => t.Process(It.IsAny<Exception>(), It.IsAny<ProcessingErrorInfo>(), It.IsAny<CancellationToken>())).Throws(new Exception());

			bulkProcessor.AddProcessor(mockedErrorProcessor.Object);

			var res = bulkProcessor.Process(new Exception(), ProcessingErrorContext.FromRetry(1), It.IsAny<CancellationToken>());
			Assert.IsTrue(res.ProcessErrors.Count() == 1);
		}

		[Test]
		public void Should_Process_Return_Status_Success_When_NoTimeUp_And_CorrectProcessor()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);

			var mockedErrorProcessor = new Mock<IErrorProcessor>();
			mockedErrorProcessor.Setup((t) => t.Process(It.IsAny<Exception>(), It.IsAny<ProcessingErrorInfo>(), It.IsAny<CancellationToken>())).Returns(new Exception());

			bulkProcessor.AddProcessor(mockedErrorProcessor.Object);
			var res = bulkProcessor.Process(new Exception(), ProcessingErrorContext.FromRetry(1), It.IsAny<CancellationToken>());
			Assert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public void Should_Process_NotCallOtherProcessor_If_Canceled()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);
			cancelTokenSource.CancelAfter(500);
			var delayProcessor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
			bulkProcessor.AddProcessor(delayProcessor);
			bulkProcessor.AddProcessor(new BasicErrorProcessor());

			var res = bulkProcessor.Process(new Exception(), ProcessingErrorContext.FromRetry(1), cancelTokenSource.Token);
			Assert.IsTrue(res.ProcessErrors.Count() == 1);
			Assert.IsTrue(res.ProcessErrors.FirstOrDefault().InnerException?.GetType().Equals(typeof(OperationCanceledException)));
			Assert.IsTrue(res.IsCanceled);
		}

		[Test]
		public async Task Should_ProcessAsync_NotCallOtherProcessor_If_Canceled()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);
			cancelTokenSource.CancelAfter(500);
			var delayProcessor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
			bulkProcessor.AddProcessor(delayProcessor);
			bulkProcessor.AddProcessor(new BasicErrorProcessor());

			var res = await bulkProcessor.ProcessAsync(new Exception(), ProcessingErrorContext.FromRetry(1), cancelTokenSource.Token);
			Assert.IsTrue(res.ProcessErrors.Count() == 1);
			//				The real type here id TaskCanceledException.
			Assert.IsTrue(res.ProcessErrors.FirstOrDefault().InnerException?.GetType().BaseType.Equals(typeof(OperationCanceledException)));
			Assert.IsTrue(res.IsCanceled);
		}
	}
}