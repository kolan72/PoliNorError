using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyBuildingThenTests
	{
		[Test]
		public void Should_Then_ReturnWrapperPolicy_For_RetryWrappingSimple()
		{
			var simple = new SimplePolicy();
			var retry = new RetryPolicy(1);

			var result = simple.Then(retry);

			Assert.That(result, Is.SameAs(retry));
		}

		[Test]
		public void Should_Then_ReturnWrapperPolicy_For_FallbackWrappingSimple()
		{
			var simple = new SimplePolicy();
			var fallback = new FallbackPolicy().WithFallbackAction(() => { });

			var result = simple.Then(fallback);

			Assert.That(result, Is.SameAs(fallback));
		}

		[Test]
		public void Should_Then_ReturnWrapperPolicy_For_SimpleWrappingRetry()
		{
			var retry = new RetryPolicy(1);
			var simple = new SimplePolicy();

			var result = retry.Then(simple);

			Assert.That(result, Is.SameAs(simple));
		}

		[Test]
		public void Should_Then_ThrowArgumentNullException_WhenWrapperPolicyIsNull()
		{
			var simple = new SimplePolicy();

			Assert.Throws<ArgumentNullException>(() => simple.Then<RetryPolicy>(null));
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_RetryWrappingSimple_When_NoError()
		{
			var innerPolicy = new SimplePolicy();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1));

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_RetryWrappingSimple_When_Error()
		{
			var innerPolicy = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(2));

			var polResult = outerPolicy.Handle(() => throw new InvalidOperationException("Test error"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(3));
			Assert.That(polResult.IsFailed, Is.True);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_FallbackWrappingSimple_When_Error()
		{
			var innerPolicy = new SimplePolicy();
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction(() => { });
			var outerPolicy = innerPolicy.Then(fallbackPolicy);

			var polResult = outerPolicy.Handle(() => throw new InvalidOperationException("Test error"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_HandleT_When_NoError()
		{
			var innerPolicy = new SimplePolicy();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1));

			var polResult = outerPolicy.Handle(() => 42);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(42));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_HandleT_When_Error()
		{
			var innerPolicy = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(2));

			var polResult = outerPolicy.Handle<int>(() => throw new InvalidOperationException("Test error"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(3));
			Assert.That(polResult.IsFailed, Is.True);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsync_When_NoError()
		{
			var innerPolicy = new SimplePolicy();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1));

			async Task act(CancellationToken _) { await Task.Delay(1); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsync_When_Error()
		{
			var innerPolicy = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(2));

			async Task act(CancellationToken _) { await Task.Delay(1); throw new InvalidOperationException("Async error"); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(3));
			Assert.That(polResult.IsFailed, Is.True);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsyncT_When_NoError()
		{
			var innerPolicy = new SimplePolicy();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1));

			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 99; }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(99));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsyncT_When_Error()
		{
			var innerPolicy = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(2));

			async Task<int> func(CancellationToken _) { await Task.Delay(1); throw new InvalidOperationException("Async T error"); }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(3));
			Assert.That(polResult.IsFailed, Is.True);
		}

		[Test]
		public void Should_DoubleThen_ProduceOneWrappedPolicyResult_When_NoError()
		{
			var simple = new SimplePolicy();
			var retry = new RetryPolicy(1);
			var fallback = new FallbackPolicy().WithFallbackAction(() => { });

			var outerPolicy = simple.Then(retry).Then(fallback);

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(outerPolicy, Is.SameAs(fallback));
			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public void Should_DoubleThen_ProduceTwoWrappedPolicyResults_When_Error()
		{
			var simple = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var retry1 = new RetryPolicy(1);
			var retry2 = new RetryPolicy(1);

			var outerPolicy = simple.Then(retry1).Then(retry2);

			var polResult = outerPolicy.Handle(() => throw new InvalidOperationException("Double chain error"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(2));
			Assert.That(polResult.IsFailed, Is.True);
		}

		[Test]
		public void Should_Then_PreserveWrapperPolicyName_InResult()
		{
			const string wrapperName = "MyRetryWrapper";
			var innerPolicy = new SimplePolicy();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1).WithPolicyName(wrapperName));

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.PolicyName, Is.EqualTo(wrapperName));
		}

		[Test]
		public void Should_Then_ReflectInnerPolicyName_InWrappedPolicyResults()
		{
			const string innerName = "InnerSimple";
			const string outerName = "OuterRetry";

			var innerPolicy = new SimplePolicy().WithPolicyName(innerName);
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1).WithPolicyName(outerName));

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.PolicyName, Is.EqualTo(outerName));
			Assert.That(polResult.WrappedPolicyResults.First().Result.PolicyName, Is.EqualTo(innerName));
		}

		[Test]
		public void Should_Then_HandleCanceledToken_When_InnerPolicyWrapsSimple()
		{
			var innerPolicy = new SimplePolicy();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(1));

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var polResult = outerPolicy.Handle(() => { }, cts.Token);

				Assert.That(polResult.IsCanceled, Is.True);
			}
		}

		[Test]
		public void Should_Then_RetryMultipleTimes_WhenInnerSimplePolicyAlwaysFails()
		{
			int attemptCount = 0;
			var innerPolicy = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var outerPolicy = innerPolicy.Then(new RetryPolicy(3));

			var polResult = outerPolicy.Handle(() =>
			{
				attemptCount++;
				throw new InvalidOperationException("fail");
			});

			Assert.That(polResult.IsFailed, Is.True);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(4));
			Assert.That(attemptCount, Is.EqualTo(4));
		}

		[Test]
		public void Should_Then_FallbackRecoverInnerFailure()
		{
			var innerPolicy = new SimplePolicy().ExcludeError<InvalidOperationException>();
			var outerPolicy = innerPolicy.Then(
				new FallbackPolicy().WithFallbackAction(() => { }));

			var polResult = outerPolicy.Handle(() => throw new InvalidOperationException("Inner failure"));

			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.WrappedPolicyResults.First().Result.IsFailed, Is.True);
		}
	}
}
