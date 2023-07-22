using Moq;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackPolicyTests
	{
		[Test]
		public void Should_Handle_Sync_NoGeneric_Fallback_With_No_Cancellation()
		{
			var fallback = new FallbackPolicy().WithFallbackAction((_) => { });

			var polResult = fallback.Handle(() => { });

			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsFalse(polResult.Errors.Any());
			Assert.IsTrue(polResult.NoError);
		}

		[Test]
		public void Should_Handle_Sync_NoGeneric_ByFallbackAsync_If_Error()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });

			var polResult = fallback.Handle(() => throw new Exception("Handle sync by async fallback"));

			Assert.IsTrue(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Any());
			Assert.NotNull(polResult.UnprocessedError);
			Assert.AreEqual(1, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_Handle_Sync_NoGeneric_ByFallbackAsync_If_Success()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			var polResult = fallback.Handle(() => throw new Exception("Handle sync by async fallback"));

			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Count() == 1);
			Assert.Null(polResult.UnprocessedError);
			Assert.AreEqual(0, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_Fallback_Result_BeCanceled_IfTokenJustCanceled()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			var policy = new FallbackPolicy().WithFallbackAction((_) => { });
			var res = policy.Handle(() => { }, cancelTokenSource.Token);

			Assert.IsTrue(res.IsCanceled);
			Assert.IsFalse(res.NoError);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_Work_For_Handle_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var result = fallBackPol.Handle(null);
			Assert.IsTrue(result.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), result.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_WithDelayProcessorOf_Delay_Work()
		{
			var pol = new FallbackPolicy().WithFallbackAction((_) => { }).WithErrorProcessor(new DelayErrorProcessor(TimeSpan.FromMilliseconds(300)));
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			pol.Handle(act);
			sw.Stop();
			Assert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public void Should_FallbackPolicyWithAction_Add_IncludedErrorFilter_Work()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.IncludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test2");
			var polRes = fbWithError.Handle(action);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
			Assert.IsTrue(polRes.IsFailed);
			Assert.AreEqual(1, polRes.Errors.Count());
		}

		[Test]
		public void Should_FallbackPolicyWithAction_Add_ExludedErrorFilter_Work()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.ExcludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test");
			var polRes = fbWithError.Handle(action);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test")]
		public void Should_Add_GenericExludedErrorFilter_Work(string testMsg)
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			void action() => throw new ArgumentNullException(testMsg);
			var polRes = fbWithError.Handle(action);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test")]
		public void Should_Handle_Work_With_No_Delegates(string testMsg)
		{
			var fallBackPol = new FallbackPolicy();
			void action() => throw new ArgumentNullException(testMsg);
			var polRes = fallBackPol.Handle(action);
			Assert.IsFalse(polRes.IsFailed);
		}

		[Test]
		public async Task Should_HandleAsync_Work_With_No_Delegates()
		{
			var fallBackPol = new FallbackPolicy();
			async Task actEmpty(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var polRes = await fallBackPol.HandleAsync(actEmpty);
			Assert.IsFalse(polRes.IsFailed);
			Assert.AreEqual(1, polRes.Errors.Count());
		}

		[Test]
		[TestCase("Test2")]
		public void Should_Add_GenericIncludeErrorFilter_Work(string testMsg)
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.IncludeError<ArgumentNullException>((ane) => ane.ParamName == "Test2");
			void actionSatisfied() => throw new ArgumentNullException(testMsg);
			var polRes = fbWithError.Handle(actionSatisfied);
			Assert.IsFalse(polRes.ErrorFilterUnsatisfied);

			void actionUnSatisfied() => throw new Exception("Test2");
			var polRes2 = fbWithError.Handle(actionUnSatisfied);
			Assert.IsTrue(polRes2.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2")]
		public void Should_IncludeError_For_Different_Types_Work(string testMsg)
		{
			var fallBackPol = new FallbackPolicy()
										.WithFallbackFunc((_) => "EmptyString")
										.WithFallbackFunc((_) => -1)
										.IncludeError<ArgumentNullException>((ane) => ane.ParamName == "Test2")
										.IncludeError<IndexOutOfRangeException>((ioe) => ioe.Message == "Out");

			string actionSatisfiedForFirst() => throw new ArgumentNullException(testMsg);
			var polRes = fallBackPol.Handle(actionSatisfiedForFirst);
			Assert.IsFalse(polRes.ErrorFilterUnsatisfied);
			Assert.AreEqual(1, polRes.Errors.Count());
			Assert.AreEqual("EmptyString", polRes.Result);

			int actionSatisfiedForSecond() => throw new IndexOutOfRangeException("Out");
			var polRes2 = fallBackPol.Handle(actionSatisfiedForSecond);
			Assert.IsFalse(polRes2.ErrorFilterUnsatisfied);
			Assert.AreEqual(1, polRes2.Errors.Count());
			Assert.AreEqual(-1, polRes2.Result);
		}

		[Test]
		public void Should_FallbackPolicy_IncludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithFallbackFunc((_) => -1)
										.IncludeError((ex) => ex.Message == "Test");

			string actionUnsatisfied() => throw new Exception("Test2");
			var polRes = fallBackPol.Handle(actionUnsatisfied);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_FallbackPolicy_ExcludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithFallbackFunc((_) => -1)
										.ExcludeError((ex) => ex.Message == "Test");

			string actionSatisfiedForFirst() => throw new Exception("Test");
			var polRes = fallBackPol.Handle(actionSatisfiedForFirst);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_FallbackPolicyWithAsyncFunc_IncludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithAsyncFallbackFunc(async(_) => await Task.Delay(1))
										.IncludeError((ex) => ex.Message == "Test");
			string actionUnsatisfied() => throw new Exception("Test2");
			var polRes = fallBackPol.Handle(actionUnsatisfied);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_FallbackPolicyWithAsyncFunc_ExcludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithAsyncFallbackFunc(async (_) => await Task.Delay(1))
										.ExcludeError((ex) => ex.Message == "Test");
			string actionUnsatisfied() => throw new Exception("Test");
			var polRes = fallBackPol.Handle(actionUnsatisfied);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2", "Test2", "Test", false)]
		[TestCase("Test", "Test", "Test", true)]
		[TestCase("Test2", "Test", "Test2", true)]
		[TestCase("Test2", "Test", "Test", true)]
		public void Should_IncludeAndExcludeFilter_Work(string paramName, string forErrorParamName, string excludeErrorParamName, bool res)
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			void actionUnsatisied() => throw new ArgumentNullException(paramName);
			fallBackPol.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == excludeErrorParamName).IncludeError<ArgumentNullException>((ane) => ane.ParamName == forErrorParamName);
			var resHandle = fallBackPol.Handle(actionUnsatisied);
			Assert.AreEqual(res, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_WithErrorProcessors_Add_ErrorProcessors()
		{
			var fallBack = new FallbackPolicy();
			var fp = fallBack.WithErrorProcessor(new DefaultErrorProcessor())
							 .WithErrorProcessor(new DefaultErrorProcessor())
							 .WithErrorProcessor(new DefaultErrorProcessor());
			Assert.AreEqual(3, fp.PolicyProcessor.Count());

			var fallBackWithAsync = fallBack.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			fallBackWithAsync.WithErrorProcessor(new DefaultErrorProcessor())
							 .WithErrorProcessor(new DefaultErrorProcessor())
							 .WithErrorProcessor(new DefaultErrorProcessor());
			Assert.AreEqual(6, fp.PolicyProcessor.Count());

			var fallBackFull = fallBackWithAsync.WithFallbackAction((_) => { });
			fallBackFull.WithErrorProcessor(new DefaultErrorProcessor())
							 .WithErrorProcessor(new DefaultErrorProcessor())
							 .WithErrorProcessor(new DefaultErrorProcessor());
			Assert.AreEqual(9, fp.PolicyProcessor.Count());
		}

		[Test]
		public void Should_FallbackActionInit_Work_For_Any_Setup()
		{
			var fbOne = new FallbackPolicy();
			Assert.IsFalse(fbOne.HasFallbackAction());

			var fbOneWithAction = fbOne.WithFallbackAction((_) => { });
			Assert.IsTrue(fbOneWithAction.HasFallbackAction());

			var fbTwo = fbOne.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			Assert.IsFalse(fbTwo.HasFallbackAction());

			var fbTwoWithAction = fbTwo.WithFallbackAction((_) => { });
			Assert.IsTrue(fbTwoWithAction.HasFallbackAction());
		}

		[Test]
		public void Should_FallbackAsyncInit_Work_For_Any_Setup()
		{
			var fbOne = new FallbackPolicy();
			Assert.IsFalse(fbOne.HasAsyncFallbackFunc());

			var fbOneWithAction = fbOne.WithAsyncFallbackFunc(async(_) => await Task.Delay(1));
			Assert.IsTrue(fbOneWithAction.HasAsyncFallbackFunc());

			var fbTwo = fbOne.WithFallbackAction((_) => { });
			Assert.IsFalse(fbTwo.HasAsyncFallbackFunc());

			var fbTwoWithAction = fbTwo.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			Assert.IsTrue(fbTwoWithAction.HasAsyncFallbackFunc());
		}

		[Test]
		public async Task Should_HandleAsync_Work_Without_CancelToken()
		{
			async Task actFallBack(CancellationToken _) => await Task.Delay(1);
			async Task actEmpty(CancellationToken _) { await Task.Delay(1); }

			var policy = new FallbackPolicy().WithAsyncFallbackFunc(actFallBack);
			var polResult = await policy.HandleAsync(actEmpty);
			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsFalse(polResult.Errors.Any());
			Assert.IsTrue(polResult.NoError);
		}

		[Test]
		public async Task Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Error()
		{
			var fallback = new FallbackPolicy().WithFallbackAction((_) => throw new Exception(nameof(Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Error)));
			var polResult = await fallback.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });

			Assert.IsTrue(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Any());
		}

		[Test]
		public async Task Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Success()
		{
			var fallback = new FallbackPolicy().WithFallbackAction((_) => { });
			var polResult = await fallback.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });

			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Count() == 1);
			Assert.AreEqual(0, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var result = await fallBackPol.HandleAsync(null);
			Assert.IsTrue(result.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), result.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_HandlT_Work_Without_CancelToken()
		{
			int actFallBack(CancellationToken _) => 1;
			int actEmpty() => 1;

			var policy = new FallbackPolicy().WithFallbackFunc(actFallBack);
			var polResult = policy.Handle(actEmpty);
			Assert.IsTrue(!polResult.IsFailed);
			Assert.IsTrue(!polResult.IsCanceled);
			Assert.IsTrue(!polResult.Errors.Any());
			Assert.IsTrue(polResult.Result == 1);
		}

		[Test]
		public void Should_Handle_Sync_Generic_Fallback()
		{
			var policy = new FallbackPolicy().WithFallbackFunc((_) => 2);

			var polResult = policy.Handle(() => 45, default);

			Assert.AreEqual(45, polResult.Result);
		}

		[Test]
		public void Should_HandleT_Sync_CrossInitByFuncWithOtherReturnType_Return_Default()
		{
			var retry = new FallbackPolicy().WithFallbackFunc((_) => "Test");

			var polResult = retry.Handle<int>(() => throw new Exception());

			Assert.AreEqual(0, polResult.Result);
		}

		[Test]
		public void Should_HandleT_Sync_CrossInitByAction_Work()
		{
			int k = 0;
			var fallback = new FallbackPolicy().WithFallbackAction((_) => k = 10);
			int func() => throw new Exception();
			var polResult = fallback.Handle(func);
			Assert.AreEqual(default(int), polResult.Result);
			Assert.AreEqual(10, k);
		}

		[Test]
		public void Should_Handle_Sync_Generic_ByFallbackAsync_If_Error()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc<int>(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });
			var polResult = fallback.Handle<int>(() => throw new Exception("Handle sync by async fallback"));

			Assert.IsTrue(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Any());

			Assert.AreEqual(1, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_Handle_Sync_Generic_ByFallbackAsync_If_Success()
		{
			var fallBack = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; });
			var polResult = fallBack.Handle<int>(() => throw new Exception("Handle sync by async fallback"));

			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Count() == 1);
			Assert.AreEqual(0, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_Handle_Sync_Generic_ByFallbackAsync_If_Canceled()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();
				var fallBack = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; });
				var polResult = fallBack.Handle<int>(() => throw new Exception("Handle sync by async fallback"), cancelTokenSource.Token);
				Assert.IsTrue(polResult.IsCanceled);
			}
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy();
			var result = fallBackPol.Handle<int>(null);
			Assert.IsTrue(result.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), result.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_HandleAsyncT_Work_Without_CancelToken()
		{
			async Task<int> actFallBack(CancellationToken _) { await Task.Delay(1); return 1; }
			async Task<int> actEmpty(CancellationToken _) { await Task.Delay(1); return 4; }

			var policy = new FallbackPolicy().WithAsyncFallbackFunc(actFallBack);
			var polResult = await policy.HandleAsync(actEmpty);
			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsFalse(polResult.Errors.Any());
			Assert.IsTrue(polResult.NoError);
			Assert.AreEqual(4, polResult.Result);
		}

		[Test]
		public async Task Should_HandleT_ASync_CrossInitByFuncWithOtherReturnType_Return_Default()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return "Test"; });

			var polResult = await fallback.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception();});

			Assert.AreEqual(0, polResult.Result);
		}

		[Test]
		public async Task Should_HandleAsyncT_CrossInitByParameterlessFunc_Work()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			async Task<int> actEmpty(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var polResult = await fallback.HandleAsync(actEmpty);
			Assert.AreEqual(default(int), polResult.Result);
		}

		[Test]
		public async Task Should_Handle_ASync_Generic_ByFallbackSync_If_Error()
		{
			var fallback = new FallbackPolicy().WithFallbackFunc((Func<CancellationToken, int>)((_) => throw new Exception(nameof(Should_Handle_ASync_Generic_ByFallbackSync_If_Error))));
			var polResult = await fallback.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync");});

			Assert.IsTrue(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Any());
			Assert.IsTrue(polResult.CatchBlockErrors.Count() == 1);
		}

		[Test]
		public async Task Should_Handle_ASync_Generic_ByFallbackSync_If_Success()
		{
			var fallback = new FallbackPolicy().WithFallbackFunc((_) => 1);
			var polResult = await fallback.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync");});

			Assert.IsFalse(polResult.IsFailed);
			Assert.IsFalse(polResult.IsCanceled);
			Assert.IsTrue(polResult.Errors.Count() == 1);
			Assert.AreEqual(0, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => Expression.Empty());
			var result = await fallBackPol.HandleAsync<int>(null);
			Assert.IsTrue(result.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			Assert.AreEqual(result.Errors.FirstOrDefault()?.GetType(), typeof(NoDelegateException));
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_Handle_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = fallback.Handle(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_HandleT_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = fallback.Handle<int>(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_HandleAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = await fallback.HandleAsync(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_HandleTAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = await fallback.HandleAsync<int>(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Cancel()
		{
			using (var cancelSource = new CancellationTokenSource())
			{
				cancelSource.Cancel();
				var fallback = new FallbackPolicy().WithFallbackAction((_) => { });
				var polResult = await fallback.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); }, cancelSource.Token);
				Assert.IsTrue(polResult.IsCanceled);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicyBase_AddPolicyResultHandler_By_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { }) ;
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler((_) => i++);
			}
			policy.Handle(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicyBase_AddPolicyResultHandler_By_Generic_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler<int>((_) => i++);
			}
			policy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicyBase_AddPolicyResultHandler_By_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicyBase_AddPolicyResultHandler_By_Generic_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler<int>(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicy_AddPolicyResultHandler_By_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler((_) => i++);
			}
			policy.Handle(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicy_AddPolicyResultHandler_By_Generic_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler<int>((_) => i++);
			}
			policy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicy_AddPolicyResultHandler_By_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicy_AddPolicyResultHandler_By_Generic_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler<int>(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicyWithAction_AddPolicyResultHandler_By_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler((_) => i++);
			}
			policy.Handle(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicyWithAction_AddPolicyResultHandler_By_Generic_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler<int>((_) => i++);
			}
			policy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicyWithAction_AddPolicyResultHandler_By_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicyyWithAction_AddPolicyResultHandler_By_Generic_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithFallbackAction((_) => { });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler<int>(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicyWithAsyncFunc_AddPolicyResultHandler_By_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async(_) => {await Task.Delay(1); throw new Exception("HandleAsync");});
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler((_) => i++);
			}
			policy.Handle(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_FallbackPolicyWithAsyncFunc_AddPolicyResultHandler_By_Generic_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler<int>((_) => i++);
			}
			policy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicyWithAsyncFunc_AddPolicyResultHandler_By_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackPolicyWithAsyncFunc_AddPolicyResultHandler_By_Generic_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler<int>(async (_) => { await Task.Delay(1); i++; });
			}
			await policy.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}

		[Test]
		public void Should_WithPolicyName_Work()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			Assert.AreEqual(fallBackPol.GetType().Name, fallBackPol.PolicyName);

			const string polName = "FB";
			fallBackPol.WithPolicyName(polName);
			Assert.AreEqual(polName, fallBackPol.PolicyName);
		}
	}
}