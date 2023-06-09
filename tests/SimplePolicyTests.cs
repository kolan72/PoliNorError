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
		public void Should_Handle_Work_When_Ok()
		{
			var simple = new SimplePolicy();
			void action() => Expression.Empty();

			var res = simple.Handle(action);
			Assert.IsFalse(res.IsFailed);
			Assert.IsFalse(res.IsCanceled);
			Assert.IsFalse(res.Errors.Any());
			Assert.IsTrue(res.IsOk);
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
			var retryPolTest = new SimplePolicy();
			var retryResult = retryPolTest.Handle(null);
			Assert.IsTrue(retryResult.IsFailed);
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
		public void Should_HandleT_Work_When_Ok()
		{
			var simple = new SimplePolicy();
			int func() => 4;

			var res = simple.Handle(func);
			Assert.IsFalse(res.IsFailed);
			Assert.IsFalse(res.IsCanceled);
			Assert.IsFalse(res.Errors.Any());
			Assert.IsTrue(res.IsOk);
			Assert.AreEqual(4, res.Result);
		}

		[Test]
		public void Should_Work_For_HandleT_Null_Delegate()
		{
			var retryPolTest = new SimplePolicy();
			var retryResult = retryPolTest.Handle<int>(null);
			Assert.IsTrue(retryResult.IsFailed);
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
			polResult.SetFailed();
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
		public async Task Should_HandleAsync_Work_When_Ok()
		{
			var simple = new SimplePolicy();
			async Task action(CancellationToken _) { await Task.Delay(1); }

			var res = await simple.HandleAsync(action);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.IsOk);
		}

		[Test]
		public async Task Should_Work_For_HandleAsync_Null_Delegate()
		{
			var simplePolTest = new SimplePolicy();
			var retryResult = await simplePolTest.HandleAsync(null);
			Assert.IsTrue(retryResult.IsFailed);
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
		public async Task Should_HandleAsyncT_Work_When_Ok()
		{
			var simple = new SimplePolicy();
			async Task<int> action(CancellationToken _) { await Task.Delay(1); return 4; }

			var res = await simple.HandleAsync(action);

			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(false, res.IsCanceled);
			Assert.AreEqual(false, res.Errors.Any());
			Assert.IsTrue(res.IsOk);
			Assert.AreEqual(4, res.Result);
		}

		[Test]
		public async Task Should_Work_For_HandleAsyncT_Null_Delegate()
		{
			var simple = new SimplePolicy();
			var res = await simple.HandleAsync<int>(null);
			Assert.IsTrue(res.IsFailed);
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
	}
}
