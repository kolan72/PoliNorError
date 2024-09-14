using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class DelayProviderTests
	{
		[Test]
		public void Should_BackoffSafely_Be_Without_Exception_If_Cancellation_Has_Occured()
		{
			using (var cts = new CancellationTokenSource())
			{
				var delayProvider = new DelayProviderThatAlreadyCanceled(cts);
				var br = delayProvider.BackoffSafely(TimeSpan.FromMilliseconds(1), cts.Token);
				Assert.That(br.IsCanceled, Is.True);
			}
		}

		[Test]
		public void Should_BackoffSafely_Be_Without_Exception_If_BackoffFailed()
		{
			var delayProvider = new DelayProviderThatFailed();
			var br = delayProvider.BackoffSafely(TimeSpan.FromMilliseconds(1));
			Assert.That(br.IsFailed, Is.True);
			Assert.That(br.Error, Is.Not.Null);
		}

		[Test]
		public async Task Should_BackoffSafely_Be_Without_Exception_If_BackoffFailedAsync()
		{
			var delayProvider = new DelayProviderThatFailed();
			var br = await delayProvider.BackoffSafelyAsync(TimeSpan.FromMilliseconds(1));
			Assert.That(br.IsFailed, Is.True);
			Assert.That(br.Error, Is.Not.Null);
		}

		[Test]
		public async Task Should_BackoffSafelyAsync_Be_Without_Exception_If_Cancellation_Has_Occured()
		{
			using (var cts = new CancellationTokenSource())
			{
				var delayProvider = new DelayProviderThatAlreadyCanceled(cts);
				var br = await delayProvider.BackoffSafelyAsync(TimeSpan.FromMilliseconds(1), false, cts.Token).ConfigureAwait(false);
				Assert.That(br.IsCanceled, Is.True);
			}
		}
	}
}
