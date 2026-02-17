using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class TimeoutPolicyTests
	{
		[Test]
		public async Task Should_SetFailedAndCanceled_When_Timeout_Exceeded()
		{
			var policy = new TimeoutPolicy(TimeSpan.FromMilliseconds(30));

			var result = await policy.HandleAsync(async ct => await Task.Delay(200, ct), false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.True);
			Assert.That(result.Errors.Any(e => e is TimeoutRejectedException), Is.True);
		}

		[Test]
		public void Should_Throw_When_ThrowTimeoutException_Enabled()
		{
			var policy = new TimeoutPolicy(TimeSpan.FromMilliseconds(20))
				.WithOptions(o => o.ThrowTimeoutException = true);

			Assert.ThrowsAsync<TimeoutRejectedException>(async () =>
				await policy.HandleAsync(async ct => await Task.Delay(200, ct), false, CancellationToken.None));
		}

		[Test]
		public void Should_Handle_WithoutTimeout_For_Fast_Action()
		{
			var policy = new TimeoutPolicy(TimeSpan.FromSeconds(1));

			var result = policy.Handle(() => { });

			Assert.That(result.NoError, Is.True);
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsCanceled, Is.False);
		}
	}
}
