using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics;
using NSubstitute;
using NUnit.Framework.Legacy;
using System.Collections.Generic;

namespace PoliNorError.Tests
{
	internal class RetryPolicyTests
	{
		[Test]
		public async Task Should_HandleAsync_WorkWithClosure()
		{
			var retry = new RetryPolicy(1);

			var testClass = new TestAsyncClass();
			Func<CancellationToken, Task> taskSave = testClass.SaveAsync;

			await retry.HandleAsync(taskSave);
			ClassicAssert.AreEqual(1, testClass.I);
		}

		[Test]
		public void Should_Handle_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			void action() => Expression.Empty();

			var res = retry.Handle(action);
			ClassicAssert.IsFalse(res.IsFailed);
			ClassicAssert.IsFalse(res.IsCanceled);
			ClassicAssert.IsFalse(res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			// Method intentionally left empty.
		}

		[Test]
		public async Task Should_HandleAsync_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			async Task action(CancellationToken _) { await Task.Delay(100); }

			var res = await retry.HandleAsync(action);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(false, res.IsCanceled);
			ClassicAssert.AreEqual(false, res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(retry.PolicyName, res.PolicyName);
		}

		[Test]
		public void Should_HandleT_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			int action() => 4;

			var res = retry.Handle(action);

			ClassicAssert.AreEqual(4, res.Result);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(false, res.IsCanceled);
			ClassicAssert.AreEqual(false, res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(retry.PolicyName, res.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			async Task<int> action(CancellationToken _) { await Task.Delay(100); return 4; }

			var res = await retry.HandleAsync(action);
			ClassicAssert.AreEqual(4, res.Result);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(false, res.IsCanceled);
			ClassicAssert.AreEqual(false, res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(retry.PolicyName, res.PolicyName);
		}

		[Test]
		public void Should_Handle_CallRetryProcessor_If_No_Wrap()
		{
			var rci = new RetryCountInfo(2, null, 0);
			var subsProcessor = Substitute.For<IRetryProcessor>();
			subsProcessor.Retry(Arg.Any<Action>(), rci).Returns(new PolicyResult());

			var policy = new RetryPolicy(subsProcessor, rci);
			policy.Handle(() => { }, default);

			subsProcessor.Received(1).Retry(Arg.Any<Action>(), rci);
		}

		[Test]
		public void Should_Be_Cancelable_ForSyncAction_WithError()
		{
			var retryPol = RetryPolicy.InfiniteRetries().WithWait(TimeSpan.FromSeconds(1));
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

				void act() => throw new Exception();
				var polResult = retryPol.Handle(act, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polResult.IsCanceled);
				ClassicAssert.AreEqual(retryPol.PolicyName, polResult.PolicyName);
			}
		}

		[Test]
		public void Should_Be_Cancelable_ForSyncAction_WithDelay()
		{
			var retryPol = RetryPolicy.InfiniteRetries().WithWait(TimeSpan.FromSeconds(1));
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

				void act() => Task.Delay(2000, cancelTokenSource.Token).GetAwaiter().GetResult();
				var polResult = retryPol.Handle(act, cancelTokenSource.Token);
				ClassicAssert.IsTrue(polResult.IsCanceled);
				ClassicAssert.IsTrue(polResult.IsFailed);
			}
		}

