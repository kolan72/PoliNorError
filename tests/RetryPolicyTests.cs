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
using System.Collections;
using static PoliNorError.Tests.ExceptionFilterTests;

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
		public void Should_RetryPolicy_Handle_Preserve_BadResults_In_WrappedPolicyResults_When_RetryPolicy_Wrap_SimplePolicy()
		{
			int func() => 1;
			var simplePolicy = new SimplePolicy().SetPolicyResultFailedIf<int>((_) => true);
			var retryPolicy = new RetryPolicy(1);
			retryPolicy.WrapPolicy(simplePolicy);
			var retryResult = retryPolicy.Handle(func);
			var wprs = retryResult.WrappedPolicyResults.ToArray();
			Assert.That(wprs[0].Result.Result, Is.EqualTo(1));
			Assert.That(wprs[1].Result.Result, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_RetryPolicy_Handle_Exceptions_When_RetryPolicy_Wrap_SimplePolicy_That_Wrap_OtherPolicy_And_AlwaysSetPolicyResultFailed(bool throwEx)
		{
			Func<int> func;
			if (throwEx)
			{
				func = () => throw new InvalidOperationException();
			}
			else
			{
				func = () => 1;
			}
			var simplePolicy = new SimplePolicy(true)
								.IncludeError<SimplePolicyException>()
								.SetPolicyResultFailedIf<int>((_) => true)
								.WrapPolicy(new RetryPolicy(2));
			var retryPolicy = new RetryPolicy(1);
			retryPolicy.WrapPolicy(simplePolicy).ExcludeError<NotImplementedException>();
			var retryResult = retryPolicy.Handle(func);
			if (throwEx)
			{
				Assert.That(retryResult.Errors.Count(), Is.EqualTo(2));
				Assert.That(retryResult.Errors.Any(ex => ex.GetType() != typeof(InvalidOperationException)), Is.False);
			}
			else
			{
				var wprs = retryResult.WrappedPolicyResults.ToArray();
				Assert.That(wprs[0].Result.Result, Is.EqualTo(1));
				Assert.That(wprs[1].Result.Result, Is.EqualTo(1));
			}
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
		[TestCase(true)]
		[TestCase(false)]
		public void Should_SetPolicyResultFailedIfWithHandlerT_Work(bool setIsFailedByPolicyResultHandler)
		{
			var policy = new RetryPolicy(1);
			PolicyResult<int> polResult = null;
			bool continueThrow = true;
			bool handlerFlag = false;
			void act(PolicyResult<int> _) => handlerFlag = true;
			polResult = policy.SetPolicyResultFailedIf<int>(pr => pr.Errors.Any(e => e.Message == "Test"), act)
				.Handle(() =>
				{
					if (continueThrow)
					{
						continueThrow = !setIsFailedByPolicyResultHandler;
						throw new ArgumentException("Test");
					}
					return 1;
				});

			Assert.That(polResult.IsFailed, Is.EqualTo(true));

			Assert.That(polResult.FailedReason, Is.EqualTo(setIsFailedByPolicyResultHandler
														? PolicyResultFailedReason.PolicyResultHandlerFailed
														: PolicyResultFailedReason.PolicyProcessorFailed));

			Assert.That(handlerFlag, Is.EqualTo(setIsFailedByPolicyResultHandler));
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
		public void Should_WithWait_With_DelayErrorProcessorArg_Called_In_Handle_Method()
		{
			var policy = new RetryPolicy(1);
			var delayProcessor = new TestDelayErrorProcessor(TimeSpan.FromMilliseconds(1));
			policy.WithWait(delayProcessor);
			var res = policy.Handle(() => throw new InvalidOperationException());
			Assert.That(policy.PolicyProcessor.Count(), Is.EqualTo(1));
			Assert.That(delayProcessor.Counter, Is.EqualTo(1));
			Assert.That(res.IsFailed, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Backoff_Occurs_In_Handle_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2,  delayProvider:delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			policy.Handle(() => throw new Exception("Test"));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Backoff_Occurs_In_HandleT_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2, delayProvider: delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			policy.Handle<int>(() => throw new Exception("Test"));
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Backoff_Occurs_In_HandleAsync_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2, delayProvider: delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			await policy.HandleAsync((_) => throw new Exception("Test")).ConfigureAwait(false);
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Backoff_Occurs_In_HandleAsyncT_Method_When_RetryPolicy_Created_With_RetryDelay_Param(bool zeroDelay)
		{
			var delayProvider = new FakeDelayProvider();
			var policy = new RetryPolicy(2, delayProvider: delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(zeroDelay ? 0 : 1)));
			await policy.HandleAsync<int>((_) => throw new Exception("Test")).ConfigureAwait(false);
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(zeroDelay ? 0 : 2));
		}

		[Test]
		public void Should_Backoff_Occurs_In_Handle_Method_When_InfiniteRetryPolicy_Created_With_RetryDelay_Param()
		{
			using (var source = new CancellationTokenSource())
			{
				var delayProvider = new FakeDelayProvider(source);
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(1)));
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
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(1)));
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
				LinearRetryDelay retryDelay = LinearRetryDelay.Create(TimeSpan.FromMilliseconds(1));
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: retryDelay);
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
				var policy = RetryPolicy.InfiniteRetries(delayProvider: delayProvider, null, false, retryDelay: LinearRetryDelay.Create(TimeSpan.FromMilliseconds(1)));
				await policy.HandleAsync<int>((_) => throw new Exception("Test"), source.Token).ConfigureAwait(false);
				Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_RetryPolicy_WithErrorContextProcessor_Throws_Only_For_Not_DefaultRetryProcessor(bool throwEx)
		{
			RetryPolicy retryPolicyTest;
			if (throwEx)
			{
				retryPolicyTest = new RetryPolicy(new TestRetryProcessor(), 1);
			}
			else
			{
				retryPolicyTest = new RetryPolicy(1);
			}
			if (throwEx)
			{
				Assert.Throws<NotImplementedException>(() => retryPolicyTest.WithErrorContextProcessor(new DefaultErrorProcessor<int>((_, __) => { })));
			}
			else
			{
				Assert.DoesNotThrow(() => retryPolicyTest.WithErrorContextProcessor(new DefaultErrorProcessor<int>((_, __) => { })));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WithErrorContextProcessorOf_Action_Throws_For_Not_DefaultRetryProcessor(bool withCancellationType)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var retryPolicyTest = new RetryPolicy(new TestRetryProcessor(), 1);
			if (withCancellationType)
			{
				Assert.Throws<NotImplementedException>(() => retryPolicyTest.WithErrorContextProcessorOf<int>(action, CancellationType.Precancelable));
			}
			else
			{
				Assert.Throws<NotImplementedException>(() => retryPolicyTest.WithErrorContextProcessorOf<int>(action));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Ignore_ActionBased_ErrorContextProcessor_In_Parameterless_Handle_Methods(bool withCancellationType)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m += pi.Param;
			}

			var retryPolicy = new RetryPolicy(2);
			if (withCancellationType)
			{
				retryPolicy.WithErrorContextProcessorOf<int>(action);
			}
			else
			{
				retryPolicy.WithErrorContextProcessorOf<int>(action, CancellationType.Precancelable);
			}

			var result = retryPolicy
							.Handle(() => throw new InvalidOperationException());
			Assert.That(m, Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			var result2 = retryPolicy
							.Handle<int>(() => throw new InvalidOperationException());
			Assert.That(m, Is.Zero);
			Assert.That(result2.IsFailed, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Ignore_FuncBased_ErrorContextProcessor_In_Parameterless_HandleAsync_Methods(bool withCancellationType)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi)
			{
				await Task.Delay(1);
				m += pi.Param;
			}

			var retryPolicy = new RetryPolicy(2);
			if (!withCancellationType)
			{
				retryPolicy.WithErrorContextProcessorOf<int>(fn);
			}
			else
			{
				retryPolicy.WithErrorContextProcessorOf<int>(fn, CancellationType.Precancelable);
			}

			var result = await retryPolicy
							.HandleAsync((_) => throw new InvalidOperationException());
			Assert.That(m, Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			var result2 = await retryPolicy
							.HandleAsync<int>((_) => throw new InvalidOperationException());
			Assert.That(m, Is.Zero);
			Assert.That(result2.IsFailed, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WithErrorProcessorOf_AsyncFunc_Throws_For_Not_DefaultRetryProcessor(bool withCancellationType)
		{
			async Task fn(Exception _, ProcessingErrorInfo<int> __)
			{
				await Task.Delay(1);
			}
			var retryPolicy = new RetryPolicy(new TestRetryProcessor(), 1);
			if (withCancellationType)
			{
				Assert.Throws<NotImplementedException>(() => retryPolicy.WithErrorContextProcessorOf<int>(fn, CancellationType.Precancelable));
			}
			else
			{
				Assert.Throws<NotImplementedException>(() => retryPolicy.WithErrorContextProcessorOf<int>(fn));
			}
		}

		[Test]
		public void Should_WithErrorProcessorOf_Action_With_Token_Throws_For_Not_DefaultRetryProcessor()
		{
			void action(Exception _, ProcessingErrorInfo<int> __, CancellationToken ___)
			{
				// Method intentionally left empty.
			}

			var retryPolicy = new RetryPolicy(new TestRetryProcessor(), 1);
			Assert.Throws<NotImplementedException>(() => retryPolicy.WithErrorContextProcessorOf<int>(action));
		}

		[Test]
		[TestCase(true, null, null)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		[TestCase(false, true, false)]
		[TestCase(false, false, false)]
		public void Should_WithErrorContextProcessor_Throws_Only_For_Not_DefaultRetryProcessor(bool throwEx, bool? wrap, bool? withRetryDelay)
		{
			RetryPolicy retryPolicy;
			if (throwEx)
			{
				retryPolicy = new RetryPolicy(new TestRetryProcessor(), 1);
				Assert.Throws<NotImplementedException>(() => retryPolicy.WithErrorContextProcessor(new DefaultErrorProcessor<int>((_, __) => { })));
			}
			else
			{
				int m = 0;
				int retryCount = 0;

				void action(Exception _, ProcessingErrorInfo<int> pi)
				{
					m += pi.Param;
					retryCount = pi.GetRetryCount();
				}

				if (withRetryDelay == false)
				{
					retryPolicy = new RetryPolicy(2);
				}
				else
				{
					retryPolicy = new RetryPolicy(2, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
				}

				retryPolicy.WithErrorContextProcessorOf<int>(action);

				if (wrap == true)
				{
					retryPolicy = retryPolicy.WrapPolicy(new RetryPolicy(1));
				}

				var result = retryPolicy
							.Handle(() => throw new InvalidOperationException(), 5);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(retryCount, Is.EqualTo(1));
				Assert.That(m, Is.EqualTo(10));
			}
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, null, null)]
		public void Should_Handle_With_TParam_For_Action_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool? useWrap, bool? withRetryDelay)
		{
			int m = 0;
			int addable = 1;
			int attempts = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy retryPolicy;

			if (withRetryDelay == false)
			{
				retryPolicy = new RetryPolicy(1);
			}
			else
			{
				retryPolicy = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			retryPolicy.WithErrorContextProcessorOf<int>(action);

			if (useWrap == true)
			{
				retryPolicy = retryPolicy.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = retryPolicy.Handle((_) => throw new InvalidOperationException(), 5);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap == true ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(result.IsFailed, Is.True);
				Assert.That(attempts, useWrap == true ? Is.EqualTo(0) : Is.EqualTo(1));
			}
			else
			{
#pragma warning disable RCS1021 // Convert lambda expression body to expression-body.
				result = retryPolicy.Handle((v) => { addable += v; }, 5);
#pragma warning restore RCS1021 // Convert lambda expression body to expression-body.
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, false, null)]
		[TestCase(false, true, null)]
		public void Should_Handle_With_TParam_For_Func_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool wrap, bool? withRetryDelay)
		{
			int m = 0;
			int attempts = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy retryPolicy;
			if (withRetryDelay == false)
			{
				retryPolicy = new RetryPolicy(1);
			}
			else
			{
				retryPolicy = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			retryPolicy.WithErrorContextProcessorOf<int>(action);

			if (wrap)
			{
				retryPolicy = retryPolicy.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = retryPolicy.Handle<int, int>(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.IsFailed, Is.True);
				Assert.That(attempts, Is.EqualTo(1));
			}
			else
			{
				result = retryPolicy.Handle(() => 1, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
				Assert.That(result.Result, Is.EqualTo(1));
				Assert.That(result.IsSuccess, Is.True);
			}
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, null)]
		[TestCase(false, true, null)]
		public void Should_Handle_With_TParam_For_Func_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool useWrap, bool? withRetryDelay)
		{
			int m = 0;
			int attempts = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy policyToTest;
			if (withRetryDelay == false)
			{
				policyToTest = new RetryPolicy(1);
			}
			else
			{
				policyToTest = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			policyToTest.WithErrorContextProcessorOf<int>(action);

			if (useWrap)
			{
				policyToTest = policyToTest.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = policyToTest.Handle<int, int>((_) => throw new InvalidOperationException(), 5);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(attempts, useWrap ? Is.EqualTo(0) : Is.EqualTo(1));
				Assert.That(result.IsSuccess, Is.False);
			}
			else
			{
				result = policyToTest.Handle((v) => { addable += v; return addable; }, 5);
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.IsSuccess, Is.True);
				Assert.That(result.Result, Is.EqualTo(6));
			}
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, null)]
		[TestCase(false, true, null)]
		public async Task Should_HandleAsync_With_TParam_For_NonGeneric_AsyncFunc_WithErrorContextProcessor_Be_Correct(bool throwEx, bool useWrap, bool? withRetryDelay)
		{
			int m = 0;
			int attempts = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy retryPolicy;
			if (withRetryDelay == false)
			{
				retryPolicy = new RetryPolicy(1);
			}
			else
			{
				retryPolicy = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			retryPolicy.WithErrorContextProcessorOf<int>(action);
			if (useWrap)
			{
				retryPolicy = retryPolicy.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;

			if (throwEx)
			{
				result = await retryPolicy
						.HandleAsync(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);

				Assert.That(result.IsSuccess, Is.False);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(attempts, Is.EqualTo(1));
			}
			else
			{
				result = await retryPolicy
					.HandleAsync(async (_) => await Task.Delay(1), 5, false, default);

				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.IsSuccess, Is.True);
				Assert.That(result.NoError, Is.True);
				Assert.That(attempts, Is.EqualTo(0));
			}
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, null)]
		[TestCase(false, true, null)]
		public async Task Should_HandleAsync_With_TParam_For_AsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool useWrap, bool? withRetryDelay)
		{
			int m = 0;
			int addable = 1;

			int attempts = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy retryPolicy;
			if (withRetryDelay == false)
			{
				retryPolicy = new RetryPolicy(1);
			}
			else
			{
				retryPolicy = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			retryPolicy.WithErrorContextProcessorOf<int>(action);

			if (useWrap)
			{
				retryPolicy = retryPolicy.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = await retryPolicy.HandleAsync(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(result.IsFailed, Is.True);
				Assert.That(attempts, useWrap ? Is.EqualTo(0) : Is.EqualTo(1));
			}
			else
			{
				result = await retryPolicy.HandleAsync(async (v, _) => { await Task.Delay(1); addable += v; }, 5, false, default);
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.IsSuccess, Is.True);
			}
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, null)]
		[TestCase(false, true, null)]
		public async Task Should_HandleAsync_With_TParam_For_Generic_AsyncFunc_WithErrorContextProcessor_Be_Correct(bool throwEx, bool useWrap, bool? withRetryDelay)
		{
			int m = 0;
			int attempts = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy retryPolicy;
			if (withRetryDelay == false)
			{
				retryPolicy = new RetryPolicy(1);
			}
			else
			{
				retryPolicy = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			retryPolicy.WithErrorContextProcessorOf<int>(action);
			if (useWrap)
			{
				retryPolicy = retryPolicy.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult<int> result = null;

			if (throwEx)
			{
				result = await retryPolicy
						.HandleAsync<int, int>(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);

				Assert.That(result.IsSuccess, Is.False);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(attempts, Is.EqualTo(1));
			}
			else
			{
				result = await retryPolicy
					.HandleAsync(async (_) => { await Task.Delay(1); return 1; }, 5, false, default);

				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.IsSuccess, Is.True);
				Assert.That(result.NoError, Is.True);
				Assert.That(attempts, Is.EqualTo(0));
				Assert.That(result.Result, Is.EqualTo(1));
			}
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, null)]
		[TestCase(false, true, null)]
		public async Task Should_HandleAsync_With_TParam_For_GenericAsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool useWrap, bool? withRetryDelay)
		{
			int m = 0;
			int attempts = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
				attempts = pi.GetAttemptCount();
			}

			RetryPolicy policyToTest;
			if (withRetryDelay == false)
			{
				policyToTest = new RetryPolicy(1);
			}
			else
			{
				policyToTest = new RetryPolicy(1, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			policyToTest.WithErrorContextProcessorOf<int>(action);

			if (useWrap)
			{
				policyToTest = policyToTest.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = await policyToTest.HandleAsync<int, int>(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(attempts, useWrap ? Is.EqualTo(0) : Is.EqualTo(1));
				Assert.That(result.IsSuccess, Is.False);
			}
			else
			{
				result = await policyToTest.HandleAsync(async (v, _) => { await Task.Delay(1); addable += v; return addable; }, 5, false, default);
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.IsSuccess, Is.True);
				Assert.That(result.Result, Is.EqualTo(6));
			}
		}

		[Test]
		public void Should_WithErrorProcessorOf_AsyncFunc_With_Token_Throws_For_Not_DefaultRetryProcessor()
		{
			async Task fn(Exception _, ProcessingErrorInfo<int> __, CancellationToken ___)
			{
				await Task.Delay(1);
			}
			var retryPolicy = new RetryPolicy(new TestRetryProcessor(), 1);

			Assert.Throws<NotImplementedException>(() => retryPolicy.WithErrorContextProcessorOf<int>(fn));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Handle_RetryCount_In_ErrorProcessor_And_ErrorProcessorWithContext(bool withRetryDelay)
		{
			RetryPolicy retryPolicy;
			int retryCount = 0;
			int retryCountWithContext = 0;

			int m = 0;

			void actionWithContext(Exception _, ProcessingErrorInfo<int> pi)
			{
				m += pi.Param;
				retryCountWithContext = pi.GetRetryCount();
			}

			void action(Exception _, ProcessingErrorInfo pi)
			{
				retryCount = pi.GetRetryCount();
			}

			if (!withRetryDelay)
			{
				retryPolicy = new RetryPolicy(2);
			}
			else
			{
				retryPolicy = new RetryPolicy(2, false, new ConstantRetryDelay(TimeSpan.FromTicks(1)));
			}

			retryPolicy
				.WithErrorContextProcessorOf<int>(actionWithContext)
				.WithErrorProcessorOf(action);

			var result = retryPolicy
						.Handle(() => throw new InvalidOperationException(), 5);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(retryCount, Is.EqualTo(1));
			Assert.That(retryCountWithContext, Is.EqualTo(1));
			Assert.That(m, Is.EqualTo(10));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_ThenFallback_Returns_Valid_Result(bool thenFallback)
		{
#pragma warning disable RCS1118 // Mark local variable as const.
			var zero = 0;
#pragma warning restore RCS1118 // Mark local variable as const.

			const string fallBackLogMsg = "Fallback to int.MaxValue";
			string errorProcessorMsg = null;
			var policy = new RetryPolicy(3)
									.ExcludeError<DivideByZeroException>()
									.ThenFallback()
									.WithFallbackFunc(() => int.MaxValue)
									.IncludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => errorProcessorMsg = fallBackLogMsg);

			if (thenFallback)
			{
				var fallbackResult = policy
									.Handle(() => 5 / zero);

				Assert.That(fallbackResult.IsSuccess, Is.True);
				Assert.That(fallbackResult.Result, Is.EqualTo(int.MaxValue));
				Assert.That(fallbackResult.Errors.Count(), Is.EqualTo(1));
				Assert.That(fallbackResult.Errors.FirstOrDefault()?.GetType(), Is.EqualTo(typeof(DivideByZeroException)));
				Assert.That(fallbackResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count, Is.EqualTo(1));
				Assert.That(errorProcessorMsg, Is.EqualTo(fallBackLogMsg));
			}
			else
			{
				var result = policy
									.Handle<int>(() =>throw new InvalidOperationException());

				Assert.That(result.IsFailed, Is.True);
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				Assert.That(result.Result, Is.EqualTo(default(int)));
				Assert.That(result.Errors.Count(), Is.EqualTo(1));
				Assert.That(result.UnprocessedError.GetType(), Is.EqualTo(typeof(InvalidOperationException)));
				Assert.That(result.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count, Is.EqualTo(4));
			}
		}

		[Test]
		public void Should_ThenFallback_Returns_Valid_Result_When_NoError()
		{
			const string fallBackLogMsg = "Fallback to int.MaxValue";
			string errorProcessorMsg = null;
			var result = new RetryPolicy(3)
									.ExcludeError<DivideByZeroException>()
									.ThenFallback()
									.WithFallbackFunc(() => int.MaxValue)
									.IncludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => errorProcessorMsg = fallBackLogMsg)
									.Handle(() => 5 / 1);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
			Assert.That(result.Errors.Count(), Is.EqualTo(0));
			Assert.That(result.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count, Is.EqualTo(0));
			Assert.That(errorProcessorMsg, Is.Null);
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

		private class TestDelayErrorProcessor : DelayErrorProcessor
		{
			public TestDelayErrorProcessor(TimeSpan timeSpan) : base(timeSpan) { }

			public override Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
			{
				Counter++;
				return error;
			}

			public int Counter { get; private set; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>")]
		private class SimplePolicyException : Exception{}

		public class TestRetryProcessor : IRetryProcessor
		{
			public PolicyProcessor.ExceptionFilter ErrorFilter => throw new InvalidOperationException();

			public void AddErrorProcessor(IErrorProcessor newErrorProcessor) => throw new InvalidOperationException();
			public IEnumerator<IErrorProcessor> GetEnumerator() => throw new InvalidOperationException();
			public PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, CancellationToken token = default) => throw new InvalidOperationException();
			public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default) => throw new InvalidOperationException();
			public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default) => throw new InvalidOperationException();
			public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default) => throw new InvalidOperationException();
			public IRetryProcessor UseCustomErrorSaver(IErrorProcessor saveErrorProcessor) => throw new InvalidOperationException();
			IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException();
		}
	}
}