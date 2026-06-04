using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class RetryPolicyWithTypedErrorProcessorTests
	{
		[Test]
		public void Should_Register_Action_Processor_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			var retryPolicy = new RetryPolicy(1);
			var registeredPolicy = retryPolicy.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
			{
				handledError = ex;
				processingErrorInfo = info;
			});

			result = retryPolicy.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		public void Should_Register_Action_Processor_With_CancellationType_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			var retryPolicy = new RetryPolicy(1);
			var registeredPolicy = retryPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(
				(ex, info) =>
				{
					handledError = ex;
					processingErrorInfo = info;
				},
				CancellationType.Precancelable);

			result = retryPolicy.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		public void Should_Register_Action_Processor_With_Token_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;
			PolicyResult result = null;

			using (var cts = new CancellationTokenSource())
			{
				var retryPolicy = new RetryPolicy(1);
				var registeredPolicy = retryPolicy.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
				{
					handledError = ex;
					processingErrorInfo = info;
					receivedToken = token;
				});

				result = retryPolicy.Handle(() => throw new InvalidOperationException("typed"), cts.Token);

				Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(handledError.Message, Is.EqualTo("typed"));
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
			}
		}

		[Test]
		public async Task Should_Register_Async_Processor_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			var retryPolicy = new RetryPolicy(1);
			var registeredPolicy = retryPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
			{
				await Task.Delay(1);
				handledError = ex;
				processingErrorInfo = info;
			});

			result = await retryPolicy.HandleAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		public async Task Should_Register_Async_Processor_With_CancellationType_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			var retryPolicy = new RetryPolicy(1);
			var registeredPolicy = retryPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(
				async (ex, info) =>
				{
					await Task.Delay(1);
					handledError = ex;
					processingErrorInfo = info;
				},
				CancellationType.Precancelable);

			result = await retryPolicy.HandleAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		public async Task Should_Register_Async_Processor_With_Token_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;
			PolicyResult result = null;

			using (var cts = new CancellationTokenSource())
			{
				var retryPolicy = new RetryPolicy(1);

				var registeredPolicy = retryPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
				{
					await Task.Delay(1);
					handledError = ex;
					processingErrorInfo = info;
					receivedToken = token;
				});

				result = await retryPolicy.HandleAsync(
					async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					},
					false,
					cts.Token);

				Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(handledError.Message, Is.EqualTo("typed"));
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
			}
		}

		[Test]
		public void Should_Register_DefaultTypedErrorProcessor_For_Typed_Error()
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) =>
			{
				handledError = ex;
				processingErrorInfo = info;
			});

			var retryPolicy = new RetryPolicy(1);
			var registeredPolicy = retryPolicy.WithTypedErrorProcessor(errorProcessor);

			result = retryPolicy.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(registeredPolicy, Is.SameAs(retryPolicy));
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}
	}
}