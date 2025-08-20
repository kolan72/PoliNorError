using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class SimplePolicyTests
	{
		[Test]
		public void Should_Handle_Work_When_NoError()
		{
			const string polName = "StupidPolicy";
			var simple = new SimplePolicy().WithPolicyName(polName);
			void action() => Expression.Empty();

			var res = simple.Handle(action);
			ClassicAssert.IsFalse(res.IsFailed);
			ClassicAssert.IsFalse(res.IsCanceled);
			ClassicAssert.IsFalse(res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(polName, simple.PolicyName);
			ClassicAssert.AreEqual(simple.PolicyName, res.PolicyName);
		}

		[Test]
		public void Should_Handle_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var subsProcessor = Substitute.For<ISimplePolicyProcessor>();
			subsProcessor.Execute(Arg.Any<Action>()).Returns(new PolicyResult());
			var policy = new SimplePolicy(subsProcessor);
			policy.Handle(() => { }, default);

			subsProcessor.Received(1).Execute(Arg.Any<Action>());
		}

		[Test]
		public void Should_Work_For_Handle_Null_Delegate()
		{
			var simplePolTest = new SimplePolicy();
			var simpleResult = simplePolTest.Handle(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.NotNull(simpleResult.UnprocessedError);
			ClassicAssert.AreEqual(simplePolTest.PolicyName, simpleResult.PolicyName);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_Handle_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var wrapResult = simple.Handle(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_HandleT_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var wrapResult = simple.Handle<int>(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_HandleAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var retryResult = await simple.HandleAsync(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_HandleTAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var retryResult = await simple.HandleAsync<int>(null);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_Handle_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var simple = new SimplePolicy();

			var subsWrappedPolicy = Substitute.For<IPolicyBase>();

			void act()
			{
				// Method intentionally left empty.
			}

			subsWrappedPolicy.Handle(act).Returns(new PolicyResult());

			simple.WrapPolicy(subsWrappedPolicy);

			var outPolicyResult = simple.Handle(act, default);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			subsWrappedPolicy.Received(1).Handle(act);
		}

		[Test]
		public void Should_HandleT_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var subsProcessor = Substitute.For<ISimplePolicyProcessor>();
			subsProcessor.Execute(Arg.Any<Func<int>>()).Returns(new PolicyResult<int>());
			var policy = new SimplePolicy(subsProcessor);
			policy.Handle(() => 1, default);

			subsProcessor.Received(1).Execute(Arg.Any<Func<int>>());
		}

		[Test]
		public void Should_HandleT_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			int func() => 4;

			var res = simple.Handle(func);
			ClassicAssert.IsFalse(res.IsFailed);
			ClassicAssert.IsFalse(res.IsCanceled);
			ClassicAssert.IsFalse(res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(4, res.Result);
			ClassicAssert.AreEqual(simple.PolicyName, res.PolicyName);
		}

		[Test]
		public void Should_HandleT_Work_When_Error()
		{
			var simple = new SimplePolicy();
			int func() => throw new Exception();

			var res = simple.Handle(func);
			ClassicAssert.IsFalse(res.IsFailed);
			ClassicAssert.IsTrue(res.IsSuccess);
			ClassicAssert.IsTrue(res.Errors.Any());
			ClassicAssert.IsFalse(res.NoError);
			ClassicAssert.IsTrue(res.IsPolicySuccess);
			ClassicAssert.AreEqual(0, res.Result);
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var retryPolTest = new SimplePolicy();
			var simpleResult = retryPolTest.Handle<int>(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.NotNull(simpleResult.UnprocessedError);
			ClassicAssert.AreEqual(retryPolTest.PolicyName, simpleResult.PolicyName);
		}

		[Test]
		public void Should_HandleT_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var simple = new SimplePolicy();

			var subsWrappedPolicy = Substitute.For<IPolicyBase>();

			int func() => throw new Exception();

			var polResult = new PolicyResult<int>();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));
			subsWrappedPolicy.Handle(func).Returns(polResult);

			simple.WrapPolicy(subsWrappedPolicy);

			var outPolicyResult = simple.Handle(func, default);

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			ClassicAssert.IsFalse(outPolicyResult.IsFailed);
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count());

			subsWrappedPolicy.Received(1).Handle(func, default);
		}

		[Test]
		public async Task Should_HandleAsync_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			async Task action(CancellationToken _) { await Task.Delay(1); }

			var res = await simple.HandleAsync(action);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(false, res.IsCanceled);
			ClassicAssert.AreEqual(false, res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(simple.PolicyName, res.PolicyName);
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var simplePolTest = new SimplePolicy();
			var simpleResult = await simplePolTest.HandleAsync(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.NotNull(simpleResult.UnprocessedError);
			ClassicAssert.AreEqual(simplePolTest.PolicyName, simpleResult.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsync_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var subsProcessor = Substitute.For<ISimplePolicyProcessor>();
			subsProcessor.ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>()).Returns(new PolicyResult());
			var policy = new SimplePolicy(subsProcessor);
			await policy.HandleAsync(async (_) => await Task.Delay(1), default);

			await subsProcessor.Received(1).ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>());
		}

		[Test]
		public async Task Should_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var simple = new SimplePolicy();

			var subsWrappedPolicy = Substitute.For<IPolicyBase>();
			async Task func(CancellationToken _) => await Task.Delay(1);
			subsWrappedPolicy.HandleAsync(func).Returns(Task.FromResult(new PolicyResult()));

			simple.WrapPolicy(subsWrappedPolicy);

			var outPolicyResult = await simple.HandleAsync(func);
			await subsWrappedPolicy.Received(1).HandleAsync(func);

			ClassicAssert.AreEqual(simple.PolicyName, outPolicyResult.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			async Task<int> action(CancellationToken _) { await Task.Delay(1); return 4; }

			var res = await simple.HandleAsync(action);

			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(false, res.IsCanceled);
			ClassicAssert.AreEqual(false, res.Errors.Any());
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(4, res.Result);
			ClassicAssert.AreEqual(simple.PolicyName, res.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_Work_When_Error()
		{
			var simple = new SimplePolicy();
			async Task<int> action(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			var res = await simple.HandleAsync(action);

			ClassicAssert.IsFalse(res.IsFailed);
			ClassicAssert.IsTrue(res.IsSuccess);
			ClassicAssert.IsTrue(res.Errors.Any());
			ClassicAssert.IsFalse(res.NoError);
			ClassicAssert.AreEqual(0, res.Result);
			ClassicAssert.AreEqual(simple.PolicyName, res.PolicyName);
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var simple = new SimplePolicy();
			var simpleResult = await simple.HandleAsync<int>(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
			ClassicAssert.NotNull(simpleResult.UnprocessedError);
			ClassicAssert.AreEqual(simple.PolicyName, simpleResult.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var subsProcessor = Substitute.For<ISimplePolicyProcessor>();
			subsProcessor.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<int>>>()).Returns(new PolicyResult<int>());
			var policy = new SimplePolicy(subsProcessor);
			await policy.HandleAsync(async (_) => { await Task.Delay(1); return 1; });

			await subsProcessor.Received(1).ExecuteAsync(Arg.Any<Func<CancellationToken, Task<int>>>());
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var simple = new SimplePolicy();

			var subsWrappedPolicy = Substitute.For<IPolicyBase>();
			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 1; }
			subsWrappedPolicy.HandleAsync(func).Returns(Task.FromResult(new PolicyResult<int>()));

			simple.WrapPolicy(subsWrappedPolicy);

			var outPolicyResult = await simple.HandleAsync(func);
			await subsWrappedPolicy.Received(1).HandleAsync(func);

			ClassicAssert.AreEqual(simple.PolicyName, outPolicyResult.PolicyName);
		}

		[TestCase("Test")]
		public void Should_GenericExcludeFilterWork(string errParamName)
		{
			var retryPolTest = new SimplePolicy();
			retryPolTest.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			void actionUnsatisied() => throw new ArgumentNullException(errParamName);
			var res = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.IsTrue(res.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.NotNull(res.UnprocessedError);
		}

		[Test]
		public void Should_GenericIncludeFilterError_Work()
		{
			var retryPolTest = new SimplePolicy();
			void actionUnsatisied() => throw new Exception("Test");
			retryPolTest.IncludeError<ArgumentNullException>();
			var res = retryPolTest.Handle(actionUnsatisied);
			ClassicAssert.IsTrue(res.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.NotNull(res.UnprocessedError);
		}

		[Test]
		public void Should_ExludedErrorFilter_Work()
		{
			var simplePolicy = new SimplePolicy();
			var fbWithError = simplePolicy.ExcludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test");
			var res = fbWithError.Handle(action);
			ClassicAssert.IsTrue(res.ErrorFilterUnsatisfied);
			ClassicAssert.NotNull(res.UnprocessedError);
		}

		[Test]
		public void Should_IncludedErrorFilter_Work()
		{
			var simplePolicy = new SimplePolicy();
			var fbWithError = simplePolicy.IncludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test2");
			var res = fbWithError.Handle(action);
			ClassicAssert.IsTrue(res.ErrorFilterUnsatisfied);
			ClassicAssert.NotNull(res.UnprocessedError);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var retryPolTest = new SimplePolicy();
			retryPolTest.IncludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(retryPolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var simplePolTest = new SimplePolicy();
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = simplePolTest.IncludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(simplePolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false, true)]
		[TestCase(TestErrorSetMatch.NoMatch, true, false)]
		[TestCase(TestErrorSetMatch.FirstParam, false, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false, false)]
		public void Should_IncludeErrorSet_ForInnerExceptions_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed, bool consistsOfErrorAndInnerError)
		{
			var simplePolTest = new SimplePolicy();
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
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var retryPolTest = new SimplePolicy();
			retryPolTest.ExcludeErrorSet<ArgumentException, ArgumentNullException>();
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(retryPolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var simplePolTest = new SimplePolicy();
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			_ = simplePolTest.ExcludeErrorSet(errorSet);
			var res = TestHandlingForErrorSet.HandlePolicyWithErrorSet(simplePolTest, testErrorSetMatch);
			ClassicAssert.AreEqual(isFailed, res.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true, true)]
		[TestCase(TestErrorSetMatch.NoMatch, false, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true, false)]
		[TestCase(TestErrorSetMatch.SecondParam, true, false)]
		public void Should_ExcludeErrorSet_ForInnerExceptions_WithTwoGenericParams_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed, bool consistsOfErrorAndInnerError)
		{
			var simplePolTest = new SimplePolicy();
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
			ClassicAssert.AreEqual(isFailed, res.IsFailed);
		}

		[Test]
		public void Should_InvokeParams_ToSimplePolicy_Work()
		{
			ErrorProcessorParam invokeParamsNull = null;
			var policyFromNull = invokeParamsNull.ToSimplePolicy();
			ClassicAssert.IsNotNull(policyFromNull);
			ClassicAssert.IsInstanceOf<SimplePolicy>(policyFromNull);
			ClassicAssert.AreEqual(0, policyFromNull.PolicyProcessor.Count());

			ErrorProcessorParam invokeParamsFromAction = ErrorProcessorParam.From((_, __) => Expression.Empty());
			var policyFromAction = invokeParamsFromAction.ToSimplePolicy();
			ClassicAssert.IsNotNull(policyFromAction);
			ClassicAssert.IsInstanceOf<SimplePolicy>(policyFromAction);
			ClassicAssert.AreEqual(1, policyFromAction.PolicyProcessor.Count());
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddPolicyResultHandler_By_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new SimplePolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler((_) => i++);
			}
			var polResult = policy.Handle(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(policy.PolicyName, polResult.PolicyName);
			ClassicAssert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddPolicyResultHandler_By_Generic_Action_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new SimplePolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>((_, __) => i++);
			}
			else
			{
				policy.AddPolicyResultHandler<int>((_) => i++);
			}
			var res = policy.Handle<int>(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
			ClassicAssert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandler_By_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new SimplePolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });
			}
			var res = await policy.HandleAsync(async (_) => {await Task.Delay(1); throw new Exception("Handle"); });
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
			ClassicAssert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandler_By_Generic_AsyncFunc_Work(bool withCancelTokenParam)
		{
			int i = 0;
			var policy = new SimplePolicy();
			if (withCancelTokenParam)
			{
				policy.AddPolicyResultHandler<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				policy.AddPolicyResultHandler<int>(async (_) => { await Task.Delay(1); i++; });
			}
			var res = await policy.HandleAsync<int>(async(_) => {await Task.Delay(1); throw new Exception("Handle"); });
			ClassicAssert.AreEqual(policy.PolicyName, res.PolicyName);
			ClassicAssert.AreEqual(1, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_SetPolicyResultFailedIf_Work(bool generic)
		{
			var policy = new SimplePolicy();
			PolicyResult polResult = null;
			if (generic)
			{
				polResult = policy.SetPolicyResultFailedIf<int>(pr => pr.Errors.Any(e => e.Message == "Test"))
					.Handle<int>(() => throw new ArgumentException("Test"));
			}
			else
			{
				polResult = policy.SetPolicyResultFailedIf(pr => pr.Errors.Any(e => e.Message == "Test"))
					.Handle(() => throw new ArgumentException("Test"));
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
			var policy = new SimplePolicy();
			PolicyResult<int> polResult = null;
			bool continueThrow = true;
			bool handlerFlag = false;
			void act(PolicyResult<int> _) => handlerFlag = true;

			using (var cts = new CancellationTokenSource())
			{
				polResult = policy.SetPolicyResultFailedIf<int>(pr => pr.Errors.Any(e => e.Message == "Test"), act)
					.Handle(() =>
					{
						if (continueThrow)
						{
							if (!setIsFailedByPolicyResultHandler)
							{
								cts.Cancel();
							}
							continueThrow = !setIsFailedByPolicyResultHandler;
							throw new ArgumentException("Test");
						}
						return 1;
					}, cts.Token);
			}

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
			var policy = new SimplePolicy();
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
			var policy = new SimplePolicy();
			TestHandlingForInnerError.ExcludeInnerErrorFromPolicy(policy, withInnerError, satisfyFilterFunc);

			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			TestHandlingForInnerError.HandlePolicyWithExcludeInnerErrorFilter(policy, actionToHandle, withInnerError, satisfyFilterFunc);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Rethrow_Or_Handle_If_PolicyCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForHandle(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = proc.Handle(ActionWithInner);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>();
				var exception = Assert.Throws<TestExceptionWithInnerException>(() => proc.Handle(ActionWithInner));
				Assert.That(exception.DataContainsKeyStringWithValue(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY, true), Is.True);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Rethrow_Or_Handle_If_PolicyCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForHandleAsync(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = await proc.HandleAsync(AsyncFuncWithInner);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>();
				var exception = Assert.ThrowsAsync<TestExceptionWithInnerException>(async () => await proc.HandleAsync(AsyncFuncWithInner));
				Assert.That(exception.DataContainsKeyStringWithValue(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY, true), Is.True);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Rethrow_Or_Handle_If_PolicyCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForHandleT(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = proc.Handle(FuncWithInner);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>();
				var exception = Assert.Throws<TestExceptionWithInnerException>(() => proc.Handle(FuncWithInner));
				Assert.That(exception.DataContainsKeyStringWithValue(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY, true), Is.True);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Rethrow_Or_Handle_If_PolicyCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForHandleAsyncT(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = await proc.HandleAsync(AsyncFuncWithInnerT);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicy(true).ExcludeError<TestExceptionWithInnerException>();
				var exception = Assert.ThrowsAsync<TestExceptionWithInnerException>(async () => await proc.HandleAsync(AsyncFuncWithInnerT));
				Assert.That(exception.DataContainsKeyStringWithValue(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY, true), Is.True);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WithErrorContextProcessorOf_Action_Throws_For_Not_SimplePolicyProcessor(bool withCancellationType)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var simplePolicy = new SimplePolicy(new TestSimplePolicyProcessor());
			if (withCancellationType)
			{
				Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessorOf<int>(action, CancellationType.Precancelable));
			}
			else
			{
				Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessorOf<int>(action));
			}
		}

		[Test]
		public void Should_WithErrorProcessorOf_Action_With_Token_Throws_For_Not_SimplePolicyProcessor()
		{
			void action(Exception _, ProcessingErrorInfo<int> __, CancellationToken ___)
			{
				// Method intentionally left empty.
			}

			var simplePolicy = new SimplePolicy(new TestSimplePolicyProcessor());
			Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessorOf<int>(action));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WithErrorProcessorOf_AsyncFunc_Throws_For_Not_SimplePolicyProcessor(bool withCancellationType)
		{
			async Task fn(Exception _, ProcessingErrorInfo<int> __)
			{
				await Task.Delay(1);
			}
			var simplePolicy = new SimplePolicy(new TestSimplePolicyProcessor());
			if (withCancellationType)
			{
				Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessorOf<int>(fn, CancellationType.Precancelable));
			}
			else
			{
				Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessorOf<int>(fn));
			}
		}

		[Test]
		public void Should_WithErrorProcessorOf_AsyncFunc_With_Token_Throws_For_Not_SimplePolicyProcessor()
		{
			async Task fn(Exception _, ProcessingErrorInfo<int> __, CancellationToken ___)
			{
				await Task.Delay(1);
			}
			var simplePolicy = new SimplePolicy(new TestSimplePolicyProcessor());

			Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessorOf<int>(fn));
		}

		[Test]
		[TestCase(true, null)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_WithErrorContextProcessor_Throws_Only_For_Not_SimplePolicyProcessor(bool throwEx, bool? wrap)
		{
			SimplePolicy simplePolicy;
			if (throwEx)
			{
				simplePolicy = new SimplePolicy(new TestSimplePolicyProcessor());
				Assert.Throws<NotImplementedException>(() => simplePolicy.WithErrorContextProcessor(new DefaultErrorProcessor<int>((_, __) => { })));
			}
			else
			{
				int m = 0;

				void action(Exception _, ProcessingErrorInfo<int> pi)
				{
					m = pi.Param;
				}

				simplePolicy = new SimplePolicy()
								.WithErrorContextProcessor(new DefaultErrorProcessor<int>(action));

				if (wrap == true)
				{
					simplePolicy = simplePolicy.WrapPolicy(new RetryPolicy(1));
				}

				var result = simplePolicy
							.Handle(() => throw new InvalidOperationException(), 5);

				Assert.That(result.NoError, Is.False);
				Assert.That(result.IsSuccess, Is.True);

				Assert.That(m, Is.EqualTo(5));
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_Handle_For_Action_With_Generic_Param_WithErrorProcessorOf_Action_Process_Correctly(bool shouldWork, bool withCancellationType)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			SimplePolicy policy;

			if (!withCancellationType)
			{
				policy = new SimplePolicy(true)
							.WithErrorContextProcessorOf<int>(action);
			}
			else
			{
				policy = new SimplePolicy(true)
						.WithErrorContextProcessorOf<int>(action, CancellationType.Precancelable);
			}

			PolicyResult result = null;

			if (shouldWork)
			{
				result = policy.Handle(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = policy.Handle(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Handle_For_Action_With_Generic_Param_WithErrorProcessorOf_Action_With_Token_Param_Process_Correctly(bool shouldWork)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				m = pi.Param;
			}

			var policy = new SimplePolicy(true)
						.WithErrorContextProcessorOf<int>(action);

			PolicyResult result;

			if (shouldWork)
			{
				result = policy.Handle(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = policy.Handle(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_Handle_For_Action_With_Generic_Param_WithErrorProcessorOf_AsyncFunc_Process_Correctly(bool shouldWork, bool withCancellationType)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi)
			{
				await Task.Delay(1);
				m = pi.Param;
			}

			SimplePolicy policy;

			if (!withCancellationType)
			{
				policy = new SimplePolicy(true)
							.WithErrorContextProcessorOf<int>(fn);
			}
			else
			{
				policy = new SimplePolicy(true)
							.WithErrorContextProcessorOf<int>(fn, CancellationType.Precancelable);
			}

			PolicyResult result = null;

			if (shouldWork)
			{
				result = policy.Handle(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = policy.Handle(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Handle_For_Action_With_Generic_Param_WithErrorProcessorOf_AsyncFunc_With_Token_Param_Process_Correctly(bool shouldWork)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				await Task.Delay(1);
				m = pi.Param;
			}

			var policy = new SimplePolicy(true)
						.WithErrorContextProcessorOf<int>(fn);

			PolicyResult result;

			if (shouldWork)
			{
				result = policy.Handle(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = policy.Handle(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, null)]
		public void Should_Handle_With_TParam_For_Action_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool? useWrap)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var policyToTest = new SimplePolicy(true).WithErrorContextProcessorOf<int>(action);

			if (useWrap == true)
			{
				policyToTest = policyToTest.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = policyToTest.Handle((_) => throw new InvalidOperationException(), 5);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap == true ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
#pragma warning disable RCS1021 // Convert lambda expression body to expression-body.
				result = policyToTest.Handle((v) => { addable += v; }, 5);
#pragma warning restore RCS1021 // Convert lambda expression body to expression-body.
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_Handle_With_TParam_For_Func_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool wrap)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			SimplePolicy policy = new SimplePolicy(true)
							.WithErrorContextProcessorOf<int>(action);
			if (wrap)
			{
				policy = policy.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = policy.Handle<int, int>(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = policy.Handle(() => 1, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, null)]
		public void Should_Handle_With_TParam_For_Func_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool? useWrap)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var policyToTest = new SimplePolicy(true).WithErrorContextProcessorOf<int>(action);

			if (useWrap == true)
			{
				policyToTest = policyToTest.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = policyToTest.Handle<int, int>((_) => throw new InvalidOperationException(), 5);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap == true ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = policyToTest.Handle((v) => { addable += v; return addable; }, 5);
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(null, true)]
		public async Task Should_HandleAsync_With_TParam_For_NonGeneric_AsyncFunc_WithErrorProcessorOf_Action_Process_Correctly(bool? throwEx, bool notDefaultImpl)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			SimplePolicy policy;

			if (notDefaultImpl)
			{
				policy = new SimplePolicy(new TestSimplePolicyProcessor());
				Assert.ThrowsAsync<NotImplementedException>(async() => await policy.HandleAsync(async (_) => await Task.Delay(1), 5, false, default));
			}
			else
			{
				policy = new SimplePolicy(true)
							.WithErrorContextProcessorOf<int>(action);

				PolicyResult result = null;
				if (throwEx == true)
				{
					result = await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);
					Assert.That(m, Is.EqualTo(5));
					Assert.That(result.NoError, Is.False);
				}
				else
				{
					result = await policy.HandleAsync(async (_) => await Task.Delay(1), 5, false, default);
					Assert.That(m, Is.EqualTo(0));
					Assert.That(result.NoError, Is.True);
				}
				Assert.That(result.IsSuccess, Is.True);
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, null)]
		public async Task Should_HandleAsync_With_TParam_For_AsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool? useWrap)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var policyToTest = new SimplePolicy(true).WithErrorContextProcessorOf<int>(action);

			if (useWrap == true)
			{
				policyToTest = policyToTest.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = await policyToTest.HandleAsync(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap == true ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await policyToTest.HandleAsync(async (v, _) => { await Task.Delay(1); addable += v; }, 5, false, default);
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(null, true)]
		public async Task Should_HandleAsync_With_TParam_For_Generic_AsyncFunc_WithErrorProcessorOf_Action_Process_Correctly(bool? throwEx, bool notDefaultImpl)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			SimplePolicy policy;

			if (notDefaultImpl)
			{
				policy = new SimplePolicy(new TestSimplePolicyProcessor());
				Assert.ThrowsAsync<NotImplementedException>(async () => await policy.HandleAsync(async (_) => { await Task.Delay(1); return 1; }, 5, false, default));
			}
			else
			{
				policy = new SimplePolicy(true)
							.WithErrorContextProcessorOf<int>(action);

				PolicyResult result = null;
				if (throwEx == true)
				{
					result = await policy.HandleAsync<int, int>(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);
					Assert.That(m, Is.EqualTo(5));
					Assert.That(result.NoError, Is.False);
				}
				else
				{
					result = await policy.HandleAsync(async (_) => { await Task.Delay(1); return 1; }, 5, false, default);
					Assert.That(m, Is.EqualTo(0));
					Assert.That(result.NoError, Is.True);
				}
				Assert.That(result.IsSuccess, Is.True);
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, null)]
		public async Task Should_HandleAsync_With_TParam_For_GenericAsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx, bool? useWrap)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var policyToTest = new SimplePolicy(true).WithErrorContextProcessorOf<int>(action);

			if (useWrap == true)
			{
				policyToTest = policyToTest.WrapPolicy(new RetryPolicy(1));
			}

			PolicyResult result = null;
			if (throwEx)
			{
				result = await policyToTest.HandleAsync<int, int>(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, false, default);
				//With wrapping, we fallback to no-param handling
				Assert.That(m, useWrap == true ? Is.EqualTo(0) : Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await policyToTest.HandleAsync(async (v, _) => { await Task.Delay(1); addable += v; return addable; }, 5, false, default);
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Then_Fallback_Returns_Valid_Result(bool isZero)
		{
			var fallbackErrorProcessorFlag = false;
			var simpleErrorProcessorFlag = false;
			var zero = isZero ? 0 : 5;

			Func<int> funcToHandle;
			if (isZero)
			{
				funcToHandle = () => 5 / zero;
			}
			else
			{
				funcToHandle = () => throw new InvalidOperationException();
			}

			var fallbackResult = new SimplePolicy()
									.ExcludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => simpleErrorProcessorFlag = true)
									.ThenFallback()
									.WithFallbackFunc(() => int.MaxValue)
									.IncludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => fallbackErrorProcessorFlag = true)
									.Handle(funcToHandle);
			if (isZero)
			{
				Assert.That(fallbackResult.Result, Is.EqualTo(int.MaxValue));
				Assert.That(fallbackResult.Errors.Count(), Is.EqualTo(1));
				Assert.That(fallbackResult.Errors.FirstOrDefault()?.GetType(), Is.EqualTo(typeof(DivideByZeroException)));
				Assert.That(fallbackResult.WrappedPolicyResults.LastOrDefault()?.Result.ErrorFilterUnsatisfied, Is.True);
				Assert.That(fallbackResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Failed));
				Assert.That(simpleErrorProcessorFlag, Is.False);
				Assert.That(fallbackErrorProcessorFlag, Is.True);			}
			else
			{
				Assert.That(fallbackResult.Result, Is.EqualTo(default(int)));
				Assert.That(fallbackResult.NoError, Is.True);
				Assert.That(fallbackResult.WrappedPolicyResults.LastOrDefault()?.Result.Errors.FirstOrDefault(), Is.TypeOf<InvalidOperationException>());
				Assert.That(fallbackResult.WrappedPolicyResults.LastOrDefault()?.Result.NoError, Is.False);
				Assert.That(fallbackResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.PolicySuccess));
				Assert.That(simpleErrorProcessorFlag, Is.True);
				Assert.That(fallbackErrorProcessorFlag, Is.False);
			}
			Assert.That(fallbackResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		public class TestSimplePolicyProcessor : ISimplePolicyProcessor
		{
			public PolicyProcessor.ExceptionFilter ErrorFilter => throw new NotImplementedException();

			public void AddErrorProcessor(IErrorProcessor newErrorProcessor) => Expression.Empty();
			public PolicyResult Execute(Action action, CancellationToken token = default) => throw new NotImplementedException();
			public PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default) => throw new NotImplementedException();
			public Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default) => throw new Exception();
			public Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default) => throw new NotImplementedException();
			public IEnumerator<IErrorProcessor> GetEnumerator() => throw new NotImplementedException();
			IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
		}
	}
}
