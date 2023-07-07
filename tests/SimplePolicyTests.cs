using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class SimplePolicyTests
	{
		[Test]
		public void Should_Handle_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			void action() => Expression.Empty();

			var res = simple.Handle(action);
			Assert.IsFalse(res.IsFailed);
			Assert.IsFalse(res.IsCanceled);
			Assert.IsFalse(res.Errors.Any());
			Assert.IsTrue(res.NoError);
		}

		[Test]
		public void Should_Handle_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.Execute(It.IsAny<Action>(), default)).Returns(new PolicyResult());

			var policy = new SimplePolicy(moqProcessor.Object);
			policy.Handle(() => { }, default);

			moqProcessor.Verify((t) => t.Execute(It.IsAny<Action>(), default), Times.Exactly(1));
		}

		[Test]
		public void Should_Work_For_Handle_Null_Delegate()
		{
			var simplePolTest = new SimplePolicy();
			var simpleResult = simplePolTest.Handle(null);
			Assert.IsTrue(simpleResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_Handle_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var wrapResult = simple.Handle(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_HandleT_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var wrapResult = simple.Handle<int>(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, wrapResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), wrapResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_HandleAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var retryResult = await simple.HandleAsync(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_SimplePolicy_Wrap_OtherPolicy_And_HandleTAsync_NullDelegate()
		{
			var simpleWrapped = new SimplePolicy();
			var simple = new SimplePolicy();
			simple.WrapPolicy(simpleWrapped);
			var retryResult = await simple.HandleAsync<int>(null);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_Handle_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var simple = new SimplePolicy();

			var wrappedPolicy = new Mock<IPolicyBase>();

			Action act = () => { };

			wrappedPolicy.Setup(t => t.Handle(act, default)).Returns(new PolicyResult());
			simple.WrapPolicy(wrappedPolicy.Object);

			var outPolicyResult = simple.Handle(act, default);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			wrappedPolicy.Verify((t) => t.Handle(act, default), Times.AtLeastOnce);
		}

		[Test]
		public void Should_HandleT_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.Execute(It.IsAny<Func<int>>(), default)).Returns(new PolicyResult<int>());

			var policy = new SimplePolicy(moqProcessor.Object);
			policy.Handle(() => 1, default);

			moqProcessor.Verify((t) => t.Execute(It.IsAny<Func<int>>(), default), Times.Exactly(1));
		}

		[Test]
		public void Should_HandleT_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			int func() => 4;

			var res = simple.Handle(func);
			Assert.IsFalse(res.IsFailed);
			Assert.IsFalse(res.IsCanceled);
			Assert.IsFalse(res.Errors.Any());
			Assert.IsTrue(res.NoError);
			Assert.AreEqual(4, res.Result);
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var retryPolTest = new SimplePolicy();
			var retryResult = retryPolTest.Handle<int>(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_HandleT_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var simple = new SimplePolicy();

			var wrappedPolicy = new Mock<IPolicyBase>();
			var cancelToken = new CancellationToken();

			Func<int> func = () => throw new Exception();

			var polResult = new PolicyResult<int>();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			wrappedPolicy.Setup(t => t.Handle(func, cancelToken)).Returns(polResult);
			simple.WrapPolicy(wrappedPolicy.Object);

			var outPolicyResult = simple.Handle(func, cancelToken);

			//Out policy error should be the same as wrapped policy error.
			Assert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			Assert.IsFalse(outPolicyResult.IsFailed);
			Assert.AreEqual(1, outPolicyResult.Errors.Count());

			wrappedPolicy.Verify((t) => t.Handle(func, cancelToken), Times.Exactly(1));
		}

		[Test]
		public async Task Should_HandleAsync_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			async Task action(CancellationToken _) { await Task.Delay(1); }

			var res = await simple.HandleAsync(action);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.NoError);
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var simplePolTest = new SimplePolicy();
			var retryResult = await simplePolTest.HandleAsync(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, retryResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_HandleAsync_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.ExecuteAsync(It.IsAny<Func<CancellationToken,Task>>(), false, default)).ReturnsAsync(new PolicyResult());

			var policy = new SimplePolicy(moqProcessor.Object);
			await policy.HandleAsync(async (_) => await Task.Delay(1), default);

			moqProcessor.Verify((t) => t.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), false, default), Times.Exactly(1));
		}

		[Test]
		public async Task Should_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var simple = new SimplePolicy();

			var wrappedPolicy = new Mock<IPolicyBase>();

			Func<CancellationToken, Task> func = async (_) => await Task.Delay(1);

			wrappedPolicy.Setup(t => t.HandleAsync(func, default, default)).Returns(Task.FromResult(new PolicyResult()));
			simple.WrapPolicy(wrappedPolicy.Object);

			var outPolicyResult = await simple.HandleAsync(func, default);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			wrappedPolicy.Verify((t) => t.HandleAsync(func, default, default), Times.AtLeastOnce);
		}

		[Test]
		public async Task Should_HandleAsyncT_Work_When_NoError()
		{
			var simple = new SimplePolicy();
			async Task<int> action(CancellationToken _) { await Task.Delay(1); return 4; }

			var res = await simple.HandleAsync(action);

			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.NoError);
			Assert.AreEqual(4, res.Result);
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var simple = new SimplePolicy();
			var res = await simple.HandleAsync<int>(null);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, res.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), res.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_HandleAsyncT_CallSimplePolicyProcessor_If_No_Wrap()
		{
			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<int>>>(), false, default)).ReturnsAsync(new PolicyResult<int>());

			var policy = new SimplePolicy(moqProcessor.Object);
			await policy.HandleAsync(async (_) => { await Task.Delay(1); return 1; }, default);

			moqProcessor.Verify((t) => t.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<int>>>(), false, default), Times.Exactly(1));
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var simple = new SimplePolicy();

			var wrappedPolicy = new Mock<IPolicyBase>();

			Func<CancellationToken, Task<int>> func = async (_) => { await Task.Delay(1); return 1; };

			wrappedPolicy.Setup(t => t.HandleAsync(func, default, default)).Returns(Task.FromResult(new PolicyResult<int>()));
			simple.WrapPolicy(wrappedPolicy.Object);

			var outPolicyResult = await simple.HandleAsync(func, default);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			wrappedPolicy.Verify((t) => t.HandleAsync(func, default, default), Times.Exactly(1));
		}

		[TestCase("Test")]
		public void Should_GenericExcludeFilterWork(string errParamName)
		{
			var retryPolTest = new SimplePolicy();
			retryPolTest.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == "Test");
			void actionUnsatisied() => throw new ArgumentNullException(errParamName);
			var res = retryPolTest.Handle(actionUnsatisied);
			Assert.IsTrue(res.ErrorFilterUnsatisfied);
			Assert.IsTrue(res.IsFailed);
		}

		[Test]
		public void Should_GenericIncludeFilterError_Work()
		{
			var retryPolTest = new SimplePolicy();
			void actionUnsatisied() => throw new Exception("Test");
			retryPolTest.IncludeError<ArgumentNullException>();
			var res = retryPolTest.Handle(actionUnsatisied);
			Assert.IsTrue(res.ErrorFilterUnsatisfied);
			Assert.IsTrue(res.IsFailed);
		}

		[Test]
		public void Should_ExludedErrorFilter_Work()
		{
			var simplePolicy = new SimplePolicy();
			var fbWithError = simplePolicy.ExcludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test");
			var polRes = fbWithError.Handle(action);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_IncludedErrorFilter_Work()
		{
			var simplePolicy = new SimplePolicy();
			var fbWithError = simplePolicy.IncludeError((e) => e.Message == "Test");
			void action() => throw new Exception("Test2");
			var polRes = fbWithError.Handle(action);
			Assert.IsTrue(polRes.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_InvokeParams_ToSimplePolicy_Work()
		{
			InvokeParams invokeParamsNull = null;
			var policyFromNull = invokeParamsNull.ToSimplePolicy();
			Assert.IsNotNull(policyFromNull);
			Assert.IsInstanceOf<SimplePolicy>(policyFromNull);
			Assert.AreEqual(0, policyFromNull.PolicyProcessor.Count());

			InvokeParams invokeParamsFromAction = InvokeParams.From((_, __) => Expression.Empty());
			var policyFromAction = invokeParamsFromAction.ToSimplePolicy();
			Assert.IsNotNull(policyFromAction);
			Assert.IsInstanceOf<SimplePolicy>(policyFromAction);
			Assert.AreEqual(1, policyFromAction.PolicyProcessor.Count());
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
			policy.Handle(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
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
			policy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(1, i);
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
			await policy.HandleAsync(async (_) => {await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
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
			await policy.HandleAsync<int>(async(_) => {await Task.Delay(1); throw new Exception("Handle"); });
			Assert.AreEqual(1, i);
		}
	}
}
