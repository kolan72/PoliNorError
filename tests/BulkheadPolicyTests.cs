using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class BulkheadPolicyTests
	{
		[Test]
		public async Task Should_Reject_When_MaxParallelization_Reached_And_NoQueue()
		{
			var policy = new BulkheadPolicy(maxParallelization: 1, maxQueueSize: 0);
			var gate = new TaskCompletionSource<bool>();

			var running = policy.HandleAsync(async _ => await gate.Task, false, CancellationToken.None);
			await Task.Delay(30);

			var rejected = await policy.HandleAsync(async _ => await Task.Delay(10), false, CancellationToken.None);

			Assert.That(rejected.IsFailed, Is.True);
			Assert.That(rejected.Errors.Any(e => e is BulkheadRejectedException), Is.True);

			gate.TrySetResult(true);
			await running;
		}

		[Test]
		public async Task Should_Allow_Queued_Call_When_QueueConfigured()
		{
			var policy = new BulkheadPolicy(maxParallelization: 1, maxQueueSize: 1)
				.WithQueueTimeout(TimeSpan.FromSeconds(1));
			var gate = new TaskCompletionSource<bool>();

			var first = policy.HandleAsync(async _ => await gate.Task, false, CancellationToken.None);
			await Task.Delay(30);

			var secondTask = policy.HandleAsync(async _ => await Task.Delay(10), false, CancellationToken.None);
			await Task.Delay(30);
			gate.TrySetResult(true);

			var second = await secondTask;

			Assert.That(second.IsSuccess, Is.True);
			Assert.That(second.NoError, Is.True);
			await first;
		}
	}
}