		[Test]
		public void Should_WithDelayProcessorOf_Func_Work()
		{
			var pol = new RetryPolicy(1).WithWait((_, __) => TimeSpan.FromMilliseconds(300));
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			pol.Handle(act);
			sw.Stop();
			ClassicAssert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public void Should_WithDelayProcessorOf_Delay_Work()
		{
			var pol = new RetryPolicy(1).WithWait(TimeSpan.FromMilliseconds(300));
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			pol.Handle(act);
			sw.Stop();
			ClassicAssert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public async Task Should_Be_Cancelable_ForASyncFunc_WithError()
		{
			var retryPol = RetryPolicy.InfiniteRetries().WithWait(TimeSpan.FromSeconds(1));
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

			async Task func(CancellationToken _) { await Task.Delay(100); throw new Exception(); }

			var polResult = await retryPol.HandleAsync(func, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.IsFalse(polResult.IsSuccess);
			ClassicAssert.IsFalse(polResult.NoError);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_Be_Cancelable_AfterTime_ForNativeToken()
		{
			var retryPol = new RetryPolicy(1);
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50));

			async Task func(CancellationToken _) { await Task.Delay(110, cancelTokenSource.Token); }

			var polResult = await retryPol.HandleAsync(func, cancelTokenSource.Token);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.IsFailed);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_Be_Cancelable_AfterTime_ForThirdPartyToken()
		{
			var retryPol = new RetryPolicy(1).ExcludeError<TaskCanceledException>();

			var cancelTokenSourceThirdParty = new CancellationTokenSource();
			cancelTokenSourceThirdParty.CancelAfter(TimeSpan.FromMilliseconds(50));

			async Task func(CancellationToken _) { await Task.Delay(100, cancelTokenSourceThirdParty.Token); }

			var cancelTokenSourceNative = new CancellationTokenSource();
			var polResult = await retryPol.HandleAsync(func, cancelTokenSourceNative.Token);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.IsFailed);
			//Because we excluded TaskCanceledException type exception handling.
			ClassicAssert.AreEqual(1, polResult.Errors.Count());
			cancelTokenSourceThirdParty.Dispose();
		}

		[Test]
		public void Should_Work_ForZeroRetryCount_ForSyncAction_WithError()
		{
			var retryPol = new RetryPolicy(0);

			void act() => throw new Exception();
			var polResult = retryPol.Handle(act, default);
			ClassicAssert.AreEqual(2, polResult.Errors.Count());
			ClassicAssert.IsTrue(polResult.IsFailed);
		}

		[Test]
		public void Should_Work_For_Handle_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = retryPolTest.Handle(null);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(retryPolTest.PolicyName, retryResult.PolicyName);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_RetryPolicy_Wrap_OtherPolicy_And_Handle_NullDelegate()
		{
			var fallBackPolicy = new SimplePolicy();
			var retryPolicy = new RetryPolicy(1);
			retryPolicy.WrapPolicy(fallBackPolicy);
			var retryResult = retryPolicy.Handle(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(retryPolicy.PolicyName, retryResult.PolicyName);
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_RetryPolicy_Wrap_OtherPolicy_And_HandleAsync_NullDelegate()
		{
			var fallBackPolicy = new SimplePolicy();
			var retryPolicy = new RetryPolicy(1);
			retryPolicy.WrapPolicy(fallBackPolicy);
			var retryResult = await retryPolicy.HandleAsync(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(retryPolicy.PolicyName, retryResult.PolicyName);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_RetryPolicy_Wrap_OtherPolicy_And_HandleT_NullDelegate()
		{
			var fallBackPolicy = new SimplePolicy();
			var retryPolicy = new RetryPolicy(1);
			retryPolicy.WrapPolicy(fallBackPolicy);
			var retryResult = retryPolicy.Handle<int>(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_RetryPolicy_Wrap_OtherPolicy_And_HandleTAsync_NullDelegate()
		{
			var fallBackPolicy = new SimplePolicy();
			var retryPolicy = new RetryPolicy(1);
			retryPolicy.WrapPolicy(fallBackPolicy);
			var retryResult = await retryPolicy.HandleAsync<int>(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = await retryPolTest.HandleAsync(null);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(retryPolTest.PolicyName, retryResult.PolicyName);
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = retryPolTest.Handle<int>(null);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(retryPolTest.PolicyName, retryResult.PolicyName);
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = await retryPolTest.HandleAsync<int>(null);
			ClassicAssert.IsTrue(retryResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(retryPolTest.PolicyName, retryResult.PolicyName);
		}

		[Test]
		[TestCase("Test")]
		public void Should_Add_GenericIncludeErrorFilter(string errParamName)
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.IncludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");

			void actionUnsatisied() => throw new Exception("Test2");
			var polRes = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(polRes.IsFailed);
			ClassicAssert.AreEqual(1, polRes.Errors.Count());

			void actionSatisied() => throw new ArgumentNullException(errParamName);
			var polRes2 = retryPolTest.Handle(actionSatisied);
			ClassicAssert.IsFalse(polRes2.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(polRes2.IsFailed);
			ClassicAssert.AreEqual(2, polRes2.Errors.Count());
		}

		[Test]
		[TestCase("Test")]
		public void Should_GenericExcludeFilterWork(string errParamName)
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			void actionUnsatisied() => throw new ArgumentNullException(errParamName);
			var res = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.IsTrue(res.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2", "Test2", "Test", false)]
		[TestCase("Test", "Test", "Test", true)]
		[TestCase("Test2", "Test", "Test2", true)]
		[TestCase("Test2", "Test", "Test", true)]
		public void Should_IncludeAndExcludeFilter_Work(string paramName, string forErrorParamName, string excludeErrorParamName, bool res)
		{
			var retryPolTest = new RetryPolicy(1);
			void actionUnsatisied() => throw new ArgumentNullException(paramName);
			retryPolTest.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == excludeErrorParamName).IncludeError<ArgumentNullException>((ane) => ane.ParamName == forErrorParamName);
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.AreEqual(res, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", "Test", true, 1)]
		[TestCase("Test", "Test2", false, 2)]
		public void Should_ExcludeFilter_Filter_IncludeFiler(string excMessage, string excludeExcMessage, bool res, int errorsCount)
		{
			var retryPolTest = new RetryPolicy(1);
			void actionUnsatisied() => throw new ArgumentNullException(excMessage, new Exception());
			retryPolTest.IncludeError<ArgumentNullException>().ExcludeError<ArgumentNullException>((ane) => ane.Message == excludeExcMessage);
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.AreEqual(res, resHandle.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(resHandle.Errors.Count(), errorsCount);
		}

		[Test]
		[TestCase("Test", "Test", true)]
		[TestCase("Test", "Test2", false)]
		public void Should_IncludeTypeAndExcludeByErrorPropertyFilter_Work(string paramName, string excludeParamName, bool resUnsatisfied)
		{
			var retryPolTest = new RetryPolicy(1);
			void actionUnsatisied() => throw new ArgumentNullException(paramName);
			retryPolTest.IncludeError<ArgumentNullException>().ExcludeError<ArgumentNullException>((ane) => ane.ParamName == excludeParamName);
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.AreEqual(resUnsatisfied, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_IncludeTypeAndExcludeByErrorPropertyFilter_ForDiffErorType_Work()
		{
			var retryPolTest = new RetryPolicy(1);
			void actionUnsatisied() => throw new Exception("Test");
			retryPolTest.IncludeError<ArgumentNullException>().ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test2");
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.AreEqual(true, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_Add_IncludeErrorFilter()
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.IncludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test2");
			var polRes = retryPolTest.Handle(action);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(polRes.IsFailed);
			ClassicAssert.AreEqual(1, polRes.Errors.Count());
		}

		[Test]
		public void Should_Add_MoreThanOne_IncludeErrorFilter_Work()
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.IncludeError((e) => e.Message == "Test")
						.IncludeError((e) => e.Message == "Test2");
			int i = 0;
			void action()
			{
				if (i == 0)
				{
					i++;
					throw new Exception("Test2");
				}
				else
				{
					throw new Exception("Test");
				}
			}

			var polRes = retryPolTest.Handle(action);
			ClassicAssert.IsFalse(polRes.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(polRes.IsFailed);
			ClassicAssert.AreEqual(2, polRes.Errors.Count());
		}

		[Test]
		public void Should_Add_ExludedErrorFilter()
		{
			var retryPolTest = new RetryPolicy(2).ExcludeError((e) => e.Message == "Test");
			void act() => throw new Exception("Test");
			var resPol = retryPolTest.Handle(act);
			ClassicAssert.AreEqual(1, resPol.Errors.Count());
			ClassicAssert.IsTrue(resPol.ErrorFilterUnsatisfied);
			retryPolTest = retryPolTest.ExcludeError((e) => e.Message == "Test2");
			void act2() => throw new Exception("Test2");
			var resPol2 = retryPolTest.Handle(act2);
			ClassicAssert.AreEqual(1, resPol2.Errors.Count());
			ClassicAssert.IsTrue(resPol2.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.IncludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(retryPolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var retryPolTest = new RetryPolicy(1);
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = retryPolTest.IncludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(retryPolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false, true)]
		[TestCase(TestErrorSetMatch.NoMatch, true, false)]
		[TestCase(TestErrorSetMatch.FirstParam, false, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false, false)]
		public void Should_IncludeErrorSet_ForInnerExceptions_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed, bool consistsOfErrorAndInnerError)
		{
			var simplePolTest = new RetryPolicy(1);
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			_ = simplePolTest.IncludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(simplePolTest, testErrorSetMatch, true);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.ExcludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(retryPolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var simplePolTest = new RetryPolicy(1);
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = simplePolTest.ExcludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(simplePolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true, true)]
		[TestCase(TestErrorSetMatch.NoMatch, false, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true, false)]
		[TestCase(TestErrorSetMatch.SecondParam, true, false)]
		public void Should_ExcludeErrorSet_ForInnerExceptions_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed, bool consistsOfErrorAndInnerError)
		{
			var simplePolTest = new RetryPolicy(1);
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			_ = simplePolTest.ExcludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(simplePolTest, testErrorSetMatch, true);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_CatchBlockError_Handled_For_Handle()
		{
			void saveAsync() => throw new Exception();
			void errorProcessorFunc(Exception ex) => throw ex;
			var retryPolTest = new RetryPolicy(1).WithErrorProcessorOf(errorProcessorFunc).ToPolicyDelegate(saveAsync);
			var res = retryPolTest.Handle();
			ClassicAssert.IsTrue(res.Errors.Count() == 2);
			ClassicAssert.IsTrue(res.CatchBlockErrors.Count() == 1);
		}

		[Test]
		public void Should_NotCritical_CatchBlockError_NotAffect_IsFailed()
		{
			void errorProcessorFunc(Exception ex) => throw ex;
			var retryPolTest = new RetryPolicy(1);
			int i = 0;
			void action()
			{
				if (i == 0)
				{
					i++;
					throw new Exception("Test2");
				}
				else
				{
					i++;
				}
			}
			retryPolTest.WithErrorProcessorOf(errorProcessorFunc);
			var polRes = retryPolTest.Handle(action);
			ClassicAssert.IsFalse(polRes.IsFailed);
			ClassicAssert.IsTrue(polRes.CatchBlockErrors.Count() == 1);
			ClassicAssert.IsFalse(polRes.CatchBlockErrors.FirstOrDefault().IsCritical);
			ClassicAssert.IsTrue(polRes.Errors.Any());
			ClassicAssert.IsNull(polRes.UnprocessedError);
		}

		[Test]
		public void Should_WithPolicyName_Work()
		{
			var retryPol = new RetryPolicy(1);
			ClassicAssert.AreEqual(retryPol.GetType().Name, retryPol.PolicyName);

			const string polName = "RP";
			retryPol.WithPolicyName(polName);
			ClassicAssert.AreEqual(polName, retryPol.PolicyName);
		}

		[Test]
		public void Should_RetryInfo_Be_Correct_ForAnySetup()
		{
			var retryPol1 = new RetryPolicy(1);
			ClassicAssert.IsFalse(retryPol1.RetryInfo.IsInfinite);
			ClassicAssert.AreEqual(1, retryPol1.RetryInfo.RetryCount);

			var retryPol0 = new RetryPolicy(0);
			ClassicAssert.IsFalse(retryPol0.RetryInfo.IsInfinite);
			ClassicAssert.AreEqual(1, retryPol0.RetryInfo.RetryCount);

			var retryPolInfinite = RetryPolicy.InfiniteRetries();
			ClassicAssert.IsTrue(retryPolInfinite.RetryInfo.IsInfinite);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddPolicyResultHandler_By_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new RetryPolicy(1);
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler((_) => i++);
			}
			var res = policy.Handle(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddPolicyResultHandler_By_Generic_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new RetryPolicy(1);
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler<int>((_) => i++);
			}
			var res = policy.Handle<int>(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandler_By_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new RetryPolicy(1);
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });
			}
			var res = await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandler_By_Generic_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new RetryPolicy(1);
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler<int>(async (_) => { await Task.Delay(1); i++; });
			}
			var res = await policy.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_SetPolicyResultFailedIf_Work(bool generic)
		{
			var policy = new RetryPolicy(1);
			PolicyResult polResult = null;
			bool beforeRetry = true;
			if (generic)
			{
				polResult = policy.SetPolicyResultFailedIf<int>(pr => pr.Errors.Any(e => e.Message == "Test"))
					.Handle(() =>
					{
						if (beforeRetry)
						{
							beforeRetry = false;
							throw new ArgumentException("Test");
						}
						return 1;
					});
			}
			else
			{
				polResult = policy.SetPolicyResultFailedIf(pr => pr.Errors.Any(e => e.Message == "Test"))
					.Handle(() =>
					{
						if (beforeRetry)
						{
							beforeRetry = false;
							throw new ArgumentException("Test");
						}
					});
			}
			Assert.That(polResult.IsFailed, Is.EqualTo(true));
			Assert.That(polResult.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyResultHandlerFailed));
			Assert.That(polResult.FailedHandlerIndex, Is.EqualTo(0));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_IncludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var policy = new RetryPolicy(1);
			TestHandlingForInnerError.IncludeInnerErrorInPolicy(policy, withInnerError, satisfyFilterFunc);

			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			TestHandlingForInnerError.HandlePolicyWithIncludeInnerErrorFilter(policy, actionToHandle, withInnerError, satisfyFilterFunc);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_ExcludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var policy = new RetryPolicy(1);
			TestHandlingForInnerError.ExcludeInnerErrorFromPolicy(policy, withInnerError, satisfyFilterFunc);

			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			TestHandlingForInnerError.HandlePolicyWithExcludeInnerErrorFilter(policy, actionToHandle, withInnerError, satisfyFilterFunc);
		}

		[Test]
		public void Should_WithWait_Add_ErrorProcessor()
		{
			var policy = new RetryPolicy(1);
			int i = 0;

			TimeSpan delayOnRetryFunc(int _) { i++; return TimeSpan.Zero; }
			policy.WithWait(delayOnRetryFunc);

			policy.Handle(() => throw new Exception("Test"));
			Assert.That(i, Is.EqualTo(1));

			TimeSpan delayOnRetryFunc2(TimeSpan _, int __, Exception ___) { i++; return TimeSpan.Zero; }
			policy.WithWait(delayOnRetryFunc2, TimeSpan.Zero);

			policy.Handle(() => throw new Exception("Test"));
			Assert.That(i, Is.EqualTo(3));

			policy.WithWait(TimeSpan.Zero);
			policy.Handle(() => throw new Exception("Test"));
			Assert.That(i, Is.EqualTo(5));
		}

		[Test]
		[TestCase(RetryDelayType.Constant, true)]
		[TestCase(RetryDelayType.Constant, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		public void Should_RetryDelay_Returns_Jittered_Timespan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var repeater = new RetryDelayRepeater(GetJitteredRetryDelayByRetryDelayType());
			var res = repeater.Repeat(1, 10);
			RetryDelay GetJitteredRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Constant:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Constant, TimeSpan.FromSeconds(4), true);
						else
							return new ConstantRetryDelay(TimeSpan.FromSeconds(4), true);
					case RetryDelayType.Linear:
						if (useBaseClass)
						{
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2), true);
						}
						else
						{
							return new LinearRetryDelay(TimeSpan.FromSeconds(2), null, true);
						}
					default:
						throw new NotImplementedException();
				}
			}
			Assert.That(res.Exists(t => Math.Abs(4 - t.TotalSeconds) > 0), Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_ExponentialRetryDelay_Returns_Jittered_Timespan(bool useBaseClass)
		{
			var times = new List<TimeSpan>();
			for (int i = 0; i < 10; i++)
			{
				times.Add(GetRetryDelay().GetDelay(1));
			}

			Assert.That(times.Exists(t => Math.Abs(4 - t.TotalSeconds) > 0), Is.True);

			RetryDelay GetRetryDelay()
			{
				if (useBaseClass)
				{
					return new RetryDelay(RetryDelayType.Exponential, TimeSpan.FromSeconds(2), true);
				}
				else
				{
					return new ExponentialRetryDelay(TimeSpan.FromSeconds(2), useJitter: true);
				}
			}
		}

		[Test]
		[TestCase(RetryDelayType.Constant, true)]
		[TestCase(RetryDelayType.Constant, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		public void Should_RetryDelay_Returns_Correct_Timespan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(0, 1, 2);

			switch (retryDelayType)
			{
				case RetryDelayType.Constant:
					Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[1].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[2].TotalSeconds, Is.EqualTo(2));
					break;
				case RetryDelayType.Linear:
					Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[1].TotalSeconds, Is.EqualTo(4));
					Assert.That(res[2].TotalSeconds, Is.EqualTo(6));
					break;
				case RetryDelayType.Exponential:
					Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[1].TotalSeconds, Is.EqualTo(4));
					Assert.That(res[2].TotalSeconds, Is.EqualTo(8));
					break;
			}

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Constant:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Constant, TimeSpan.FromSeconds(2));
						else
							return new ConstantRetryDelay(TimeSpan.FromSeconds(2));
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2));
						else
							return new LinearRetryDelay(TimeSpan.FromSeconds(2));
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential,  TimeSpan.FromSeconds(2));
						else
							return new ExponentialRetryDelay(TimeSpan.FromSeconds(2));
					default:
						throw new NotImplementedException();
				}
			}
		}

		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		public void Should_RetryDelayJittered_Returns_MaxTimeSpan_When_Calculated_One_Exceed_MaxTimeSpan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.EqualTo(TimeSpan.FromMilliseconds(RetryDelayConstants.MaxTimeSpanMs)));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.MaxValue, true);
						else
							return new LinearRetryDelay(TimeSpan.MaxValue, useJitter: true);
					default:
						throw new NotImplementedException();
				}
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_ExponentialRetryDelay_Jittered_Returns_MaxTimeSpan_When_Calculated_One_Exceed_MaxTimeSpan(bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.EqualTo(RetryDelayConstants.MaxTimeSpanFromTicks));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				if (useBaseClass)
					return new RetryDelay(RetryDelayType.Exponential, TimeSpan.MaxValue, true);
				else
					return new ExponentialRetryDelay(TimeSpan.MaxValue, useJitter: true);
			}
		}

		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		public void Should_RetryDelay_Returns_MaxTimeSpan_When_Calculated_One_Exceed_MaxTimeSpan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			if (retryDelayType == RetryDelayType.Exponential)
			{
				Assert.That(res[0], Is.EqualTo(TimeSpan.MaxValue));
			}
			else
			{
				Assert.That(res[0], Is.EqualTo(TimeSpan.FromMilliseconds(RetryDelayConstants.MaxTimeSpanMs)));
			}

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.MaxValue);
						else
							return new LinearRetryDelay(TimeSpan.MaxValue);
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential, TimeSpan.MaxValue);
						else
							return new ExponentialRetryDelay(TimeSpan.MaxValue);
					default:
						throw new NotImplementedException();
				}
			}
		}

		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		public void Should_RetryDelay_NotExceed_MaxDelay(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.EqualTo(TimeSpan.FromSeconds(1)));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
						else
							return new LinearRetryDelay(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1));
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
						else
							return new ExponentialRetryDelay(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1));
					default:
						throw new NotImplementedException();
				}
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Backoff_Occurs_In_Handle_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2,  delayProvider:delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			policy.Handle(() => throw new Exception("Test"));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Backoff_Occurs_In_HandleT_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2, delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			policy.Handle<int>(() => throw new Exception("Test"));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Backoff_Occurs_In_HandleAsync_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2, delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			await policy.HandleAsync((_) => throw new Exception("Test")).ConfigureAwait(false);
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Backoff_Occurs_In_HandleAsyncT_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2, delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			await policy.HandleAsync<int>((_) => throw new Exception("Test")).ConfigureAwait(false);
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		public void Should_Backoff_Occurs_In_Handle_Method_When_InfiniteRetryPolicy_Created_With_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(1)));
				policy.Handle(() => throw new Exception("Test"), source.Token);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		public void Should_Backoff_Occurs_In_HandleT_Method_When_InfiniteRetryPolicy_Created_With_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(1)));
				policy.Handle<int>(() => throw new Exception("Test"), source.Token);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task Should_Backoff_Occurs_In_HandleAsync_Method_When_InfiniteRetryPolicy_Created_With_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(1)));
				await policy.HandleAsync((_) => throw new Exception("Test"), source.Token).ConfigureAwait(false);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task Should_Backoff_Occurs_In_HandleAsyncT_Method_When_InfiniteRetryPolicy_Created_With_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: new LinearRetryDelay(TimeSpan.FromMilliseconds(1)));
				await policy.HandleAsync<int>((_) => throw new Exception("Test"), source.Token).ConfigureAwait(false);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		private class RetryDelayChecker
		{
			private readonly RetryDelay _retryDelay;
			public RetryDelayChecker(RetryDelay retryDelay)
			{
				_retryDelay = retryDelay;
			}

			public List<TimeSpan> Attempt(params int[] attemptNumbers)
			{
				var times = new List<TimeSpan>();
				foreach (var an in attemptNumbers)
				{
					times.Add(_retryDelay.GetDelay(an));
				}
				return times;
			}
		}

		private class RetryDelayRepeater
		{
			private readonly RetryDelay _retryDelay;
			public RetryDelayRepeater(RetryDelay retryDelay)
			{
				_retryDelay = retryDelay;
			}

			public List<TimeSpan> Repeat(int attemptNumber, int numOfRepeats)
			{
				var times = new List<TimeSpan>();
				for (int i = 0; i < numOfRepeats; i++)
				{
					times.Add(_retryDelay.GetDelay(attemptNumber));
				}
				return times;
			}
		}

		private class TestAsyncClass
		{
			private int _i;

			public async Task SaveAsync(CancellationToken ct)
			{
				await Task.Delay(1, ct);
				_i++;
			}

			public int I
			{
				get
				{
					return _i;
				}
			}
		}
	}
}