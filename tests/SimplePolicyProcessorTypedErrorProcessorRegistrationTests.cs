using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class SimplePolicyProcessorTypedErrorProcessorRegistrationTests
	{
		[Test]
		public void Should_Register_Action_Processor_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;

			var registeredProcessor = processor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
			{
				handledError = ex;
				processingErrorInfo = info;
			});

			var result = processor.Execute(() => throw new InvalidOperationException("typed"));

			Assert.That(registeredProcessor, Is.SameAs(processor));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Should_Register_Action_Processor_With_CancellationType_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;

			var registeredProcessor = processor.WithTypedErrorProcessorOf<InvalidOperationException>(
				(ex, info) =>
				{
					handledError = ex;
					processingErrorInfo = info;
				},
				CancellationType.Precancelable);

			var result = processor.Execute(() => throw new InvalidOperationException("typed"));

			Assert.That(registeredProcessor, Is.SameAs(processor));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Should_Register_Action_Processor_With_Token_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;

			using (var cts = new CancellationTokenSource())
			{
				var registeredProcessor = processor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
				{
					handledError = ex;
					processingErrorInfo = info;
					receivedToken = token;
				});

				var result = processor.Execute(() => throw new InvalidOperationException("typed"), cts.Token);

				Assert.That(registeredProcessor, Is.SameAs(processor));
				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(handledError.Message, Is.EqualTo("typed"));
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
				Assert.That(result.IsSuccess, Is.True);
			}
		}

		[Test]
		public async Task Should_Register_Async_Processor_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;

			var registeredProcessor = processor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
			{
				await Task.Delay(1);
				handledError = ex;
				processingErrorInfo = info;
			});

			var result = await processor.ExecuteAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(registeredProcessor, Is.SameAs(processor));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public async Task Should_Register_Async_Processor_With_CancellationType_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;

			var registeredProcessor = processor.WithTypedErrorProcessorOf<InvalidOperationException>(
				async (ex, info) =>
				{
					await Task.Delay(1);
					handledError = ex;
					processingErrorInfo = info;
				},
				CancellationType.Precancelable);

			var result = await processor.ExecuteAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(registeredProcessor, Is.SameAs(processor));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public async Task Should_Register_Async_Processor_With_Token_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;

			using (var cts = new CancellationTokenSource())
			{
				var registeredProcessor = processor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
				{
					await Task.Delay(1);
					handledError = ex;
					processingErrorInfo = info;
					receivedToken = token;
				});

				var result = await processor.ExecuteAsync(
					async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					},
					false,
					cts.Token);

				Assert.That(registeredProcessor, Is.SameAs(processor));
				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(handledError.Message, Is.EqualTo("typed"));
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
				Assert.That(result.IsSuccess, Is.True);
			}
		}

		[Test]
		public void Should_Register_DefaultTypedErrorProcessor_For_Typed_Error()
		{
			var processor = new SimplePolicyProcessor();
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) =>
			{
				handledError = ex;
				processingErrorInfo = info;
			});

			var registeredProcessor = processor.WithTypedErrorProcessor<InvalidOperationException>(errorProcessor);

			var result = processor.Execute(() => throw new InvalidOperationException("typed"));

			Assert.That(registeredProcessor, Is.SameAs(processor));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(processingErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Simple));
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}
	}
}
