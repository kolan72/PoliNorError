using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static PoliNorError.BulkErrorProcessor;
using static PoliNorError.Tests.DelayTimeErrorProcessorTests;

namespace PoliNorError.Tests
{
	internal class DefaultRetryProcessorTests
	{
		[Test]
		[TestCase(0, 2)]
		[TestCase(1, 2)]
		public void Should_Retry_WhenZeroOrOneRetry(int retryCount, int resErrorCount)
		{
			void save() => throw new Exception();
			var processor = new DefaultRetryProcessor();

			var tryResCount = processor.Retry(save, retryCount);

			Assert.AreEqual(resErrorCount, tryResCount.Errors.Count());
		}

		[Test]
		public void Should_Retry_Break_And_BeFailedCanceled_WhenDelegateWithErrorAndCanceled()
		{
			var cancelTokenSource = new CancellationTokenSource();
			void save() { cancelTokenSource.Cancel(); throw new Exception(); }
			var processor = new DefaultRetryProcessor();
			var polRetryResult =  processor.Retry(save, 6, cancelTokenSource.Token);
			Assert.IsTrue(polRetryResult.IsFailed);
			Assert.IsTrue(polRetryResult.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_RetryInfinite_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();

			void save() { throw new ApplicationException(); }
			cancelTokenSource.CancelAfter(1000);

			var processor = new DefaultRetryProcessor();

			var tryResCount = processor.RetryInfinite(save, cancelTokenSource.Token);
			Assert.IsTrue(tryResCount.IsCanceled);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_ErrorCount_Equals_RetryCountPlusOne_WhenError()
		{
			var cancelTokenSource = new CancellationTokenSource();

			void save() => throw new ApplicationException();
			const int retryCount = 2;

			var processor = new DefaultRetryProcessor();
			var tryResCount = processor.Retry(save, 2, cancelTokenSource.Token);

			Assert.AreEqual(retryCount+1, tryResCount.Errors.Count());
			cancelTokenSource.Dispose();
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_Generic_IncludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = new DefaultRetryProcessor();
			processor.IncludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() { throw new ArgumentNullException(errorParamName); }
			var tryResCountWithNoInclude = processor.Retry(saveWithInclude, 1);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_Generic_ExcludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = new DefaultRetryProcessor();
			processor.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() { throw new ArgumentNullException(errorParamName); }
			var tryResCountWithNoInclude = processor.Retry(saveWithInclude, 1);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", "")]
		public void Should_GenericIncludeFilterWork(string include, string notInclude)
		{
			var processor = new DefaultRetryProcessor();

			processor.IncludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			Assert.AreEqual(1, processor.IncludedErrorFilters.Count());

			void saveWithInclude() { throw new ArgumentNullException(include); }
			var tryResCount = processor.Retry(saveWithInclude, 2);

			Assert.AreEqual(3, tryResCount.Errors.Count());
			Assert.AreEqual(false, tryResCount.ErrorFilterUnsatisfied);

			void saveWithNoInclude() { throw new ArgumentNullException(notInclude); }
			var tryResCountWithNoInclude = processor.Retry(saveWithNoInclude, 2);

			Assert.AreEqual(1, tryResCountWithNoInclude.Errors.Count());
			Assert.AreEqual(true, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(null, "Test")]
		public void Should_GenericExcludeFilterWork(string notExclude, string exclude)
		{
			var argNull = new ArgumentNullException(notExclude);

			var processor = new DefaultRetryProcessor();

			processor.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");

			Assert.AreEqual(1, processor.ExcludedErrorFilters.Count());

			void saveWithExclude() { throw new ArgumentNullException(exclude); }
			var tryResCount = processor.Retry(saveWithExclude, 2);

			Assert.AreEqual(true, tryResCount.ErrorFilterUnsatisfied);

			void saveWithExclude2() => throw argNull;
			var tryResCount2 = processor.Retry(saveWithExclude2, 2);
			Assert.AreEqual(3, tryResCount2.Errors.Count());
			Assert.AreEqual(false, tryResCount2.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_Retry_Throw_When_Task_IsReturnType()
		{
			var throwingExc = new ApplicationException();
			async Task save() { await Task.Delay(1); throw throwingExc; }
			var processor = new DefaultRetryProcessor();
			Assert.Throws<ArgumentException>(() => processor.Retry(save, 1, CancellationToken.None));
		}

		[Test]
		public void Should_Retry_NotBreak_When_ErrorProcessing_Faulted()
		{
			var throwingExc = new ApplicationException();
			void save() { throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.Process(It.IsAny<CatchBlockProcessErrorInfo>(), throwingExc, It.IsAny<CancellationToken>())).Returns(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Faulted) }));

			var processor = new DefaultRetryProcessor(mockedBulkProcessor.Object);

			const int plannedRetryCount = 1;
			var tryResCount = processor.Retry(save, plannedRetryCount, It.IsAny<CancellationToken>());

			Assert.AreEqual(plannedRetryCount+1, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public void Should_Break_When_ErrorProcessing_Canceled()
		{
			var throwingExc = new ApplicationException();
			void save() { throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.Process(It.IsAny<CatchBlockProcessErrorInfo>(), throwingExc, It.IsAny<CancellationToken>())).Returns(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Canceled) }));

			var processor = RetryProcessor
									.CreateDefault(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = processor.Retry(save, plannedRetryCount, It.IsAny<CancellationToken>());

			Assert.AreEqual(1, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
			Assert.AreEqual(true, tryResCount.IsCanceled);
		}

		[Test]
		public void Should_RetryT_NotBreak_When_ErrorProcessing_Faulted()
		{
			var throwingExc = new ApplicationException();
			int save() { throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.Process(It.IsAny<CatchBlockProcessErrorInfo>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>())).Returns(
					new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Faulted) })
				);

			var processor = new DefaultRetryProcessor(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = processor.Retry(save, plannedRetryCount, CancellationToken.None);

			Assert.AreEqual(plannedRetryCount+1, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public void Should_RetryT_Break_When_ErrorProcessing_Canceled()
		{
			var throwingExc = new ApplicationException();
			int save() => throw throwingExc;
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.Process(It.IsAny<CatchBlockProcessErrorInfo>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>())).Returns(
					new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Canceled) })
				);

			var processor = new DefaultRetryProcessor(mockedBulkProcessor.Object);

			const int plannedRetryCount = 3;
			var tryResCount = processor.Retry(save, plannedRetryCount);

			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		public void Should_Retry_NotCallErrorProcess_When_ErrorSavingFailed()
		{
			var processor = RetryProcessor.CreateDefault((pr, _) => pr.SetFailedInner());
			var i = 0;
			processor.WithErrorProcessorOf((_) => i++);
			processor.Retry(() => throw new Exception("Test"), 2);
			Assert.AreEqual(0, i);
		}

		[Test]
		public void Should_RetryT_NotCallErrorProcess_When_ErrorSavingFailed()
		{
			var processor = RetryProcessor.CreateDefault((pr, _) => pr.SetFailedInner());
			int i = 0;
			processor.WithErrorProcessorOf((_) => i++);
			processor.Retry<int>(() => throw new Exception("Test"), 2);
			Assert.AreEqual(0, i);
		}

		[Test]
		public void Should_WithWait_Delay_Work()
		{
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			RetryProcessor
									.CreateDefault()
									.WithWait(TimeSpan.FromMilliseconds(300))
									.Retry(act, 1);
			sw.Stop();
			Assert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public async Task Should_WithWait_WithCustomDelayErrorProcessor_Work()
		{
			var customDelayErrorProcessor = new YourDelayErrorProcessor(TimeSpan.FromMilliseconds(300));
			async Task funcAsync(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var sw = Stopwatch.StartNew();
			await RetryProcessor
									.CreateDefault()
									.WithWait(customDelayErrorProcessor)
									.RetryAsync(funcAsync, 2);
			sw.Stop();
			Assert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
			Assert.AreEqual(1, customDelayErrorProcessor.CurRetry);
		}
	}
}