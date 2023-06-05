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
		public void Should_Handle_CanNotHandleIfTokenCanceledAlready()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.Execute(It.IsAny<Action>(), cancelTokenSource.Token));

			var policy = new SimplePolicy();
			var res = policy.Handle(() => { }, cancelTokenSource.Token);

			Assert.AreEqual(true, res.IsCanceled);
			Assert.AreEqual(false, res.IsFailed);
			Assert.IsFalse(res.IsSuccess);
			Assert.IsFalse(res.IsOk);

			moqProcessor.Verify((t) => t.Execute(It.IsAny<Action>(), cancelTokenSource.Token), Times.Never);
			cancelTokenSource.Dispose();
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
		public void Should_HandleT_CanNotHandleIfTokenCanceledAlready()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.Execute(It.IsAny<Func<int>>(), cancelTokenSource.Token));

			var policy = new SimplePolicy();
			var res = policy.Handle(() => 1, cancelTokenSource.Token);

			Assert.AreEqual(true, res.IsCanceled);
			Assert.AreEqual(false, res.IsFailed);
			Assert.IsFalse(res.IsSuccess);
			Assert.IsFalse(res.IsOk);

			moqProcessor.Verify((t) => t.Execute(It.IsAny<Func<int>>(), cancelTokenSource.Token), Times.Never);
			cancelTokenSource.Dispose();
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
			var retryPolTest = new SimplePolicy();
			var retryResult = await retryPolTest.HandleAsync(null);
			Assert.IsTrue(retryResult.IsFailed);
			Assert.AreEqual(typeof(NoDelegateException), retryResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_HandleAsync_CanNotHandleIfTokenCanceledAlready()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			var moqProcessor = new Mock<ISimplePolicyProcessor>();

			moqProcessor.Setup((t) => t.ExecuteAsync(It.IsAny<Func<CancellationToken,Task>>(), false, cancelTokenSource.Token));

			var policy = new SimplePolicy();
			var res = await policy.HandleAsync(async (_) => await Task.Delay(1), cancelTokenSource.Token);

			Assert.AreEqual(true, res.IsCanceled);
			Assert.AreEqual(false, res.IsFailed);
			Assert.IsFalse(res.IsSuccess);
			Assert.IsFalse(res.IsOk);

			moqProcessor.Verify((t) => t.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), false, cancelTokenSource.Token), Times.Never);
			cancelTokenSource.Dispose();
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
	}
}
