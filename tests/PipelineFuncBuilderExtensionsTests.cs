using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class PipelineFuncBuilderExtensionsTests
	{
		[Test]
		public void Should_UseStepPolicyName_When_NoNameIsSupplied()
		{
			var pipelineFunc = PipelineFuncBuilder.StartWith<int, string>(i => $"value:{i}").ToPipelineFunc();

			Assert.That(pipelineFunc.Name, Is.EqualTo("SimplePolicy"));
		}

		[Test]
		public void Should_UseLeafPolicyName_When_PolicyIsNamed()
		{
			var pipelineFunc = PipelineFuncBuilder
				.StartWithRetry<int, int>(i => i + 1, 1)
				.ToPipelineFunc();

			Assert.That(pipelineFunc.Name, Is.EqualTo("RetryPolicy"));
		}

		[Test]
		public void Should_UseLastStepPolicyName_When_ChainIsMaterialized()
		{
			var pipelineFunc = PipelineFuncBuilder
				.StartWith<int, int>(i => i * 2)
				.AddFunc(i => i + 1, new SimplePolicy().WithPolicyName("LastStep"))
				.ToPipelineFunc();

			Assert.That(pipelineFunc.Name, Is.EqualTo("LastStep"));
		}

		[Test]
		public void Should_OverrideName_When_ExplicitNameIsSupplied()
		{
			var pipelineFunc = PipelineFuncBuilder
				.StartWith<int, int>(i => i)
				.ToPipelineFunc("MyPipeline");

			Assert.That(pipelineFunc.Name, Is.EqualTo("MyPipeline"));
		}

		[Test]
		public void Should_RoundTripDelegate_When_Materialized()
		{
			var pipelineFunc = PipelineFuncBuilder
				.StartWith<int, int>(i => i + 1)
				.AddFunc(i => i * 10, new RetryPolicy(1))
				.ToPipelineFunc("roundTrip");

			var result = pipelineFunc.GetPipelineDelegate()(4, default);

			Assert.That(pipelineFunc.Name, Is.EqualTo("roundTrip"));
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(50));
		}

		[Test]
		public void Should_ThrowArgumentNullException_When_BuilderIsNull()
		{
			IPipelineFuncBuilder<int, int> builder = null;

			Assert.That(() => builder.ToPipelineFunc(), Throws.ArgumentNullException);
		}

		[Test]
		public void Should_RegisterIntoRegistry_When_ServiceProviderSupplied()
		{
			var services = new ServiceCollection();
			services.AddSingleton<PipelineFuncRegistry>();
			var serviceProvider = services.BuildServiceProvider();

			var pipelineFunc = PipelineFuncBuilder
				.StartWith<int, int>(i => i + 1)
				.ToPipelineFunc(serviceProvider, "viaBridge");

			var registry = serviceProvider.GetRequiredService<PipelineFuncRegistry>();

			Assert.That(pipelineFunc.Name, Is.EqualTo("viaBridge"));
			Assert.That(registry.Get<int, int>("viaBridge"), Is.SameAs(pipelineFunc));
			Assert.That(registry.Contains<int, int>(), Is.False);
		}

		[Test]
		public void Should_RegisterUnderDefaultKey_When_NoNameSuppliedWithProvider()
		{
			var services = new ServiceCollection();
			services.AddSingleton<PipelineFuncRegistry>();
			var serviceProvider = services.BuildServiceProvider();

			var pipelineFunc = PipelineFuncBuilder
				.StartWith<int, int>(i => i + 1)
				.ToPipelineFunc(serviceProvider);

			var registry = serviceProvider.GetRequiredService<PipelineFuncRegistry>();

			Assert.That(registry.Get<int, int>(), Is.SameAs(pipelineFunc));
			Assert.That(registry.Contains<int, int>(), Is.True);
		}

		[Test]
		public void Should_ThrowInvalidOperationException_When_NoRegistryIsRegistered()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			Assert.That(
				() => PipelineFuncBuilder.StartWith<int, int>(i => i).ToPipelineFunc(serviceProvider),
				Throws.InstanceOf<InvalidOperationException>().With.Message.Contains(nameof(PipelineFuncServiceCollectionExtensions.AddPipelineFunc)));
		}

		[Test]
		public void Should_ThrowInvalidOperationException_When_SameKeyIsRegisteredTwiceViaBridge()
		{
			var services = new ServiceCollection();
			services.AddSingleton<PipelineFuncRegistry>();
			var serviceProvider = services.BuildServiceProvider();

			PipelineFuncBuilder.StartWith<int, int>(i => i + 1).ToPipelineFunc(serviceProvider, "dup");

			Assert.That(
				() => PipelineFuncBuilder.StartWith<int, int>(i => i + 2).ToPipelineFunc(serviceProvider, "dup"),
				Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("already registered"));
		}
	}
}