using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace PoliNorError.Tests
{
	internal class PipelineDelegateHolderTests
	{
		[Test]
		public void Should_ReturnSuccessfulPipelineResult_When_FunctionCompletesWithoutErrors()
		{
			var holder = new PipelineDelegateHolder<int, string>(value => $"value:{value}");

			var pipelineDelegate = holder.GetPipelineDelegate();
			var result = pipelineDelegate(3, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo("value:3"));
		}

		[Test]
		public void Should_ReturnFailedPipelineResult_When_FunctionThrowsHandledException()
		{
			var expectedException = new InvalidOperationException("boom");
			var holder = new PipelineDelegateHolder<int, string>(_ => throw expectedException);

			var pipelineDelegate = holder.GetPipelineDelegate();
			var result = pipelineDelegate(0, CancellationToken.None);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.Null);
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.IsFailed, Is.False);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.Errors.Single(), Is.SameAs(expectedException));
		}

		[Test]
		public void Should_ReturnCanceledPipelineResult_When_TokenIsAlreadyCanceled()
		{
			var holder = new PipelineDelegateHolder<int, string>(value => $"value:{value}");
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var pipelineDelegate = holder.GetPipelineDelegate();
				var result = pipelineDelegate(3, cts.Token);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.Result, Is.Null);
				Assert.That(result.FailedPolicyResult, Is.Not.Null);
				Assert.That(result.FailedPolicyResult.IsCanceled, Is.True);
				Assert.That(result.FailedPolicyResult.NoError, Is.True);
			}
		}

		[Test]
		public void Should_UseConfiguredBulkErrorProcessor_When_ConfigurationIsProvided()
		{
			var expectedException = new InvalidOperationException("boom");
			var holder = new PipelineDelegateHolder<int, string>(_ => throw expectedException);
			holder.SetConfigure(processors => processors.WithErrorProcessorOf(exception => exception.Data["processed"] = true));

			var pipelineDelegate = holder.GetPipelineDelegate();
			var result = pipelineDelegate(0, CancellationToken.None);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(expectedException.Data.Contains("processed"), Is.True);
			Assert.That(expectedException.Data["processed"], Is.EqualTo(true));
		}
	}
}
