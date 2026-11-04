using NUnit.Framework;
using System;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class PipelineFuncRegistryTests
	{
		private static PipelineFunc<int, string> CreateIntToStringPipeline(string name = null)
		{
			return PipelineFuncBuilder.StartWith<int, string>(i => $"value:{i}").ToPipelineFunc(name);
		}

		[Test]
		public void Should_ReturnSameInstance_When_RegisteredUnderDefaultKey()
		{
			var registry = new PipelineFuncRegistry();
			var pipelineFunc = CreateIntToStringPipeline();

			registry.Register(pipelineFunc);

			Assert.That(registry.Get<int, string>(), Is.SameAs(pipelineFunc));
			Assert.That(registry.Contains<int, string>(), Is.True);
		}

		[Test]
		public void Should_UseInputTypeName_AsDefaultKey()
		{
			var registry = new PipelineFuncRegistry();
			registry.Register(CreateIntToStringPipeline());

			var ex = Assert.Throws<InvalidOperationException>(() => registry.Get<int, string>("SimplePolicy"));

			Assert.That(ex.Message, Does.Contain("'Int32'"));
		}

		[Test]
		public void Should_ReturnSameInstance_When_RegisteredWithExplicitName()
		{
			var registry = new PipelineFuncRegistry();
			var pipelineFunc = CreateIntToStringPipeline("first");

			registry.Register(pipelineFunc, "first");

			Assert.That(registry.Get<int, string>("first"), Is.SameAs(pipelineFunc));
			Assert.That(registry.Contains<int, string>("first"), Is.True);
		}

		[Test]
		public void Should_TreatNullName_AsDefaultKey()
		{
			var registry = new PipelineFuncRegistry();
			var pipelineFunc = CreateIntToStringPipeline("ignored");

			registry.Register(pipelineFunc, null);

			Assert.That(registry.Get<int, string>(), Is.SameAs(pipelineFunc));
		}

		[Test]
		public void Should_ThrowWithRegisteredNames_When_DefaultKeyIsMissing()
		{
			var registry = new PipelineFuncRegistry();
			registry.Register(CreateIntToStringPipeline(), "named");

			var ex = Assert.Throws<InvalidOperationException>(() => registry.Get<int, string>());

			Assert.That(ex.Message, Does.Contain("'Int32'"));
			Assert.That(ex.Message, Does.Contain("'named'"));
		}

		[Test]
		public void Should_ThrowWithNoRegistrationsMessage_When_TypePairIsUnknown()
		{
			var registry = new PipelineFuncRegistry();

			var ex = Assert.Throws<InvalidOperationException>(() => registry.Get<int, string>());

			Assert.That(ex.Message, Does.Contain("no registrations"));
		}

		[Test]
		public void Should_ThrowWithRegisteredNames_When_DuplicateDefaultKeyIsRegistered()
		{
			var registry = new PipelineFuncRegistry();
			registry.Register(CreateIntToStringPipeline());

			var ex = Assert.Throws<InvalidOperationException>(() => registry.Register(CreateIntToStringPipeline()));

			Assert.That(ex.Message, Does.Contain("already registered"));
			Assert.That(ex.Message, Does.Contain("'Int32'"));
		}

		[Test]
		public void Should_ThrowWithRegisteredNames_When_DuplicateExplicitKeyIsRegistered()
		{
			var registry = new PipelineFuncRegistry();
			registry.Register(CreateIntToStringPipeline(), "dup");

			var ex = Assert.Throws<InvalidOperationException>(() => registry.Register(CreateIntToStringPipeline(), "dup"));

			Assert.That(ex.Message, Does.Contain("already registered"));
			Assert.That(ex.Message, Does.Contain("'dup'"));
		}

		[Test]
		public void Should_AllowSameName_When_TypePairsDiffer()
		{
			var registry = new PipelineFuncRegistry();
			var intToString = CreateIntToStringPipeline("shared");
			var stringToInt = PipelineFuncBuilder.StartWith<string, int>(s => s.Length).ToPipelineFunc("shared");

			registry.Register(intToString, "shared");
			registry.Register(stringToInt, "shared");

			Assert.That(registry.Get<int, string>("shared"), Is.SameAs(intToString));
			Assert.That(registry.Get<string, int>("shared"), Is.SameAs(stringToInt));
		}

		[Test]
		public void Should_ReturnAllRegisteredPipelines()
		{
			var registry = new PipelineFuncRegistry();
			var first = CreateIntToStringPipeline();
			var second = PipelineFuncBuilder.StartWith<string, int>(s => s.Length).ToPipelineFunc("second");

			registry.Register(first);
			registry.Register(second, "second");

			var all = registry.GetAllPipelineFuncs();

			Assert.That(all.Count, Is.EqualTo(2));
			Assert.That(all, Does.Contain(first));
			Assert.That(all, Does.Contain(second));
		}

		[Test]
		public void Should_ReturnEmptyList_When_NothingIsRegistered()
		{
			var registry = new PipelineFuncRegistry();

			Assert.That(registry.GetAllPipelineFuncs(), Is.Empty);
		}

		[Test]
		public void Should_ForgetValue_When_UnregisteredByName()
		{
			var registry = new PipelineFuncRegistry();
			registry.Register(CreateIntToStringPipeline(), "temporary");

			registry.Unregister<int, string>("temporary");

			Assert.That(registry.Contains<int, string>("temporary"), Is.False);
			Assert.That(() => registry.Get<int, string>("temporary"), Throws.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_ForgetValue_When_UnregisteredByDefaultKey()
		{
			var registry = new PipelineFuncRegistry();
			registry.Register(CreateIntToStringPipeline());

			registry.Unregister<int, string>();

			Assert.That(registry.Contains<int, string>(), Is.False);
			Assert.That(() => registry.Get<int, string>(), Throws.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_BeNoOp_When_UnregisteringUnknownKey()
		{
			var registry = new PipelineFuncRegistry();

			Assert.That(() => registry.Unregister<int, string>("absent"), Throws.Nothing);
			Assert.That(() => registry.Unregister<int, string>(), Throws.Nothing);
		}

		[Test]
		public void Should_ReturnFalse_When_ContainsCheckedForUnknownTypePair()
		{
			var registry = new PipelineFuncRegistry();

			Assert.That(registry.Contains<int, string>(), Is.False);
			Assert.That(registry.Contains<int, string>("any"), Is.False);
		}

		[Test]
		public void Should_ThrowArgumentNullException_When_PipelineIsNull()
		{
			var registry = new PipelineFuncRegistry();

			Assert.That(() => registry.Register<int, string>(null), Throws.ArgumentNullException);
		}
	}
}