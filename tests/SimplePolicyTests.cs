using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
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
		}
	}
}
