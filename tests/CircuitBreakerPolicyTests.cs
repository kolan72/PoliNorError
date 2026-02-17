using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class CircuitBreakerPolicyTests
	{
		[Test]
		public void Should_Open_After_Threshold_And_Reject_Following_Calls()
		{
			var policy = new CircuitBreakerPolicy()
				.WithOptions(o =>
				{
					o.FailureThreshold = 2;
					o.OpenDuration = TimeSpan.FromSeconds(2);
					o.BreakOnHandledExceptionsOnly = true;
				});

			policy.Handle(() => throw new InvalidOperationException());
			policy.Handle(() => throw new InvalidOperationException());

			var rejected = policy.Handle(() => { });

			Assert.That(policy.State, Is.EqualTo(CircuitBreakerState.Open));
			Assert.That(rejected.IsFailed, Is.True);
			Assert.That(rejected.Errors.Any(e => e is CircuitBreakerOpenException), Is.True);
		}

		[Test]
		public async Task Should_Transition_Open_To_HalfOpen_To_Closed_On_Success()
		{
			var policy = new CircuitBreakerPolicy()
				.WithOptions(o =>
				{
					o.FailureThreshold = 1;
					o.OpenDuration = TimeSpan.FromMilliseconds(100);
					o.HalfOpenMaxCalls = 1;
				});

			policy.Handle(() => throw new InvalidOperationException());
			Assert.That(policy.State, Is.EqualTo(CircuitBreakerState.Open));

			await Task.Delay(150);

			var result = policy.Handle(() => { });

			Assert.That(result.NoError, Is.True);
			Assert.That(policy.State, Is.EqualTo(CircuitBreakerState.Closed));
		}
	}
}
