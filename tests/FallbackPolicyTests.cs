using NUnit.Framework;
using NUnit.Framework.Legacy;
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

			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsFalse(polResult.Errors.Any());
			ClassicAssert.IsTrue(polResult.NoError);
			ClassicAssert.AreEqual(fallback.PolicyName, polResult.PolicyName);
		}

		[Test]
		public void Should_CrossHandle_Sync_NoGeneric_ByFallbackAsync_If_Error()
		{
			var throwingException = new Exception("HandleAsync");
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); throw throwingException;});

			var polResult = fallback.Handle(() => throw new Exception("Handle sync by async fallback"));

			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Any());
			ClassicAssert.NotNull(polResult.UnprocessedError);
			ClassicAssert.AreEqual(1, polResult.CatchBlockErrors.Count());
			ClassicAssert.NotNull(polResult.CriticalError);
			ClassicAssert.IsTrue(typeof(AggregateException) == polResult.CriticalError.GetType());
			ClassicAssert.AreEqual(throwingException, ((AggregateException)polResult.CriticalError).InnerExceptions.FirstOrDefault());
		}

		[Test]
		public void Should_CrossHandle_Sync_NoGeneric_ByFallbackAsync_If_Success()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			var polResult = fallback.Handle(() => throw new Exception("Handle sync by async fallback"));

			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Count() == 1);
			ClassicAssert.Null(polResult.UnprocessedError);
			ClassicAssert.AreEqual(0, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_UnprocessedError_Be_Null_Even_SetFailed_In_PolicyResultHandler()
		{
			async Task func(PolicyResult pr) { await Task.Delay(1); pr.SetFailed(); }
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction(() => { })
				.AddPolicyResultHandler(func)
				;
			var res = fallbackPolicy.Handle(() => throw new Exception());
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.Null(res.UnprocessedError);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		public void Should_Fallback_Result_BeCanceled_IfTokenJustCanceled()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			var policy = new FallbackPolicy().WithFallbackAction((_) => { });
			var res = policy.Handle(() => { }, cancelTokenSource.Token);

			ClassicAssert.IsTrue(res.IsCanceled);
			ClassicAssert.IsFalse(res.NoError);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_Work_For_Handle_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var result = fallBackPol.Handle(null);
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), result.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(fallBackPol.PolicyName, result.PolicyName);
		}

		[Test]
		public void Should_WithDelayProcessorOf_Delay_Work()
		{
			var pol = new FallbackPolicy().WithFallbackAction((_) => { }).WithErrorProcessor(new DelayErrorProcessor(TimeSpan.FromMilliseconds(300)));
			void act() => throw new Exception();
			var sw = Stopwatch.StartNew();
			pol.Handle(act);
			sw.Stop();
			ClassicAssert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 250);
		}

		[Test]
		public void Should_FallbackPolicyWithAction_Add_IncludedErrorFilter_Work()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.IncludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test2");
			var polRes = fbWithError.Handle(action);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(polRes.IsFailed);
			ClassicAssert.AreEqual(1, polRes.Errors.Count());
		}

		[Test]
		public void Should_FallbackPolicyWithAction_Add_ExludedErrorFilter_Work()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.ExcludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test");
			var polRes = fbWithError.Handle(action);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test")]
		public void Should_Add_GenericExludedErrorFilter_Work(string testMsg)
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			void action() => throw new ArgumentNullException(testMsg);
			var polRes = fbWithError.Handle(action);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test")]
		public void Should_Handle_Work_With_No_Delegates(string testMsg)
		{
			var fallBackPol = new FallbackPolicy();
			void action() => throw new ArgumentNullException(testMsg);
			var polRes = fallBackPol.Handle(action);
			ClassicAssert.IsFalse(polRes.IsFailed);
		}

		[Test]
		public async Task Should_HandleAsync_Work_With_No_Delegates()
		{
			var fallBackPol = new FallbackPolicy();
			async Task actEmpty(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var polRes = await fallBackPol.HandleAsync(actEmpty);
			ClassicAssert.IsFalse(polRes.IsFailed);
			ClassicAssert.AreEqual(1, polRes.Errors.Count());
		}

		[Test]
		[TestCase("Test2")]
		public void Should_Add_GenericIncludeErrorFilter_Work(string testMsg)
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var fbWithError = fallBackPol.IncludeError<ArgumentNullException>((ane) => ane.ParamName == "Test2");
			void actionSatisfied() => throw new ArgumentNullException(testMsg);
			var polRes = fbWithError.Handle(actionSatisfied);
			ClassicAssert.IsFalse(polRes.ErrorFilterUnsatisfied);

			void actionUnSatisfied() => throw new Exception("Test2");
			var polRes2 = fbWithError.Handle(actionUnSatisfied);
			ClassicAssert.IsTrue(polRes2.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyBase(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithFallbackAction(() => { }).WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			_ = fallBackPolicyTest.ExcludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyBaseh_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithFallbackAction(() => { }).WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = fallBackPolicyTest.ExcludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyBase(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithFallbackAction(() => { }).WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			_ = fallBackPolicyTest.IncludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyBase_With_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithFallbackAction(() => { }).WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = fallBackPolicyTest.IncludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicy(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; }) ;
			_ = fallBackPolicyTest.ExcludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicy_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; });
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = fallBackPolicyTest.ExcludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicy(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1;});
			_ = fallBackPolicyTest.IncludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicy_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; });
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = fallBackPolicyTest.IncludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyWithAsyncFunc(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			_ = fallBackPolicyTest.ExcludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyWithAsyncFunc(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			_ = fallBackPolicyTest.IncludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyWithAction(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithFallbackAction((_) => { });
			_ = fallBackPolicyTest.ExcludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_For_FallbackPolicyWithAction(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var fallBackPolicyTest = new FallbackPolicy().WithFallbackAction(async (_) => await Task.Delay(1));
			_ = fallBackPolicyTest.IncludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(fallBackPolicyTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
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
			ClassicAssert.IsFalse(polRes.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(1, polRes.Errors.Count());
			ClassicAssert.AreEqual("EmptyString", polRes.Result);

			int actionSatisfiedForSecond() => throw new IndexOutOfRangeException("Out");
			var polRes2 = fallBackPol.Handle(actionSatisfiedForSecond);
			ClassicAssert.IsFalse(polRes2.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(1, polRes2.Errors.Count());
			ClassicAssert.AreEqual(-1, polRes2.Result);
		}

		[Test]
		public void Should_FallbackPolicy_IncludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithFallbackFunc((_) => -1)
										.IncludeError((ex) => ex.Message == "Test");

			string actionUnsatisfied() => throw new Exception("Test2");
			var polRes = fallBackPol.Handle(actionUnsatisfied);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_FallbackPolicy_ExcludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithFallbackFunc((_) => -1)
										.ExcludeError((ex) => ex.Message == "Test");

			string actionSatisfiedForFirst() => throw new Exception("Test");
			var polRes = fallBackPol.Handle(actionSatisfiedForFirst);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_FallbackPolicyWithAsyncFunc_IncludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithAsyncFallbackFunc(async(_) => await Task.Delay(1))
										.IncludeError((ex) => ex.Message == "Test");
			string actionUnsatisfied() => throw new Exception("Test2");
			var polRes = fallBackPol.Handle(actionUnsatisfied);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_FallbackPolicyWithAsyncFunc_ExcludeError_Based_On_Expression_Work()
		{
			var fallBackPol = new FallbackPolicy()
										.WithAsyncFallbackFunc(async (_) => await Task.Delay(1))
										.ExcludeError((ex) => ex.Message == "Test");
			string actionUnsatisfied() => throw new Exception("Test");
			var polRes = fallBackPol.Handle(actionUnsatisfied);
			ClassicAssert.IsTrue(polRes.ErrorFilterUnsatisfied);
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
			ClassicAssert.AreEqual(res, resHandle.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_WithErrorProcessors_Add_ErrorProcessors()
		{
			var fallBack = new FallbackPolicy();
			var fp = fallBack.WithErrorProcessor(new BasicErrorProcessor())
							 .WithErrorProcessor(new BasicErrorProcessor())
							 .WithErrorProcessor(new BasicErrorProcessor());
			ClassicAssert.AreEqual(3, fp.PolicyProcessor.Count());

			var fallBackWithAsync = fallBack.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			fallBackWithAsync.WithErrorProcessor(new BasicErrorProcessor())
							 .WithErrorProcessor(new BasicErrorProcessor())
							 .WithErrorProcessor(new BasicErrorProcessor());
			ClassicAssert.AreEqual(6, fp.PolicyProcessor.Count());

			var fallBackFull = fallBackWithAsync.WithFallbackAction((_) => { });
			fallBackFull.WithErrorProcessor(new BasicErrorProcessor())
							 .WithErrorProcessor(new BasicErrorProcessor())
							 .WithErrorProcessor(new BasicErrorProcessor());
			ClassicAssert.AreEqual(9, fp.PolicyProcessor.Count());
		}

		[Test]
		public void Should_FallbackActionInit_Work_For_Any_Setup()
		{
			var fbOne = new FallbackPolicy();
			ClassicAssert.IsFalse(fbOne.HasFallbackAction());

			var fbOneWithAction = fbOne.WithFallbackAction((_) => { });
			ClassicAssert.IsTrue(fbOneWithAction.HasFallbackAction());

			var fbTwo = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			ClassicAssert.IsFalse(fbTwo.HasFallbackAction());

			var fbTwoWithAction = fbTwo.WithFallbackAction((_) => { });
			ClassicAssert.IsTrue(fbTwoWithAction.HasFallbackAction());
		}

		[Test]
		public void Should_FallbackAsyncInit_Work_For_Any_Setup()
		{
			var fbOne = new FallbackPolicy();
			ClassicAssert.IsFalse(fbOne.HasAsyncFallbackFunc());

			var fbOneWithAction = fbOne.WithAsyncFallbackFunc(async(_) => await Task.Delay(1));
			ClassicAssert.IsTrue(fbOneWithAction.HasAsyncFallbackFunc());

			var fbTwo = new FallbackPolicy().WithFallbackAction((_) => { });
			ClassicAssert.IsFalse(fbTwo.HasAsyncFallbackFunc());

			var fbTwoWithAction = fbTwo.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			ClassicAssert.IsTrue(fbTwoWithAction.HasAsyncFallbackFunc());
		}

		[Test]
		public async Task Should_HandleAsync_Work_Without_CancelToken()
		{
			async Task actFallBack(CancellationToken _) => await Task.Delay(1);
			async Task actEmpty(CancellationToken _) { await Task.Delay(1); }

			var policy = new FallbackPolicy().WithAsyncFallbackFunc(actFallBack);
			var polResult = await policy.HandleAsync(actEmpty);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsFalse(polResult.Errors.Any());
			ClassicAssert.IsTrue(polResult.NoError);
		}

		[Test]
		public async Task Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Error()
		{
			var fallback = new FallbackPolicy().WithFallbackAction((_) => throw new Exception(nameof(Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Error)));
			var polResult = await fallback.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });

			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Any());
		}

		[Test]
		public async Task Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Success()
		{
			var fallback = new FallbackPolicy().WithFallbackAction((_) => { });
			var polResult = await fallback.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });

			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Count() == 1);
			ClassicAssert.AreEqual(0, polResult.CatchBlockErrors.Count());
			ClassicAssert.IsFalse(polResult.NoError);
			ClassicAssert.IsTrue(polResult.IsSuccess);
			ClassicAssert.IsTrue(polResult.IsPolicySuccess);
			ClassicAssert.AreEqual(fallback.PolicyName, polResult.PolicyName);
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			var result = await fallBackPol.HandleAsync(null);
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), result.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(fallBackPol.PolicyName, result.PolicyName);
		}

		[Test]
		public void Should_HandlT_Work_Without_CancelToken()
		{
			int actFallBack(CancellationToken _) => 1;
			int actEmpty() => 1;

			var policy = new FallbackPolicy().WithFallbackFunc(actFallBack);
			var polResult = policy.Handle(actEmpty);
			ClassicAssert.IsTrue(!polResult.IsFailed);
			ClassicAssert.IsTrue(!polResult.IsCanceled);
			ClassicAssert.IsTrue(!polResult.Errors.Any());
			ClassicAssert.IsTrue(polResult.Result == 1);
		}

		[Test]
		public void Should_Handle_Sync_Generic_Fallback()
		{
			var policy = new FallbackPolicy().WithFallbackFunc((_) => 2);

			var polResult = policy.Handle(() => 45, default);

			ClassicAssert.AreEqual(45, polResult.Result);
		}

		[Test]
		public void Should_HandleT_Sync_CrossInitByFuncWithOtherReturnType_Return_Default()
		{
			var retry = new FallbackPolicy().WithFallbackFunc((_) => "Test");

			var polResult = retry.Handle<int>(() => throw new Exception());

			ClassicAssert.AreEqual(0, polResult.Result);
		}

		[Test]
		public void Should_HandleT_Sync_CrossInitByAction_Work()
		{
			int k = 0;
			var fallback = new FallbackPolicy().WithFallbackAction((_) => k = 10);
			int func() => throw new Exception();
			var polResult = fallback.Handle(func);
			ClassicAssert.AreEqual(default(int), polResult.Result);
			ClassicAssert.AreEqual(10, k);
		}

		[Test]
		public void Should_Handle_Sync_Generic_ByFallbackAsync_If_Error()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc<int>(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); });
			var polResult = fallback.Handle<int>(() => throw new Exception("Handle sync by async fallback"));

			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Any());

			ClassicAssert.AreEqual(1, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_Handle_Sync_Generic_ByFallbackAsync_If_Success()
		{
			var fallBack = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; });
			var polResult = fallBack.Handle<int>(() => throw new Exception("Handle sync by async fallback"));

			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Count() == 1);
			ClassicAssert.AreEqual(0, polResult.CatchBlockErrors.Count());
		}

		[Test]
		public void Should_Handle_Sync_Generic_ByFallbackAsync_If_Canceled()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();
				var fallBack = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; });
				var polResult = fallBack.Handle<int>(() => throw new Exception("Handle sync by async fallback"), cancelTokenSource.Token);
				ClassicAssert.IsTrue(polResult.IsCanceled);
			}
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy();
			var result = fallBackPol.Handle<int>(null);
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), result.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(fallBackPol.PolicyName, result.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_Work_Without_CancelToken()
		{
			async Task<int> actFallBack(CancellationToken _) { await Task.Delay(1); return 1; }
			async Task<int> actEmpty(CancellationToken _) { await Task.Delay(1); return 4; }

			var policy = new FallbackPolicy().WithAsyncFallbackFunc(actFallBack);
			var polResult = await policy.HandleAsync(actEmpty);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsFalse(polResult.Errors.Any());
			ClassicAssert.IsTrue(polResult.NoError);
			ClassicAssert.AreEqual(4, polResult.Result);
			ClassicAssert.AreEqual(policy.PolicyName, polResult.PolicyName);
		}

		[Test]
		public async Task Should_HandleT_ASync_CrossInitByFuncWithOtherReturnType_Return_Default()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return "Test"; });

			var polResult = await fallback.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception();});

			ClassicAssert.AreEqual(0, polResult.Result);
		}

		[Test]
		public async Task Should_HandleAsyncT_CrossInitByParameterlessFunc_Work()
		{
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
			async Task<int> actEmpty(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			var polResult = await fallback.HandleAsync(actEmpty);
			ClassicAssert.AreEqual(default(int), polResult.Result);
		}

		[Test]
		public async Task Should_CrossHandle_ASync_Generic_ByFallbackSync_If_Error()
		{
			var throwingException = new Exception(nameof(Should_CrossHandle_ASync_Generic_ByFallbackSync_If_Error));
			var fallback = new FallbackPolicy().WithFallbackFunc((Func<CancellationToken, int>)((_) => throw throwingException));
			var polResult = await fallback.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync");});

			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Any());
			ClassicAssert.IsTrue(polResult.CatchBlockErrors.Count() == 1);
			ClassicAssert.AreEqual(throwingException, polResult.CriticalError);
		}

		[Test]
		public async Task Should_CrossHandle_ASync_Generic_ByFallbackSync_If_Success()
		{
			var fallback = new FallbackPolicy().WithFallbackFunc((_) => 1);
			var polResult = await fallback.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync");});

			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsFalse(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.Errors.Count() == 1);
			ClassicAssert.AreEqual(0, polResult.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(1, polResult.Result);
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => Expression.Empty());
			var result = await fallBackPol.HandleAsync<int>(null);
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.FailedReason);
			ClassicAssert.AreEqual(result.Errors.FirstOrDefault()?.GetType(), typeof(NoDelegateException));
			ClassicAssert.AreEqual(fallBackPol.PolicyName, result.PolicyName);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_Handle_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = fallback.Handle(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(fallback.PolicyName, wrapResult.PolicyName);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_HandleT_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = fallback.Handle<int>(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_HandleAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = await fallback.HandleAsync(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackPolicy_Wrap_OtherPolicy_And_HandleTAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var fallback = new FallbackPolicy();
			fallback.WrapPolicy(simpleWrapped);
			var wrapResult = await fallback.HandleAsync<int>(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.AreEqual(fallback.PolicyName, wrapResult.PolicyName);
		}

		[Test]
		public async Task Should_Handle_ASync_NoGeneric_ByFallbackSync_If_Cancel()
		{
			using (var cancelSource = new CancellationTokenSource())
			{
				cancelSource.Cancel();
				var fallback = new FallbackPolicy().WithFallbackAction((_) => { });
				var polResult = await fallback.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("HandleAsync"); }, cancelSource.Token);
				ClassicAssert.IsTrue(polResult.IsCanceled);
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
			ClassicAssert.AreEqual(1, i);
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
			var polResult = policy.Handle<int>(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(policy.PolicyName, polResult.PolicyName);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
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
			ClassicAssert.AreEqual(1, i);
		}

		[Test]
		public void Should_WithPolicyName_Work()
		{
			var fallBackPol = new FallbackPolicy().WithFallbackAction((_) => { });
			ClassicAssert.AreEqual(fallBackPol.GetType().Name, fallBackPol.PolicyName);

			const string polName = "FB";
			fallBackPol.WithPolicyName(polName);
			ClassicAssert.AreEqual(polName, fallBackPol.PolicyName);
		}

		[Test]
		public void Should_WrapThrow_If_More_Than_One()
		{
			var fallBackPol = new FallbackPolicy();
			fallBackPol.WrapPolicy(new SimplePolicy());
			ClassicAssert.Throws<NotImplementedException>(() => fallBackPol.WrapPolicy(new SimplePolicy()));
		}

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass, true)]
		[TestCase(FallbackTypeForTests.BaseClass, false)]
		[TestCase(FallbackTypeForTests.Creator, true)]
		[TestCase(FallbackTypeForTests.Creator, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false)]
		[TestCase(FallbackTypeForTests.WithAction, true)]
		[TestCase(FallbackTypeForTests.WithAction, false)]
		public void Should_SetPolicyResultFailedIf_Work(FallbackTypeForTests fallbackType, bool generic)
		{
			PolicyResult polResult = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.BaseClass:
					var fbBase = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
					if (generic)
					{
						polResult = fbBase.SetPolicyResultFailedIf<int>(PredicateFuncsForTests.GenericPredicate)
											.Handle<int>(() => throw new ArgumentException("Test"));
					}
					else
					{
						polResult = fbBase.SetPolicyResultFailedIf(PredicateFuncsForTests.Predicate)
											.Handle(() => throw new ArgumentException("Test"));
					}
					break;
				case FallbackTypeForTests.Creator:
					var fbCreator = new FallbackPolicy();
					if (generic)
					{
						polResult = fbCreator.SetPolicyResultFailedIf<int>(PredicateFuncsForTests.GenericPredicate)
											.Handle<int>(() => throw new ArgumentException("Test"));
					}
					else
					{
						polResult = fbCreator.SetPolicyResultFailedIf(PredicateFuncsForTests.Predicate)
											.Handle(() => throw new ArgumentException("Test"));
					}
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					var fbWithAsyncFunc = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
					if (generic)
					{
						polResult = fbWithAsyncFunc.SetPolicyResultFailedIf<int>(PredicateFuncsForTests.GenericPredicate)
											.Handle<int>(() => throw new ArgumentException("Test"));
					}
					else
					{
						polResult = fbWithAsyncFunc.SetPolicyResultFailedIf(PredicateFuncsForTests.Predicate)
											.Handle(() => throw new ArgumentException("Test"));
					}
					break;
				case FallbackTypeForTests.WithAction:
					var fbWithAction = new FallbackPolicy().WithFallbackAction((_) => { });
					if (generic)
					{
						polResult = fbWithAction.SetPolicyResultFailedIf<int>(PredicateFuncsForTests.GenericPredicate)
											.Handle<int>(() => throw new ArgumentException("Test"));
					}
					else
					{
						polResult = fbWithAction.SetPolicyResultFailedIf(PredicateFuncsForTests.Predicate)
											.Handle(() => throw new ArgumentException("Test"));
					}
					break;
				default:
					throw new NotImplementedException();
			}
			Assert.That(polResult.IsFailed, Is.EqualTo(true));
			Assert.That(polResult.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyResultHandlerFailed));
			Assert.That(polResult.FailedHandlerIndex, Is.EqualTo(0));
		}

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass, true, true)]
		[TestCase(FallbackTypeForTests.BaseClass, true, false)]
		[TestCase(FallbackTypeForTests.BaseClass, true, null)]
		[TestCase(FallbackTypeForTests.BaseClass, false, null)]
		[TestCase(FallbackTypeForTests.Creator, true, true)]
		[TestCase(FallbackTypeForTests.Creator, true, false)]
		[TestCase(FallbackTypeForTests.Creator, true, null)]
		[TestCase(FallbackTypeForTests.Creator, false, null)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, null)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false, null)]
		[TestCase(FallbackTypeForTests.WithAction, true, true)]
		[TestCase(FallbackTypeForTests.WithAction, true, false)]
		[TestCase(FallbackTypeForTests.WithAction, true, null)]
		[TestCase(FallbackTypeForTests.WithAction, false, null)]
		public void Should_IncludeInnerError_Work(FallbackTypeForTests fallbackType, bool withInnerError, bool? satisfyFilterFunc)
		{
			IPolicyBase policy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.BaseClass:
					var fbBase = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
					TestHandlingForInnerError.IncludeInnerErrorInPolicy(fbBase, withInnerError, satisfyFilterFunc);
					policy = fbBase;
					break;
				case FallbackTypeForTests.Creator:
					var fbCreator = new FallbackPolicy();
					TestHandlingForInnerError.IncludeInnerErrorInPolicy<FallbackPolicy>(fbCreator, withInnerError, satisfyFilterFunc);
					policy = fbCreator;
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					var fbWithAsyncFunc = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
					TestHandlingForInnerError.IncludeInnerErrorInPolicy<FallbackPolicyWithAsyncFunc>(fbWithAsyncFunc, withInnerError, satisfyFilterFunc);
					policy = fbWithAsyncFunc;
					break;
				case FallbackTypeForTests.WithAction:
					var fbWithAction = new FallbackPolicy().WithFallbackAction((_) => { });
					TestHandlingForInnerError.IncludeInnerErrorInPolicy<FallbackPolicyWithAction>(fbWithAction, withInnerError, satisfyFilterFunc);
					policy = fbWithAction;
					break;
			}
			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			TestHandlingForInnerError.HandlePolicyWithIncludeInnerErrorFilter(policy, actionToHandle, withInnerError, satisfyFilterFunc);
		}

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass, true, true)]
		[TestCase(FallbackTypeForTests.BaseClass, true, false)]
		[TestCase(FallbackTypeForTests.BaseClass, true, null)]
		[TestCase(FallbackTypeForTests.BaseClass, false, null)]
		[TestCase(FallbackTypeForTests.Creator, true, true)]
		[TestCase(FallbackTypeForTests.Creator, true, false)]
		[TestCase(FallbackTypeForTests.Creator, true, null)]
		[TestCase(FallbackTypeForTests.Creator, false, null)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, null)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false, null)]
		[TestCase(FallbackTypeForTests.WithAction, true, true)]
		[TestCase(FallbackTypeForTests.WithAction, true, false)]
		[TestCase(FallbackTypeForTests.WithAction, true, null)]
		[TestCase(FallbackTypeForTests.WithAction, false, null)]
		public void Should_ExcludeInnerError_Work(FallbackTypeForTests fallbackType, bool withInnerError, bool? satisfyFilterFunc)
		{
			IPolicyBase policy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.BaseClass:
					var fbBase = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
					TestHandlingForInnerError.ExcludeInnerErrorFromPolicy(fbBase, withInnerError, satisfyFilterFunc);
					policy = fbBase;
					break;
				case FallbackTypeForTests.Creator:
					var fbCreator = new FallbackPolicy();
					TestHandlingForInnerError.ExcludeInnerErrorFromPolicy<FallbackPolicy>(fbCreator, withInnerError, satisfyFilterFunc);
					policy = fbCreator;
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					var fbWithAsyncFunc = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
					TestHandlingForInnerError.ExcludeInnerErrorFromPolicy<FallbackPolicyWithAsyncFunc>(fbWithAsyncFunc, withInnerError, satisfyFilterFunc);
					policy = fbWithAsyncFunc;
					break;
				case FallbackTypeForTests.WithAction:
					var fbWithAction = new FallbackPolicy().WithFallbackAction((_) => { });
					TestHandlingForInnerError.ExcludeInnerErrorFromPolicy<FallbackPolicyWithAction>(fbWithAction, withInnerError, satisfyFilterFunc);
					policy = fbWithAction;
					break;
			}
			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			TestHandlingForInnerError.HandlePolicyWithExcludeInnerErrorFilter(policy, actionToHandle, withInnerError, satisfyFilterFunc);
		}

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass)]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc)]
		[TestCase(FallbackTypeForTests.WithAction)]
		public void Should_Be_Correctly_Initialized_By_Constructor(FallbackTypeForTests fallbackType)
		{
			FallbackPolicyBase fallbackPolicy = null;
			var policyCreator = new FallbackPolicy(new DefaultFallbackProcessor(), true);
			switch (fallbackType)
			{
				case FallbackTypeForTests.BaseClass:
					fallbackPolicy = policyCreator.WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { });
					break;
				case FallbackTypeForTests.Creator:
					fallbackPolicy = policyCreator;
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					fallbackPolicy = policyCreator.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
					break;
				case FallbackTypeForTests.WithAction:
					fallbackPolicy = policyCreator.WithFallbackAction((_) => { });
					break;
			}

			Assert.That(fallbackPolicy.OnlyGenericFallbackForGenericDelegate, Is.True);
			Assert.That(fallbackPolicy.PolicyProcessor, Is.Not.Null);
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, true, true)]
		[TestCase(false, true, false)]
		[TestCase(false, false, true)]
		[TestCase(false, false, false)]
		public void Should_GenericFallbackFunc_Stay_Registered_After_NonGeneric_Func_WasRegistered(bool genericFuncSync, bool nonGenericFuncSync, bool funcWithToken)
		{
			FallbackPolicy fbPolicyWithGenericFallbackFunc = new FallbackPolicy();
			if (genericFuncSync)
			{
				fbPolicyWithGenericFallbackFunc.WithFallbackFunc(() => 1);
			}
			else
			{
				fbPolicyWithGenericFallbackFunc.WithAsyncFallbackFunc(async () => { await Task.Delay(1); return 1;});
			}

			if (nonGenericFuncSync)
			{
				if (funcWithToken)
				{
					fbPolicyWithGenericFallbackFunc.WithFallbackAction((_) => { });
				}
				else
				{
					fbPolicyWithGenericFallbackFunc.WithFallbackAction(() => { });
				}
			}
			else
			{
				if (funcWithToken)
				{
					fbPolicyWithGenericFallbackFunc.WithAsyncFallbackFunc(async (_) => await Task.Delay(1));
				}
				else
				{
					fbPolicyWithGenericFallbackFunc.WithAsyncFallbackFunc(async () => await Task.Delay(1));
				}
			}
			if (genericFuncSync)
			{
				Assert.That(fbPolicyWithGenericFallbackFunc.HasFallbackFunc<int>(), Is.True);
			}
			else
			{
				Assert.That(fbPolicyWithGenericFallbackFunc.HasAsyncFallbackFunc<int>(), Is.True);
			}
		}
	}
}