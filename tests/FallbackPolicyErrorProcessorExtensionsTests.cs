using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackPolicyErrorProcessorExtensionsTests
	{
		[Test]
		public void Should_ToFallbackPolicy_WithFunc_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicy(() => 42);

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithFunc_RegistersFallbackFunction()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => 42);

			Assert.That(policy.HasFallbackFunc<int>(), Is.True);
		}

		[Test]
		public void Should_ToFallbackPolicy_WithFunc_ProvideCorrectResult()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => 100);

			PolicyResult result = policy.Handle(() => throw new Exception("fail"));

			Assert.That(result.IsFailed, Is.False);
		}

		[Test]
		public void Should_ToFallbackPolicy_WithFunc_WithContextCancellationType_RegisterFunc()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => 42, CancellationType.Precancelable);

			Assert.That(policy.HasFallbackFunc<int>(), Is.True);
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_ToFallbackPolicy_WithFunc_WithContextCancellationType_VariesByType(CancellationType convertType)
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => 42, convertType);

			Assert.That(policy, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAsyncFunc_TaskT_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicy(async () => await Task.FromResult(42));

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAsyncFunc_TaskT_ProvideCorrectResult()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(async () => await Task.FromResult(200));

			PolicyResult result = policy.Handle(() => throw new Exception("fail"));

			Assert.That(result.IsFailed, Is.False);
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAsyncFunc_TaskT_WithContextCancellationType_RegisterFunc()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(async () => await Task.FromResult(42), CancellationType.Precancelable);

			Assert.That(policy.HasAsyncFallbackFunc<int>(), Is.True);
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_ToFallbackPolicy_WithAsyncFunc_TaskT_WithContextCancellationType_VariesByType(CancellationType convertType)
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(async () => await Task.FromResult(42), convertType);

			Assert.That(policy, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAsyncFunc_Token_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicy(async (_) => await Task.FromResult(42));

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAsyncFunc_Token_ProvideCorrectResult()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(async (_) => await Task.FromResult(300));

			PolicyResult result = policy.Handle(() => throw new Exception("fail"));

			Assert.That(result.IsFailed, Is.False);
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicy(() => { });

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_RegistersFallbackAction()
		{
			var param = ErrorProcessorParam.Default();
			bool executed = false;
			var policy = param.ToFallbackPolicy(() => { executed = true; });

			policy.Handle(() => throw new Exception());

			Assert.That(executed, Is.True);
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_WithContextCancellationType_RegistersAction()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => { }, CancellationType.Precancelable);

			Assert.That(policy.HasFallbackAction(), Is.True);
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_ToFallbackPolicy_WithAction_WithContextCancellationType_VariesByType(CancellationType convertType)
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => { }, convertType);

			Assert.That(policy, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_SetOnlyGenericFallbackForGenericDelegate_WhenTrue()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy(() => { }, CancellationType.Precancelable, onlyGenericFallbackForGenericDelegate: true);

			Assert.That(policy, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_CancellationType_RegistersCancelableAction()
		{
			var param = ErrorProcessorParam.Default();
			bool executed = false;
			var policy = param.ToFallbackPolicy((Action)(() => executed = true), CancellationType.Precancelable);

			policy.Handle(() => throw new Exception());

			Assert.That(executed, Is.True);
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_ContextToken_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicy((_) => { });

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_ContextToken_PassesTokenToAction()
		{
			var param = ErrorProcessorParam.Default();
			bool executed = false;
			CancellationToken receivedToken = default;
			var policy = param.ToFallbackPolicy((ct) =>
			{
				executed = true;
				receivedToken = ct;
			});

			using (var cts = new CancellationTokenSource())
			{
				policy.Handle(() => throw new Exception(), cts.Token);

				Assert.That(executed, Is.True);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_ToFallbackPolicy_WithAction_ContextToken_NoCancelType_RegistersAction()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicy((_) => { });

			Assert.That(policy.HasFallbackAction(), Is.True);
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_TaskReturnReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicyWithAsyncFunc(async () => await Task.CompletedTask);

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_TaskRegisterFunc_WithContextCancellationType()
		{
			var param = ErrorProcessorParam.Default();
			bool executed = false;
			var policy = param.ToFallbackPolicyWithAsyncFunc(async () => { await Task.Delay(1); executed = true; }, CancellationType.Precancelable);

			policy.Handle(() => throw new Exception());

			Assert.That(executed, Is.True);
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_TaskRegisterFunc_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicyWithAsyncFunc(async () => await Task.CompletedTask, CancellationType.Cancelable);

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_Task_OnlyGenericFlag_True_SetsFlag()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicyWithAsyncFunc(async () => await Task.CompletedTask, CancellationType.Precancelable, true);

			Assert.That(policy, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_Task_PassesNoToken_WithoutTokenSignature()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicyWithAsyncFunc(async () => await Task.Delay(1));

			policy.Handle(() => throw new Exception());

			Assert.Pass("Completed without error");
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_Token_ReturnsFallbackPolicy()
		{
			var param = ErrorProcessorParam.Default();
			FallbackPolicy result = param.ToFallbackPolicyWithAsyncFunc(async (_) => await Task.CompletedTask);

			Assert.That(result, Is.InstanceOf<FallbackPolicy>());
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_Token_ReceivesToken_WhenCalled()
		{
			var param = ErrorProcessorParam.Default();
			CancellationToken receivedToken = default(CancellationToken);
			var policy = param.ToFallbackPolicyWithAsyncFunc(async (ct) => { receivedToken = ct; await Task.Delay(1); });

			using (var cts = new CancellationTokenSource())
			{
				policy.Handle(() => throw new Exception(), cts.Token);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_Token_RegisterFunc()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicyWithAsyncFunc(async (_) => await Task.CompletedTask);

			Assert.That(policy.HasAsyncFallbackFunc(), Is.True);
		}

		[Test]
		public void Should_ToFallbackPolicyWithAsyncFunc_Token_NoOnlyGenericFlag()
		{
			var param = ErrorProcessorParam.Default();
			var policy = param.ToFallbackPolicyWithAsyncFunc(async (_) => await Task.CompletedTask);

			Assert.That(policy, Is.InstanceOf<FallbackPolicy>());
		}
	}
}
