using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics;

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
			Assert.AreEqual(1, testClass.I);
		}

		[Test]
		public void Should_Handle_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			void action() => Expression.Empty();

			var res = retry.Handle(action);
			Assert.IsFalse(res.IsFailed);
			Assert.IsFalse(res.IsCanceled);
			Assert.IsFalse(res.Errors.Any());
			Assert.IsTrue(res.IsOk);
			// Method intentionally left empty.
		}

		[Test]
		public async Task Should_HandleAsync_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			async Task action(CancellationToken _) { await Task.Delay(100); }

			var res =await  retry.HandleAsync(action);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.IsOk);
		}

		[Test]
		public void Should_HandleT_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			int action() => 4;

			var res = retry.Handle(action);

			Assert.AreEqual(4, res.Result);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.IsOk);
		}

		[Test]
		public async Task Should_HandleAsyncT_WorkWith_No_CancelToken()
		{
			var retry = new RetryPolicy(1);
			async Task<int> action(CancellationToken _) { await Task.Delay(100); return 4; }

			var res = await retry.HandleAsync(action);
			Assert.AreEqual(4, res.Result);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.IsOk);
		}

		[Test]
		public void Should_Handle_CallRetryProcessor_If_No_Wrap()
		{
			var moqProcessor = new Mock<IRetryProcessor>();

			var rci = new RetryCountInfo(2, null, 0);

			moqProcessor.Setup((t) => t.Retry(It.IsAny<Action>(), rci, default)).Returns(new PolicyResult());

			var policy = new RetryPolicy(moqProcessor.Object, rci);
			policy.Handle(() => { }, default);

			moqProcessor.Verify((t) => t.Retry(It.IsAny<Action>(), rci, default), Times.Exactly(1));
		}

		[Test]
        public void Should_Be_Cancelable_ForSyncAction_WithError()
        {
			var retryPol = RetryPolicy.InfiniteRetries().WithWait(TimeSpan.FromSeconds(1));
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

			void act() => throw new Exception();
			var polResult = retryPol.Handle(act, cancelTokenSource.Token);
			Assert.IsTrue(polResult.IsCanceled);
		}

		[Test]
		public void Should_Be_Cancelable_ForSyncAction_WithDelay()
		{
			var retryPol = RetryPolicy.InfiniteRetries().WithWait(TimeSpan.FromSeconds(1));
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

			void act() => Task.Delay(2000, cancelTokenSource.Token).GetAwaiter().GetResult();
			var polResult = retryPol.Handle(act, cancelTokenSource.Token);
			Assert.IsTrue(polResult.IsCanceled);
			Assert.IsTrue(polResult.IsFailed);
		}

		[Test]
		public void Should_WithDelayProcessorOf_Func_Work()
		{
			var pol = new RetryPolicy(1).WithWait((_, __) => TimeSpan.FromMilliseconds(300));
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			pol.Handle(act);
			sw.Stop();
			Assert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public void Should_WithDelayProcessorOf_Delay_Work()
		{
			var pol = new RetryPolicy(1).WithWait(TimeSpan.FromMilliseconds(300));
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			pol.Handle(act);
			sw.Stop();
			Assert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public async Task Should_Be_Cancelable_ForASyncFunc_WithError()
		{
			var retryPol = RetryPolicy.InfiniteRetries().WithWait(TimeSpan.FromSeconds(1));
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

			async Task func(CancellationToken _) { await Task.Delay(100); throw new Exception(); }

			var polResult = await retryPol.HandleAsync(func, cancelTokenSource.Token);
			Assert.IsTrue(polResult.IsCanceled);
			Assert.IsFalse(polResult.IsSuccess);
			Assert.IsFalse(polResult.IsOk);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_Be_Cancelable_AfterTime_ForNativeToken()
		{
			var retryPol = new RetryPolicy(1);
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50));

			async Task func(CancellationToken _) { await Task.Delay(100, cancelTokenSource.Token); }

			var polResult = await retryPol.HandleAsync(func, cancelTokenSource.Token);
			Assert.IsTrue(polResult.IsCanceled);
			Assert.IsTrue(polResult.IsFailed);
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
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.IsFailed);
			//Because we excluded TaskCanceledException type exception handling.
			Assert.AreEqual(1, polResult.Errors.Count());
			cancelTokenSourceThirdParty.Dispose();
		}

		[Test]
		public void Should_Work_ForZeroRetryCount_ForSyncAction_WithError()
		{
			var retryPol = new RetryPolicy(0);

			void act() => throw new Exception();
			var polResult = retryPol.Handle(act, default);
			Assert.AreEqual(2, polResult.Errors.Count());
			Assert.IsTrue(polResult.IsFailed);
		}

		[Test]
		public void Should_Work_For_Handle_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = retryPolTest.Handle(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = await retryPolTest.HandleAsync(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = retryPolTest.Handle<int>(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var retryPolTest = new RetryPolicy(0);
			var retryResult = await retryPolTest.HandleAsync<int>(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		[TestCase("Test")]
		public void Should_Add_GenericForErrorFilter(string errParamName)
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.ForError<ArgumentNullException>((ane) => ane.ParamName == "Test");

			void actionUnsatisied() => throw new Exception("Test2");
			var polRes = retryPolTest.Handle(actionUnsatisied);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
			Assert.IsTrue(polRes.IsFailed);
			Assert.AreEqual(1, polRes.Errors.Count());

			void actionSatisied() => throw new ArgumentNullException(errParamName);
			var polRes2 = retryPolTest.Handle(actionSatisied);
			Assert.IsFalse(polRes2.ErrorFilterUnsatisfied);
			Assert.IsTrue(polRes2.IsFailed);
			Assert.AreEqual(2, polRes2.Errors.Count());
		}

		[Test]
		[TestCase("Test")]
		public void Should_GenericExcludeFilterWork(string errParamName)
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			void actionUnsatisied() => throw new ArgumentNullException(errParamName);
			var res = retryPolTest.Handle(actionUnsatisied);
			Assert.IsTrue(res.ErrorFilterUnsatisfied);
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
			retryPolTest.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == excludeErrorParamName).ForError<ArgumentNullException>((ane) => ane.ParamName == forErrorParamName);
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			Assert.AreEqual(res, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", "Test", true)]
		[TestCase("Test", "Test2", false)]
		public void Should_IncludeTypeAndExcludeByErrorPropertyFilter_Work(string paramName, string excludeParamName, bool resUnsatisfied)
		{
			var retryPolTest = new RetryPolicy(1);
			void actionUnsatisied() => throw new ArgumentNullException(paramName);
			retryPolTest.ForError<ArgumentNullException>().ExcludeError<ArgumentNullException>((ane) => ane.ParamName == excludeParamName);
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			Assert.AreEqual(resUnsatisfied, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_IncludeTypeAndExcludeByErrorPropertyFilter_ForDiffErorType_Work()
		{
			var retryPolTest = new RetryPolicy(1);
			void actionUnsatisied() => throw new Exception("Test");
			retryPolTest.ForError<ArgumentNullException>().ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test2");
			var resHandle = retryPolTest.Handle(actionUnsatisied);
			Assert.AreEqual(true, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_Add_ForErrorFilter()
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.ForError((e) => e.Message == "Test");
			void action() => throw new Exception("Test2");
			var polRes =  retryPolTest.Handle(action);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
			Assert.IsTrue(polRes.IsFailed);
			Assert.AreEqual(1, polRes.Errors.Count());
		}

		[Test]
		public void Should_Add_MoreThanOne_ForErrorFilter_Work()
		{
			var retryPolTest = new RetryPolicy(1);
			retryPolTest.ForError((e) => e.Message == "Test")
					    .ForError((e) => e.Message == "Test2");
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
			Assert.IsFalse(polRes.ErrorFilterUnsatisfied);
			Assert.IsTrue(polRes.IsFailed);
			Assert.AreEqual(2, polRes.Errors.Count());
		}

		[Test]
		public void Should_Add_ExludedErrorFilter()
		{
			var retryPolTest = new RetryPolicy(2).ExcludeError((e) => e.Message == "Test");
			void act() => throw new Exception("Test");
			var resPol = retryPolTest.Handle(act);
			Assert.AreEqual(1, resPol.Errors.Count());
			Assert.IsTrue(resPol.ErrorFilterUnsatisfied);
			retryPolTest  = retryPolTest.ExcludeError((e) => e.Message == "Test2");
			void act2() => throw new Exception("Test2");
			var resPol2 = retryPolTest.Handle(act2);
			Assert.AreEqual(1, resPol2.Errors.Count());
			Assert.IsTrue(resPol2.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_CatchBlockError_Handled_For_Handle()
		{
			void saveAsync() => throw new Exception();
			void errorProcessorFunc(Exception ex) => throw ex;
			var retryPolTest = new RetryPolicy(1).WithErrorProcessorOf(errorProcessorFunc).ToPolicyDelegate(saveAsync);
			var res = retryPolTest.Handle();
			Assert.IsTrue(res.Errors.Count() == 2);
			Assert.IsTrue(res.CatchBlockErrors.Count() == 1);
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
			Assert.IsFalse(polRes.IsFailed);
			Assert.IsTrue(polRes.CatchBlockErrors.Count() == 1);
			Assert.IsFalse(polRes.CatchBlockErrors.FirstOrDefault().IsCritical);
		}

		[Test]
		public void Should_CatchBlockError_Handled_For_Handle_WithFailedErrorSaver()
		{
			void saveAsync() => throw new Exception();
			void errorProcessorFunc(Exception ex) => throw ex;
			var retryPolTest = new RetryPolicy(1, (pr, _) => pr.SetFailed()).WithErrorProcessorOf(errorProcessorFunc).ToPolicyDelegate(saveAsync);
			var res = retryPolTest.Handle();
			Assert.IsTrue(res.IsFailed);
			Assert.IsTrue(!res.CatchBlockErrors.Any());
		}

		[Test]
		public void Should_CatchBlockError_BeCritical_If_DefaultErrorSaver_HasError()
		{
			var action = PolicyResultHandleErrorDelegates.GetWrappedErrorSaver((_, __) => throw new Exception("Test"));
			var res = new PolicyResult();
			action(res, new Exception());
			Assert.IsTrue(res.IsFailed);
			Assert.IsTrue(res.CatchBlockErrors.Count() == 1);
			Assert.IsTrue(res.CatchBlockErrors.FirstOrDefault().IsCritical);
		}

		[Test]
		public void Should_WithPolicyName_Work()
		{
			var retryPol = new RetryPolicy(1);
			Assert.AreEqual(retryPol.GetType().Name, retryPol.PolicyName);

			const string polName = "RP";
			retryPol.WithPolicyName(polName);
			Assert.AreEqual(polName, retryPol.PolicyName);
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