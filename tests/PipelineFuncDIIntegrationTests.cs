using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class PipelineFuncDIIntegrationTests
	{
		[Test]
		public void Should_ResolveAndRunThreeStepChain_When_BuiltWithAddPipelineFuncChain()
		{
			var services = new ServiceCollection();
			services.AddPipelineFuncChain<int, int>(
				_ => PipelineFuncBuilder
					.StartWith<int, int>(i => i * 2)
					.AddFuncWithRetry(i => $"v{i}", 1)
					.AddFunc(s => s.Length, new SimplePolicy().WithPolicyName("LengthStep")),
				pipelineName: "tripleStep");
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>("tripleStep");

			Assert.That(pipeline.Name, Is.EqualTo("tripleStep"));

			var result = pipeline.Invoke(10);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("v20".Length));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo("LengthStep"));

			var asyncResult = pipeline.InvokeAsync(10, CancellationToken.None).GetAwaiter().GetResult();

			Assert.That(asyncResult.IsFailed, Is.False);
			Assert.That(asyncResult.Result, Is.EqualTo("v20".Length));

			Assert.That(serviceProvider.GetAllPipelineFuncs().Count, Is.EqualTo(1));
			serviceProvider.GetRequiredService<PipelineFuncRegistry>().Get<int, int>("tripleStep");

			var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetPipelineFunc<int, int>());
			Assert.That(ex.Message, Does.Contain("'tripleStep'"));
		}
	}
}