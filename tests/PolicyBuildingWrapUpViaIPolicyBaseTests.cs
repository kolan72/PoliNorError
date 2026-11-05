using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyBuildingWrapUpViaIPolicyBaseTests
	{
		[Test]
		public void Should_WrapUp_Via_IPolicyBase_Return_Registrar_With_Same_WrapperPolicy()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			var wrapperPolicy = new SimplePolicy();

			var registrar = policyBase.WrapUp(wrapperPolicy);

			Assert.That(registrar, Is.Not.Null);
			Assert.That(registrar.OuterPolicy, Is.SameAs(wrapperPolicy));
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_ThrowArgumentNullException_WhenWrapperPolicyIsNull()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);

			Assert.Throws<ArgumentNullException>(() => policyBase.WrapUp<SimplePolicy>(null));
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_Wrap_Collection_As_Single_Policy_Not_As_Policy_Sequence()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2).WithPolicyName("MyCollection");
			var wrapperViaInterface = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var wrapperViaInstance = collection.WrapUp(new SimplePolicy()).OuterPolicy;

			var resultViaInterface = wrapperViaInterface.Handle(() => throw new Exception("Test"));
			var resultViaInstance = wrapperViaInstance.Handle(() => throw new Exception("Test"));

			Assert.That(resultViaInterface.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(resultViaInstance.WrappedPolicyResults.Count(), Is.EqualTo(2));
			Assert.That(resultViaInterface.WrappedPolicyResults.Single().PolicyName, Is.EqualTo("MyCollection"));
			Assert.That(resultViaInstance.WrappedPolicyResults.All(r => r.PolicyName != "MyCollection"), Is.True);
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_HandleAction_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsSuccess, Is.True);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
			Assert.That(polResult.Errors.Any(), Is.False);
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_HandleAction_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			var polResult = outerPolicy.Handle(() => throw new IndexOutOfRangeException("Test"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
			Assert.That(polResult.Errors.Any(e => e is PolicyDelegateCollectionException), Is.False);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.False);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_HandleAction_When_WrappedResultHandler_SetsFailed()
		{
			IPolicyBase policyBase = PolicyCollection.Create()
													.WithRetry(2)
													.WithRetry(3)
													.AddPolicyResultHandlerForAll(pr => pr.SetFailed());
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<PolicyResultHandlerFailedException>());
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_Use_Adapter_PolicyName_For_Wrapped_Result()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");
			var outerPolicy = policyBase.WrapUp(new SimplePolicy().WithPolicyName("OuterSimple")).OuterPolicy;

			var polResult = outerPolicy.Handle(() => { });

			var wrappedResult = polResult.WrappedPolicyResults.Single();

			Assert.That(polResult.PolicyName, Is.EqualTo("OuterSimple"));
			Assert.That(wrappedResult.PolicyName, Is.EqualTo("MyCollection"));
			Assert.That(wrappedResult.Result, Is.Not.Null);
			Assert.That(wrappedResult.Result.PolicyName, Is.EqualTo("MyCollection"));
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_HandleT_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			var polResult = outerPolicy.Handle(() => 42);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(42));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_HandleT_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			var polResult = outerPolicy.Handle<int>(() => throw new IndexOutOfRangeException("Test"));

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
			Assert.That(polResult.Errors.Any(e => e is PolicyDelegateCollectionException<int>), Is.False);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public async Task Should_WrapUp_Via_IPolicyBase_HandleAsync_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			async Task act(CancellationToken _) { await Task.Delay(1); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
			Assert.That(polResult.Errors.Any(), Is.False);
		}

		[Test]
		public async Task Should_WrapUp_Via_IPolicyBase_HandleAsync_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			async Task act(CancellationToken _) { await Task.Delay(1); throw new IndexOutOfRangeException("Async error"); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
			Assert.That(polResult.Errors.Any(e => e is PolicyDelegateCollectionException), Is.False);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public async Task Should_WrapUp_Via_IPolicyBase_HandleAsyncT_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 99; }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(99));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public async Task Should_WrapUp_Via_IPolicyBase_HandleAsyncT_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new SimplePolicy()).OuterPolicy;

			async Task<int> func(CancellationToken _) { await Task.Delay(1); throw new IndexOutOfRangeException("Async T error"); }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
			Assert.That(polResult.Errors.Any(e => e is PolicyDelegateCollectionException<int>), Is.False);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public void Should_WrapUp_Via_IPolicyBase_FallbackWrapper_Recovers_Collection_Failure()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.WrapUp(new FallbackPolicy().WithFallbackAction(() => { })).OuterPolicy;

			var polResult = outerPolicy.Handle(() => throw new IndexOutOfRangeException("Test"));

			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
		}
	}
}