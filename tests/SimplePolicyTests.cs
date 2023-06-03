using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

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
		public void Should_Execute_CanNotHandleIfTokenCanceledAlready()
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
	}
}
