using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
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
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_IncludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = RetryProcessor.CreateDefault()
										  .IncludeError((exc) => exc.Message == paramName);
			void saveWithInclude() { throw new Exception(errorParamName); }
			var tryResCountWithNoInclude = processor.Retry(saveWithInclude, 1);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = RetryProcessor.CreateDefault();
			processor.IncludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), 1);
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
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_ExcludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = RetryProcessor.CreateDefault()
										  .ExcludeError((exc) => exc.Message == paramName);
			void saveWithInclude() { throw new Exception(errorParamName); }
			var tryResCountWithNoInclude = processor.Retry(saveWithInclude, 1);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = RetryProcessor.CreateDefault();
			processor.ExcludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), 1);
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
		[TestCase(false)]
		[TestCase(true)]
		public void Should_Retry_NotBreak_When_ErrorProcessing_Faulted(bool isGeneric)
		{
			var throwingExc = new ApplicationException();
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Retry);
			bulkProcessor.AddProcessor(new BasicErrorProcessor((_, __) => throw new Exception("Test")));

			var processor = RetryProcessor.CreateDefault(bulkProcessor);
			PolicyResult tryResCount = null;
			const int plannedRetryCount = 3;

			if (isGeneric)
			{
				int save() { throw throwingExc; }
				tryResCount = processor.Retry(save, plannedRetryCount);
			}
			else
			{
				void save() { throw throwingExc; }
				tryResCount = processor.Retry(save, plannedRetryCount);
			}

			Assert.AreEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			Assert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_Retry_Break_When_ErrorProcessing_Canceled(bool isGeneric)
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
				int save() { throw throwingExc; }
				tryResCount = processor.Retry(save, plannedRetryCount, cancelSource.Token);
			}
			else
			{
				void save() { throw throwingExc; }
				tryResCount = processor.Retry(save, plannedRetryCount, cancelSource.Token);
			}

			Assert.AreEqual(true, tryResCount.IsFailed);
			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreNotEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			Assert.AreEqual(1, tryResCount.Errors.Count());
			cancelSource.Dispose();
		}

		[Test]
		public void Should_UseCustomErrorSaver_Work_When_No_Save_Error()
		{
			var processor = RetryProcessor.CreateDefault().UseCustomErrorSaver(new BasicErrorProcessor((_, __) => { }));
			int i = 0;
			var res = processor.Retry(() => { i++; throw new Exception("Test"); }, 2);
			Assert.AreEqual(3, i);
			Assert.AreEqual(true, res.ErrorsNotUsed);
			Assert.AreEqual(0, res.Errors.Count());
			Assert.IsNotNull(res.UnprocessedError);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_UseCustomErrorSaverOf_Work_When_SetupBySyncAndAsyncDelegates(bool notSync)
		{
			int asyncM = 0;
			int syncM = 0;
			var processor = RetryProcessor.CreateDefault().UseCustomErrorSaverOf(async(_, __) => { await Task.Delay(1) ; asyncM++; }, (_) => syncM++);
			int i = 0;
			PolicyResult res = null;
			if (notSync)
			{
				res = await processor.RetryAsync((_) => { i++; throw new Exception("Test"); }, 2);
				Assert.AreEqual(3, asyncM);
			}
			else
			{
				res = processor.Retry(() => { i++; throw new Exception("Test"); }, 2);
				Assert.AreEqual(3, syncM);
			}
			Assert.AreEqual(3, i);
			Assert.AreEqual(true, res.ErrorsNotUsed);
			Assert.AreEqual(0, res.Errors.Count());
			Assert.IsNotNull(res.UnprocessedError);
		}

		[Test]
		public void Should_UseCustomErrorSaver_Work_When_Has_Save_Error()
		{
			var processor = RetryProcessor.CreateDefault().UseCustomErrorSaver(new BasicErrorProcessor((_, __) => throw new Exception("Saver exception")));
			int i = 0;
			var res = processor.Retry(() => { i++; throw new Exception("Test"); }, 2);
			Assert.AreEqual(3, i);
			Assert.AreEqual(true, res.ErrorsNotUsed);
			Assert.AreEqual(0, res.Errors.Count());
			Assert.IsNotNull(res.UnprocessedError);
			Assert.AreEqual(3, res.CatchBlockErrors.Count());
			Assert.IsTrue(res.CatchBlockErrors.All(ce => ce.ExceptionSource == CatchBlockExceptionSource.ErrorSaver));
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

		[Test]
		public void Should_Retry_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = proc.Retry(null,1);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_RetryT_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = proc.Retry<int>(null, 1);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_RetryAsync_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = await proc.RetryAsync(null, 1);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_RetryTAsync_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = await proc.RetryAsync<int>(null, 1);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}
	}
}