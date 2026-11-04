using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class PipelineFuncServiceCollectionExtensionsTests
	{
		[Test]
		public void Should_MaterializeBasicFuncPipeline_When_ResolvedByDefaultKey()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i + 1);
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();

			Assert.That(pipeline.Name, Is.EqualTo("SimplePolicy"));
			Assert.That(pipeline.Invoke(41).Result, Is.EqualTo(42));
			Assert.That(pipeline.Invoke(100).IsFailed, Is.False);
			Assert.That(serviceProvider.GetPipelineFunc<int, int>(), Is.SameAs(pipeline));
			Assert.That(serviceProvider.GetRequiredService<PipelineFuncRegistry>().Contains<int, int>(), Is.True);
		}

		[Test]
		public void Should_MaterializeFactoryPipelineOnce_When_ResolvedThroughCoreOverload()
		{
			var factoryCalls = 0;
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(_ =>
			{
				factoryCalls++;
				return PipelineFuncBuilder.StartWith<int, int>(i => i + 1);
			});
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();

			Assert.That(factoryCalls, Is.EqualTo(1));
			Assert.That(pipeline.Invoke(5).Result, Is.EqualTo(6));

			var again = serviceProvider.GetPipelineFunc<int, int>();

			Assert.That(again, Is.SameAs(pipeline));
			Assert.That(factoryCalls, Is.EqualTo(1));
			Assert.That(serviceProvider.GetRequiredService<PipelineFuncRegistry>().Contains<int, int>(), Is.True);
		}

		[Test]
		public void Should_ResolveByName_When_BasicOverloadGotExplicitName()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i * 2, "doubleIt");
			var serviceProvider = services.BuildServiceProvider();

			var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetPipelineFunc<int, int>());
			Assert.That(ex.Message, Does.Contain("'doubleIt'"));

			var pipeline = serviceProvider.GetPipelineFunc<int, int>("doubleIt");

			Assert.That(pipeline.Name, Is.EqualTo("doubleIt"));
			Assert.That(pipeline.Invoke(21).Result, Is.EqualTo(42));
		}

		[Test]
		public void Should_RegisterPrebuiltPipeline_When_PrebuiltOverloadUsed()
		{
			var prebuilt = PipelineFuncBuilder.StartWith<string, int>(s => s.Length).ToPipelineFunc("built");

			var services = new ServiceCollection();
			services.AddPipelineFunc(prebuilt);
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<string, int>();

			Assert.That(pipeline, Is.SameAs(prebuilt));
			Assert.That(pipeline.Name, Is.EqualTo("built"));
			Assert.That(pipeline.Invoke("tested").Result, Is.EqualTo(6));
			Assert.That(serviceProvider.GetRequiredService<PipelineFuncRegistry>().Get<string, int>(), Is.SameAs(prebuilt));
		}

		[Test]
		public void Should_MaterializeWithRetryPolicy_When_WithRetryOverloadUsed()
		{
			var services = new ServiceCollection();
			services.AddPipelineFuncWithRetry<int, int>(i => i + 1, 2);
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();
			var result = pipeline.Invoke(41);

			Assert.That(pipeline.Name, Is.EqualTo("RetryPolicy"));
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(42));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo("RetryPolicy"));
		}

		[Test]
		public void Should_MaterializeWithInfiniteRetry_When_WithInfiniteRetryOverloadUsed()
		{
			var services = new ServiceCollection();
			services.AddPipelineFuncWithInfiniteRetry<int, int>(i => i + 10);
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();
			var result = pipeline.Invoke(1);

			Assert.That(pipeline.Name, Is.EqualTo("RetryPolicy"));
			Assert.That(result.Result, Is.EqualTo(11));
		}

		[Test]
		public void Should_UseSuppliedPolicy_When_IPolicyBaseOverloadUsed()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i + 1, new SimplePolicy().WithPolicyName("MySimple"));
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();
			var result = pipeline.Invoke(1);

			Assert.That(pipeline.Name, Is.EqualTo("MySimple"));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo("MySimple"));
		}

		[Test]
		public void Should_UseFallbackFunc_When_WithFallbackOverloadUsed()
		{
			var services = new ServiceCollection();
			services.AddPipelineFuncWithFallback<int, int>(_ => throw new InvalidOperationException("Always fails"), () => -1);
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();
			var result = pipeline.Invoke(0);

			Assert.That(pipeline.Name, Is.EqualTo("FallbackPolicy"));
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(-1));
		}

		[Test]
		public void Should_ApplyConfiguredErrorProcessors_When_ChainIsUsed()
		{
			var expectedException = new InvalidOperationException("boom");
			var processors = new PipelineErrorProcessors<int>();
			processors.Add(ex => ex.Data["processed"] = true);

			var services = new ServiceCollection();
			services.AddPipelineFuncChain<int, int>(
				_ => PipelineFuncBuilder
					.StartWith<int, int>(i => i + 1)
					.AddFunc<int>(i => throw expectedException, new SimplePolicy()),
				bulkErrorProcessor =>
				{
					foreach (var processor in processors)
					{
						bulkErrorProcessor.AddProcessor(processor);
					}
				},
				"chainWithProcessors");
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>("chainWithProcessors");
			var result = pipeline.Invoke(0);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(expectedException.Data.Contains("processed"), Is.True);
			Assert.That(expectedException.Data["processed"], Is.EqualTo(true));
		}

		[Test]
		public void Should_ThrowWithHelpMessage_When_NoPipelineWasRegistered()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetPipelineFunc<int, int>());

			Assert.That(ex.Message, Does.Contain("AddPipelineFunc"));
		}

		[Test]
		public void Should_ReturnEmptyList_When_NoPipelineWasRegistered()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			Assert.That(serviceProvider.GetAllPipelineFuncs(), Is.Empty);
		}

		[Test]
		public void Should_ReturnAllPipelines_When_MultipleTypePairsRegistered()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i + 1);
			services.AddPipelineFunc<string, int>(s => s.Length);
			var serviceProvider = services.BuildServiceProvider();

			var all = serviceProvider.GetAllPipelineFuncs();

			Assert.That(all.Count, Is.EqualTo(2));
		}

		[Test]
		public void Should_ThrowWithRegisteredNames_When_DuplicateKeyIsMaterialized()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i + 1, "dup");
			services.AddPipelineFunc<int, int>(i => i + 2, "dup");
			var serviceProvider = services.BuildServiceProvider();

			var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetPipelineFunc<int, int>("dup"));

			Assert.That(ex.Message, Does.Contain("already registered"));
			Assert.That(ex.Message, Does.Contain("'dup'"));
		}

		[Test]
		public void Should_ResolveBothPipelines_When_SameTypePairRegisteredWithTwoNames()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i + 1, "plusOne");
			services.AddPipelineFunc<int, int>(i => i + 2, "plusTwo");
			var serviceProvider = services.BuildServiceProvider();

			var first = serviceProvider.GetPipelineFunc<int, int>("plusOne");
			var second = serviceProvider.GetPipelineFunc<int, int>("plusTwo");

			Assert.That(first.Invoke(10).Result, Is.EqualTo(11));
			Assert.That(second.Invoke(10).Result, Is.EqualTo(12));
			Assert.That(serviceProvider.GetAllPipelineFuncs().Count, Is.EqualTo(2));
		}

		[Test]
		public void Should_WrapResultInTask_When_UsedViaInvokeAsync()
		{
			var services = new ServiceCollection();
			services.AddPipelineFunc<int, int>(i => i * 3);
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = serviceProvider.GetPipelineFunc<int, int>();
			var task = pipeline.InvokeAsync(7);

			Assert.That(task.IsCompleted, Is.True);
			Assert.That(task.Result.Result, Is.EqualTo(21));
			Assert.That(task.Result.IsFailed, Is.False);
		}
	}
}