using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyBuildingThenViaIPolicyBaseTests
	{
		[Test]
		public void Should_Then_Via_IPolicyBase_Return_Same_RetryWrapperPolicy()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			var wrapperPolicy = new RetryPolicy(1);

			var result = policyBase.Then(wrapperPolicy);

			Assert.That(result, Is.SameAs(wrapperPolicy));
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_Return_Same_FallbackWrapperPolicy()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			var wrapperPolicy = new FallbackPolicy().WithFallbackAction(() => { });

			var result = policyBase.Then(wrapperPolicy);

			Assert.That(result, Is.SameAs(wrapperPolicy));
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_ThrowArgumentNullException_WhenWrapperPolicyIsNull()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);

			Assert.Throws<ArgumentNullException>(() => policyBase.Then<SimplePolicy>(null));
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_Wrap_Collection_As_Single_Policy_Not_As_Policy_Sequence()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2).WithPolicyName("MyCollection");
			var wrapperViaInterface = policyBase.Then(new SimplePolicy());

			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var wrapperViaInstance = collection.Then(new SimplePolicy());

			var resultViaInterface = wrapperViaInterface.Handle(() => throw new Exception("Test"));
			var resultViaInstance = wrapperViaInstance.Handle(() => throw new Exception("Test"));

			Assert.That(resultViaInterface.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(resultViaInstance.WrappedPolicyResults.Count(), Is.EqualTo(2));
			Assert.That(resultViaInterface.WrappedPolicyResults.Single().PolicyName, Is.EqualTo("MyCollection"));
			Assert.That(resultViaInstance.WrappedPolicyResults.All(r => r.PolicyName != "MyCollection"), Is.True);
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_HandleAction_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
			Assert.That(polResult.Errors.Any(), Is.False);
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_HandleAction_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => throw new IndexOutOfRangeException("Test"));

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
			Assert.That(polResult.Errors.Any(e => e is PolicyDelegateCollectionException), Is.False);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.False);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_HandleT_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => 42);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(42));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_HandleT_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle<int>(() => throw new IndexOutOfRangeException("Test"));

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
			Assert.That(polResult.Errors.FirstOrDefault(), Is.TypeOf<IndexOutOfRangeException>());
			Assert.That(polResult.Errors.Any(e => e is PolicyDelegateCollectionException<int>), Is.False);
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public async Task Should_Then_Via_IPolicyBase_HandleAsync_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			async Task act(CancellationToken _) { await Task.Delay(1); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
			Assert.That(polResult.Errors.Any(), Is.False);
		}

		[Test]
		public async Task Should_Then_Via_IPolicyBase_HandleAsync_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

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
		public async Task Should_Then_Via_IPolicyBase_HandleAsyncT_When_NoError()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 99; }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(99));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public async Task Should_Then_Via_IPolicyBase_HandleAsyncT_When_Error_Uses_LastError_Semantics()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new SimplePolicy());

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
		public void Should_Then_Via_IPolicyBase_RetryWrapper_Retry_Failing_Collection()
		{
			int attemptCount = 0;
			IPolicyBase policyBase = PolicyCollection.Create().WithPolicy(new SimplePolicy().ExcludeError<InvalidOperationException>());
			var outerPolicy = policyBase.Then(new RetryPolicy(2));

			var polResult = outerPolicy.Handle(() =>
			{
				attemptCount++;
				throw new InvalidOperationException("fail");
			});

			Assert.That(polResult.IsFailed, Is.True);
			Assert.That(attemptCount, Is.EqualTo(3));
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(3));
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_FallbackWrapper_Recovers_Collection_Failure()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = policyBase.Then(new FallbackPolicy().WithFallbackAction(() => { }));

			var polResult = outerPolicy.Handle(() => throw new IndexOutOfRangeException("Test"));

			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.Single().IsFailed, Is.True);
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_HandleAction_With_Canceled_Token()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			var outerPolicy = policyBase.Then(new SimplePolicy());

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var polResult = outerPolicy.Handle(() => { }, cts.Token);

				Assert.That(polResult.IsCanceled, Is.True);
				Assert.That(polResult.IsFailed, Is.False);
			}
		}

		[Test]
		public void Should_Then_Via_IPolicyBase_PreserveWrapperPolicyName_InResult()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			var outerPolicy = policyBase.Then(new SimplePolicy().WithPolicyName("OuterSimple"));

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.PolicyName, Is.EqualTo("OuterSimple"));
			Assert.That(polResult.WrappedPolicyResults.Single().PolicyName, Is.EqualTo(nameof(PolicyCollection)));
		}

		[Test]
		public void Should_Then_And_WrapUp_Via_IPolicyBase_Produce_Equivalent_Results()
		{
			IPolicyBase policyBaseViaThen = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			IPolicyBase policyBaseViaWrapUp = PolicyCollection.Create().WithRetry(1).WithRetry(2);

			var viaThen = policyBaseViaThen.Then(new SimplePolicy());
			var viaWrapUp = policyBaseViaWrapUp.WrapUp(new SimplePolicy()).OuterPolicy;

			var resultThen = viaThen.Handle(() => throw new Exception("Test"));
			var resultWrapUp = viaWrapUp.Handle(() => throw new Exception("Test"));

			Assert.That(resultThen.IsFailed, Is.EqualTo(resultWrapUp.IsFailed));
			Assert.That(resultThen.Errors.First().GetType(), Is.EqualTo(resultWrapUp.Errors.First().GetType()));
			Assert.That(resultThen.WrappedPolicyResults.Count(), Is.EqualTo(resultWrapUp.WrappedPolicyResults.Count()));
		}
	}
}