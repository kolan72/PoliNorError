using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class RateLimitPolicyCompositionTests
	{
		[Test]
		public void Should_RetryOuter_Succeed_After_Denials_And_PassRetryAfter_To_DelayFunc()
		{
			int acquireCount = 0;
			int attempts = 0;
			int delayFuncCalls = 0;
			TimeSpan? delayReceivedRetryAfter = null;

			var limiter = new DelegateRateLimiter(() =>
			{
				acquireCount++;
				return acquireCount <= 2
					? RateLimitResult.Denied(TimeSpan.FromMilliseconds(7))
					: RateLimitResult.Permitted;
			});

			var outerPolicy = new RateLimitPolicy(limiter).Then(
				new RetryPolicy(3).WithWait((attempt, ex) =>
				{
					delayFuncCalls++;
					delayReceivedRetryAfter = (ex as RateLimitException)?.RetryAfter;
					return TimeSpan.Zero;
				}));

			var result = outerPolicy.Handle(() => attempts++);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(attempts, Is.EqualTo(1));
			Assert.That(acquireCount, Is.EqualTo(3));
			Assert.That(delayFuncCalls, Is.EqualTo(2));
			Assert.That(delayReceivedRetryAfter, Is.EqualTo(TimeSpan.FromMilliseconds(7)));
		}

		[Test]
		public void Should_RetryOuter_Fail_After_ExhaustedDenials()
		{
			int attempts = 0;
			int delayFuncCalls = 0;

			var limiter = new DelegateRateLimiter(() => RateLimitResult.Denied(TimeSpan.FromMilliseconds(1)));

			var outerPolicy = new RateLimitPolicy(limiter).Then(
				new RetryPolicy(2).WithWait((attempt, ex) =>
				{
					delayFuncCalls++;
					return TimeSpan.Zero;
				}));

			var result = outerPolicy.Handle(() => attempts++);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(attempts, Is.Zero);
			Assert.That(delayFuncCalls, Is.EqualTo(2));
		}

		[Test]
		public void Should_GateOuter_NotExecuteInner_When_Denied()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Denied(TimeSpan.FromSeconds(1)));
			bool innerRan = false;

			var outerPolicy = new SimplePolicy().Then(new RateLimitPolicy(limiter));

			var result = outerPolicy.Handle(() => innerRan = true);

			Assert.That(innerRan, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsRateLimited(), Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.RateLimited));
			Assert.That(result.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.None));
		}

		[Test]
		public void Should_GateOuterOverCollection_NotExecuteInner_When_Denied()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Denied());
			bool innerRan = false;

			var collection = PolicyCollection.Create(new SimplePolicy());
			var outerPolicy = collection.Then(new RateLimitPolicy(limiter));

			var result = outerPolicy.Handle(() => innerRan = true);

			Assert.That(innerRan, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsRateLimited(), Is.True);
		}

		[Test]
		public void Should_GateOuter_RecordWrappedPolicyResults_When_Permitted()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Permitted);
			bool innerRan = false;

			var outerPolicy = new SimplePolicy().Then(new RateLimitPolicy(limiter));

			var result = outerPolicy.Handle(() => innerRan = true);

			Assert.That(innerRan, Is.True);
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.WrappedPolicyResults, Is.Not.Null);
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task Should_RetryOuter_Succeed_After_Denials_Async()
		{
			int acquireCount = 0;
			int attempts = 0;
			TimeSpan? delayReceivedRetryAfter = null;

			var limiter = new DelegateRateLimiter(() =>
			{
				acquireCount++;
				return acquireCount == 1
					? RateLimitResult.Denied(TimeSpan.FromMilliseconds(3))
					: RateLimitResult.Permitted;
			});

			var outerPolicy = new RateLimitPolicy(limiter).Then(
				new RetryPolicy(2).WithWait((attempt, ex) =>
				{
					delayReceivedRetryAfter = (ex as RateLimitException)?.RetryAfter;
					return TimeSpan.Zero;
				}));

			async Task act(CancellationToken _)
			{
				attempts++;
				await Task.Yield();
			}

			var result = await outerPolicy.HandleAsync(act);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(attempts, Is.EqualTo(1));
			Assert.That(acquireCount, Is.EqualTo(2));
			Assert.That(delayReceivedRetryAfter, Is.EqualTo(TimeSpan.FromMilliseconds(3)));
		}

		[Test]
		public async Task Should_GateOuter_NotExecuteInner_Async_When_Denied()
		{
			var limiter = new DelegateRateLimiter(
				(Func<CancellationToken, Task<RateLimitResult>>)(token => Task.FromResult(RateLimitResult.Denied())));
			bool innerRan = false;

			var outerPolicy = new SimplePolicy().Then(new RateLimitPolicy(limiter));

			async Task act(CancellationToken _)
			{
				innerRan = true;
				await Task.Yield();
			}

			var result = await outerPolicy.HandleAsync(act);

			Assert.That(innerRan, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsRateLimited(), Is.True);
		}
	}
}
