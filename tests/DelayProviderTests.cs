using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.CancellationTests;

namespace PoliNorError.Tests
{
	internal class DelayProviderTests
	{
		[Test]
		[TestCase(true, TestCancellationMode.Aggregate)]
		[TestCase(true, TestCancellationMode.OperationCanceled)]
		[TestCase(false, null)]
		public void Should_BackoffSafely_Be_Without_Exception_If_Cancellation_Has_Occured(bool canceledOnLinkedSource, TestCancellationMode? cancellationMode)
		{
			using (var cts = new CancellationTokenSource())
			{
				var delayProvider = new DelayProviderThatAlreadyCanceled(cts, canceledOnLinkedSource, cancellationMode);
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
		public void Should_DelayAndCheckIfResultFailed_Return_True_If_BackoffFailed()
		{
			var delayProvider = new DelayProviderThatFailed();
			var pr = PolicyResult.ForSync();
			var handlingException = new Exception("Test");
			var IsFailed = delayProvider.DelayAndCheckIfResultFailed(TimeSpan.FromTicks(1), pr, handlingException);
			Assert.That(IsFailed, Is.True);
			Assert.That(pr.IsFailed, Is.True);
		}

		[Test]
		[TestCase(true, TestCancellationMode.Aggregate)]
		[TestCase(true, TestCancellationMode.OperationCanceled)]
		[TestCase(false, null)]
		public void Should_DelayAndCheckIfResultFailed_Return_True_If_BackoffCanceled(bool canceledOnLinkedSource, TestCancellationMode? cancellationMode)
		{
			var pr = PolicyResult.ForSync();
			var handlingException = new Exception("Test");
			using (var cts = new CancellationTokenSource())
			{
				var delayProvider = new DelayProviderThatAlreadyCanceled(cts, canceledOnLinkedSource, cancellationMode);
				var IsFailed = delayProvider.DelayAndCheckIfResultFailed(TimeSpan.FromTicks(1), pr, handlingException, cts.Token);
				Assert.That(IsFailed, Is.True);
				Assert.That(pr.IsFailed, Is.True);
				Assert.That(pr.IsCanceled, Is.True);
			}
		}

		[Test]
		public async Task Should_DelayAndCheckIfResultFailedAsync_Return_True_If_BackoffFailed()
		{
			var delayProvider = new DelayProviderThatFailed();
			var pr = PolicyResult.ForSync();
			var handlingException = new Exception("Test");
			var IsFailed = await delayProvider.DelayAndCheckIfResultFailedAsync(TimeSpan.FromTicks(1), pr, handlingException, false);
			Assert.That(IsFailed, Is.True);
			Assert.That(pr.IsFailed, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_DelayAndCheckIfResultFailedAsync_Return_True_If_BackoffCanceledAsync(bool canceledOnLinkedSource)
		{
			var pr = PolicyResult.ForNotSync();
			var handlingException = new Exception("Test");
			using (var cts = new CancellationTokenSource())
			{
				var delayProvider = new DelayProviderThatAlreadyCanceled(cts, canceledOnLinkedSource);
				var IsFailed = await delayProvider.DelayAndCheckIfResultFailedAsync(TimeSpan.FromTicks(1), pr, handlingException, false, cts.Token);
				Assert.That(IsFailed, Is.True);
				Assert.That(pr.IsFailed, Is.True);
				Assert.That(pr.IsCanceled, Is.True);
			}
		}

		[Test]
		public async Task Should_DelayAndCheckIfResultFailedAsync_Return_False_If_Delay_Is_Null()
		{
			var delayProvider = new DelayProvider();
			var pr = PolicyResult.ForSync();
			var handlingException = new Exception("Test");
			var IsFailed = await delayProvider.DelayAndCheckIfResultFailedAsync(null, pr, handlingException, false);
			Assert.That(IsFailed, Is.False);
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
		public void Should_DelayAndCheckIfResultFailed_Return_False_If_Delay_Is_Null()
		{
			var delayProvider = new DelayProvider();
			var pr = PolicyResult.ForSync();
			var handlingException = new Exception("Test");
			var IsFailed = delayProvider.DelayAndCheckIfResultFailed(null, pr, handlingException);
			Assert.That(IsFailed, Is.False);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_BackoffSafelyAsync_Be_Without_Exception_If_Cancellation_Has_Occured(bool canceledOnLinkedSource)
		{
			using (var cts = new CancellationTokenSource())
			{
				var delayProvider = new DelayProviderThatAlreadyCanceled(cts, canceledOnLinkedSource);
				var br = await delayProvider.BackoffSafelyAsync(TimeSpan.FromMilliseconds(1), canceledOnLinkedSource, cts.Token).ConfigureAwait(false);
				Assert.That(br.IsCanceled, Is.True);
			}
		}
	}
}
