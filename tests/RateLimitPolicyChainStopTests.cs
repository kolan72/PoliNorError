using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class RateLimitPolicyChainStopTests
	{
		private static DelegateRateLimiter DeniedLimiter()
			=> new DelegateRateLimiter(() => RateLimitResult.Denied());

		private static DelegateRateLimiter PermittedLimiter()
			=> new DelegateRateLimiter(() => RateLimitResult.Permitted);

#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.
		private static DelegateRateLimiter DeniedLimiterAsync()
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
			=> new DelegateRateLimiter(
				(_ => Task.FromResult(RateLimitResult.Denied())));

		[Test]
		public void Should_Stop_PolicyCollection_Chain_When_RateLimit_Denies()
		{
			bool ran = false;
			var collection = PolicyCollection.Create(new RateLimitPolicy(DeniedLimiter()), new SimplePolicy());

			var result = collection.HandleDelegate(() => ran = true);

			Assert.That(ran, Is.False);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.LastPolicyResult.IsExecutionDenied(), Is.True);
			Assert.That(result.LastPolicyResultFailedReason, Is.EqualTo(PolicyResultFailedReason.ExecutionDenied));
		}

		[Test]
		public async Task Should_Stop_PolicyCollection_Chain_Async_When_RateLimit_Denies()
		{
			bool ran = false;
			var collection = PolicyCollection.Create(new RateLimitPolicy(DeniedLimiterAsync()), new SimplePolicy());

			var result = await collection.HandleDelegateAsync(_ =>
			{
				ran = true;
				return Task.CompletedTask;
			});

			Assert.That(ran, Is.False);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.LastPolicyResult.IsExecutionDenied(), Is.True);
		}

		[Test]
		public void Should_Stop_PolicyCollection_Typed_Chain_When_RateLimit_Denies()
		{
			bool ran = false;
			var collection = PolicyCollection.Create(new RateLimitPolicy(DeniedLimiter()), new SimplePolicy());

			var result = collection.HandleDelegate(() =>
			{
				ran = true;
				return 42;
			});

			Assert.That(ran, Is.False);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.LastPolicyResult.IsExecutionDenied(), Is.True);
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		[Test]
		public void Should_Stop_PolicyDelegateCollection_Chain_When_RateLimit_Denies()
		{
			bool ran = false;
			Action action = () => ran = true;

			var collection = PolicyDelegateCollection.Create(
				new RateLimitPolicy(DeniedLimiter()).ToPolicyDelegate(action),
				new SimplePolicy().ToPolicyDelegate(action));

			var result = collection.HandleAll();

			Assert.That(ran, Is.False);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.LastPolicyResultFailedReason, Is.EqualTo(PolicyResultFailedReason.ExecutionDenied));
		}

		[Test]
		public async Task Should_Stop_PolicyDelegateCollection_Chain_Async_When_RateLimit_Denies()
		{
			bool ran = false;
			Func<CancellationToken, Task> func = ct =>
			{
				ran = true;
				return Task.CompletedTask;
			};

			var collection = PolicyDelegateCollection.Create(
				new RateLimitPolicy(DeniedLimiterAsync()).ToPolicyDelegate(func),
				new SimplePolicy().ToPolicyDelegate(func));

			var result = await collection.HandleAllAsync();

			Assert.That(ran, Is.False);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.LastPolicyResultFailedReason, Is.EqualTo(PolicyResultFailedReason.ExecutionDenied));
		}

		[Test]
		public async Task Should_Stop_PolicyDelegateCollection_Typed_Chain_Async_When_RateLimit_Denies()
		{
			bool ran = false;
			Func<CancellationToken, Task<int>> func = _ =>
			{
				ran = true;
				return Task.FromResult(42);
			};

			var collection = PolicyDelegateCollection<int>.Create(
				new RateLimitPolicy(DeniedLimiterAsync()).ToPolicyDelegate(func),
				new SimplePolicy().ToPolicyDelegate(func));

			var result = await collection.HandleAllAsync();

			Assert.That(ran, Is.False);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.LastPolicyResultFailedReason, Is.EqualTo(PolicyResultFailedReason.ExecutionDenied));
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		[Test]
		public void Should_Handle_Delegate_And_Stop_After_Success_When_RateLimit_Permits()
		{
			bool ran = false;
			var collection = PolicyCollection.Create(new RateLimitPolicy(PermittedLimiter()), new SimplePolicy());

			var result = collection.HandleDelegate(() => ran = true);

			Assert.That(ran, Is.True);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(1));
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Should_Continue_Chain_On_Ordinary_Failure()
		{
			bool secondRan = false;
			var first = new RetryPolicy(1).ToPolicyDelegate(() => throw new Exception("boom"));
			var second = new SimplePolicy().ToPolicyDelegate((Action)(() => secondRan = true));

			var result = PolicyDelegateCollection.Create(first, second).HandleAll();

			Assert.That(secondRan, Is.True);
			Assert.That(result.PolicyDelegateResults.Count(), Is.EqualTo(2));
			Assert.That(result.PolicyDelegatesUnused, Is.Empty);
		}

		[Test]
		public void Should_Stop_Wrapped_Collection_Chain_When_RateLimit_Denies()
		{
			bool ran = false;
			var collection = PolicyCollection.Create(new RateLimitPolicy(DeniedLimiter()), new SimplePolicy());
			var outerPolicy = collection.Then(new SimplePolicy());

			var result = outerPolicy.Handle(() => ran = true);


#pragma warning disable S125 // Sections of code should not be commented out
							// The inner chain stops at the denial, so the wrapped SimplePolicy never runs;
							// the RateLimitException thrown by the wrapper surfaces on the outer result.
			Assert.That(ran, Is.False);
#pragma warning restore S125 // Sections of code should not be commented out
			Assert.That(result.Errors.OfType<RateLimitException>().Any(), Is.True);
		}
	}
}
