﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static PoliNorError.Tests.DelayTimeErrorProcessorTests;
using NUnit.Framework.Legacy;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

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

			ClassicAssert.AreEqual(resErrorCount, tryResCount.Errors.Count());
		}

		[Test]
		public void Should_Retry_Break_And_BeFailedCanceled_WhenDelegateWithErrorAndCanceled()
		{
			var cancelTokenSource = new CancellationTokenSource();
			void save() { cancelTokenSource.Cancel(); throw new Exception(); }
			var processor = new DefaultRetryProcessor();
			var polRetryResult =  processor.Retry(save, 6, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polRetryResult.IsFailed);
			ClassicAssert.IsTrue(polRetryResult.IsCanceled);
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
			ClassicAssert.IsTrue(tryResCount.IsCanceled);
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

			ClassicAssert.AreEqual(retryCount+1, tryResCount.Errors.Count());
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
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
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
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_With_TwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = RetryProcessor.CreateDefault();
			processor.IncludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), 1);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = RetryProcessor.CreateDefault();
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			processor.IncludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), 1);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false, true)]
		[TestCase(TestErrorSetMatch.NoMatch, true, false)]
		[TestCase(TestErrorSetMatch.FirstParam, false, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false, false)]
		public void Should_IncludeErrorSet_With_IErrorSetParam_ForInnerExceptions_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, bool consistsOfErrorAndInnerError)
		{
			var processor = RetryProcessor.CreateDefault();
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			processor.IncludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, null, true), 1);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
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
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
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
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_With_TwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = RetryProcessor.CreateDefault();
			processor.ExcludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), 1);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = RetryProcessor.CreateDefault();
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			processor.ExcludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), 1);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true, true)]
		[TestCase(TestErrorSetMatch.NoMatch, false, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true, false)]
		[TestCase(TestErrorSetMatch.SecondParam, true, false)]
		public void Should_ExcludeErrorSet_With_IErrorSetParam_ForInnerExceptions_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, bool consistsOfErrorAndInnerError)
		{
			var processor = RetryProcessor.CreateDefault();
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			processor.ExcludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Retry(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, null, true), 1);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", "")]
		public void Should_GenericIncludeFilterWork(string include, string notInclude)
		{
			var processor = new DefaultRetryProcessor();

			processor.IncludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			ClassicAssert.AreEqual(1, processor.IncludedErrorFilters.Count());

			void saveWithInclude() { throw new ArgumentNullException(include); }
			var tryResCount = processor.Retry(saveWithInclude, 2);

			ClassicAssert.AreEqual(3, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(false, tryResCount.ErrorFilterUnsatisfied);

			void saveWithNoInclude() { throw new ArgumentNullException(notInclude); }
			var tryResCountWithNoInclude = processor.Retry(saveWithNoInclude, 2);

			ClassicAssert.AreEqual(1, tryResCountWithNoInclude.Errors.Count());
			ClassicAssert.AreEqual(true, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(null, "Test")]
		public void Should_GenericExcludeFilterWork(string notExclude, string exclude)
		{
			var argNull = new ArgumentNullException(notExclude);

			var processor = new DefaultRetryProcessor();

			processor.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");

			ClassicAssert.AreEqual(1, processor.ExcludedErrorFilters.Count());

			void saveWithExclude() { throw new ArgumentNullException(exclude); }
			var tryResCount = processor.Retry(saveWithExclude, 2);

			ClassicAssert.AreEqual(true, tryResCount.ErrorFilterUnsatisfied);

			void saveWithExclude2() => throw argNull;
			var tryResCount2 = processor.Retry(saveWithExclude2, 2);
			ClassicAssert.AreEqual(3, tryResCount2.Errors.Count());
			ClassicAssert.AreEqual(false, tryResCount2.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_Retry_Throw_When_Task_IsReturnType()
		{
			var throwingExc = new ApplicationException();
			async Task save() { await Task.Delay(1); throw throwingExc; }
			var processor = new DefaultRetryProcessor();
			ClassicAssert.Throws<ArgumentException>(() => processor.Retry(save, 1, CancellationToken.None));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_Retry_NotBreak_When_ErrorProcessing_Faulted(bool isGeneric)
		{
			var throwingExc = new ApplicationException();
			var bulkProcessor = new BulkErrorProcessor();
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

			ClassicAssert.AreEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_Retry_Break_When_ErrorProcessing_Canceled(bool isGeneric)
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
				int save() { throw throwingExc; }
				tryResCount = processor.Retry(save, plannedRetryCount, cancelSource.Token);
			}
			else
			{
				void save() { throw throwingExc; }
				tryResCount = processor.Retry(save, plannedRetryCount, cancelSource.Token);
			}

			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
			ClassicAssert.AreEqual(true, tryResCount.IsCanceled);
			ClassicAssert.AreNotEqual(plannedRetryCount + 1, tryResCount.Errors.Count());
			ClassicAssert.AreEqual(1, tryResCount.Errors.Count());
			cancelSource.Dispose();
		}

		[Test]
		public void Should_UseCustomErrorSaver_Work_When_No_Save_Error()
		{
			var processor = RetryProcessor.CreateDefault().UseCustomErrorSaver(new BasicErrorProcessor((_, __) => { }));
			int i = 0;
			var res = processor.Retry(() => { i++; throw new Exception("Test"); }, 2);
			ClassicAssert.AreEqual(3, i);
			ClassicAssert.AreEqual(true, res.ErrorsNotUsed);
			ClassicAssert.AreEqual(0, res.Errors.Count());
			ClassicAssert.IsNotNull(res.UnprocessedError);
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
				ClassicAssert.AreEqual(3, asyncM);
			}
			else
			{
				res = processor.Retry(() => { i++; throw new Exception("Test"); }, 2);
				ClassicAssert.AreEqual(3, syncM);
			}
			ClassicAssert.AreEqual(3, i);
			ClassicAssert.AreEqual(true, res.ErrorsNotUsed);
			ClassicAssert.AreEqual(0, res.Errors.Count());
			ClassicAssert.IsNotNull(res.UnprocessedError);
		}

		[Test]
		public void Should_UseCustomErrorSaver_Work_When_Has_Save_Error()
		{
			var processor = RetryProcessor.CreateDefault().UseCustomErrorSaver(new BasicErrorProcessor((_, __) => throw new Exception("Saver exception")));
			int i = 0;
			var res = processor.Retry(() => { i++; throw new Exception("Test"); }, 2);
			ClassicAssert.AreEqual(3, i);
			ClassicAssert.AreEqual(true, res.ErrorsNotUsed);
			ClassicAssert.AreEqual(0, res.Errors.Count());
			ClassicAssert.IsNotNull(res.UnprocessedError);
			ClassicAssert.AreEqual(3, res.CatchBlockErrors.Count());
			ClassicAssert.IsTrue(res.CatchBlockErrors.All(ce => ce.ExceptionSource == CatchBlockExceptionSource.ErrorSaver));
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
			ClassicAssert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
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
			ClassicAssert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
			ClassicAssert.AreEqual(1, customDelayErrorProcessor.CurRetry);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_IncludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var processor = RetryProcessor
							.CreateDefault();
			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				processor = processor.IncludeInnerError<TestInnerException>();
			}
			else
			{
				processor = processor.IncludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			PolicyResult result;
			if (withInnerError)
			{
				if (satisfyFilterFunc == true)
				{
					result = processor.Retry(((Action<string>)ActionWithInnerWithMsg).Apply("Test"), 1);
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
				else if (satisfyFilterFunc == false)
				{
					result = processor.Retry(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"), 1);
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
				else
				{
					result = processor.Retry(ActionWithInner, 1);
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
			}
			else
			{
				result = processor.Retry(Action, 1);
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_ExcludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var processor = RetryProcessor
							.CreateDefault();
			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				processor = processor.ExcludeInnerError<TestInnerException>();
			}
			else
			{
				processor = processor.ExcludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			PolicyResult result;
			if (withInnerError)
			{
				if (satisfyFilterFunc == true)
				{
					result = processor.Retry(((Action<string>)ActionWithInnerWithMsg).Apply("Test"), 1);
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
				else if (satisfyFilterFunc == false)
				{
					result = processor.Retry(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"), 1);
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
				else
				{
					result = processor.Retry(ActionWithInner, 1);
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
			}
			else
			{
				result = processor.Retry(Action, 1);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public void Should_Retry_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = proc.Retry(null,1);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_RetryT_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = proc.Retry<int>(null, 1);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_RetryAsync_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = await proc.RetryAsync(null, 1);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_RetryTAsync_Null_Delegate_Work()
		{
			var proc = RetryProcessor.CreateDefault();
			var retryResult = await proc.RetryAsync<int>(null, 1);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_Backoff_Occurs_When_Retry_Method_Has_RetryDelay_Param()
		{
			var delayProvider = new FakeDelayProvider();
			var defProcessor = new DefaultRetryProcessor(delayProvider);
			defProcessor.Retry(() => throw new Exception("Test"), 2, new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(2));
		}

		[Test]
		public void Should_Backoff_Occurs_When_RetryT_Method_Has_RetryDelay_Param()
		{
			var delayProvider = new FakeDelayProvider();
			var defProcessor = new DefaultRetryProcessor(delayProvider);
			defProcessor.Retry<int>(() => throw new Exception("Test"), 2, new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(2));
		}

		[Test]
		public void Should_Backoff_Occurs_When_RetryInfinite_Method_Has_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var defProcessor = new DefaultRetryProcessor(delayProvider);
				defProcessor.RetryInfinite(() => throw new Exception("Test"), new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)), source.Token);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		public void Should_Backoff_Occurs_When_RetryInfiniteT_Method_Has_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var defProcessor = new DefaultRetryProcessor(delayProvider);
				defProcessor.RetryInfinite<int>(() => throw new Exception("Test"), new ConstantRetryDelay(TimeSpan.FromMilliseconds(1)), source.Token);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_IsZeroTry_True_Only_Before_Increment(bool sync)
		{
			var rc = new RetryContext(10);
			Assert.That(rc.IsZeroRetry, Is.True);
			if (sync)
			{
				rc.IncrementCount();
			}
			else
			{
				rc.IncrementCountAtomic();
			}
			Assert.That(rc.IsZeroRetry, Is.False);
		}
	}
}