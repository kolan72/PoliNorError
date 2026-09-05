using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class SimplePolicyWithTypedErrorProcessorTests
	{
		[Test]
		public void Should_Register_Action_Processor_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;

			SimplePolicy policy = new SimplePolicy()
				.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
				{
					capturedError = ex;
					capturedInfo = info;
				});

			PolicyResult result = policy.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(result.NoError, Is.False);
			Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(capturedInfo, Is.Not.Null);
		}

		[Test]
		public void Should_Register_Action_Processor_With_Token_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;
			CancellationToken receivedToken = default;

			using (var cts = new CancellationTokenSource())
			{
				SimplePolicy policy = new SimplePolicy()
					.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
					{
						capturedError = ex;
						capturedInfo = info;
						receivedToken = token;
					});

				PolicyResult result = policy.Handle(() => throw new InvalidOperationException("typed"), cts.Token);

				Assert.That(result.NoError, Is.False);
				Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(capturedInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_Register_Action_Processor_With_CancellationType_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;

			SimplePolicy policy = new SimplePolicy()
				.WithTypedErrorProcessorOf<InvalidOperationException>(
					(ex, info) =>
					{
						capturedError = ex;
						capturedInfo = info;
					},
					CancellationType.Precancelable);

			PolicyResult result = policy.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(result.NoError, Is.False);
			Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(capturedInfo, Is.Not.Null);
		}

		[Test]
		public async Task Should_Register_Async_Processor_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;

			SimplePolicy policy = new SimplePolicy()
				.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
				{
					await Task.Delay(1);
					capturedError = ex;
					capturedInfo = info;
				});

			PolicyResult result = await policy.HandleAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(result.NoError, Is.False);
			Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(capturedInfo, Is.Not.Null);
		}

		[Test]
		public async Task Should_Register_Async_Processor_With_CancellationType_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;

			SimplePolicy policy = new SimplePolicy()
				.WithTypedErrorProcessorOf<InvalidOperationException>(
					async (ex, info) =>
					{
						await Task.Delay(1);
						capturedError = ex;
						capturedInfo = info;
					},
					CancellationType.Precancelable);

			PolicyResult result = await policy.HandleAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(result.NoError, Is.False);
			Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(capturedInfo, Is.Not.Null);
		}

		[Test]
		public async Task Should_Register_Async_Processor_With_Token_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;
			CancellationToken receivedToken = default;

			using (var cts = new CancellationTokenSource())
			{
				SimplePolicy policy = new SimplePolicy()
					.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
					{
						await Task.Delay(1);
						capturedError = ex;
						capturedInfo = info;
						receivedToken = token;
					});

				PolicyResult result = await policy.HandleAsync(
					async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					},
					false,
					cts.Token);

				Assert.That(result.NoError, Is.False);
				Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(capturedInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_Register_DefaultTypedErrorProcessor_For_Typed_Error()
		{
			InvalidOperationException capturedError = null;
			ProcessingErrorInfo capturedInfo = null;

			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) =>
			{
				capturedError = ex;
				capturedInfo = info;
			});

			SimplePolicy policy = new SimplePolicy()
				.WithTypedErrorProcessor(errorProcessor);

			PolicyResult result = policy.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(result.NoError, Is.False);
			Assert.That(capturedError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(capturedInfo, Is.Not.Null);
		}
	}
}
