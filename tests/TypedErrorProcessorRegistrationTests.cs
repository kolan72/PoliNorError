using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class TypedErrorProcessorRegistrationTests
	{
		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public void Should_Register_Action_Processor_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;
			IPolicyProcessor registeredProcessor = null;

			switch (alias)
			{
				case PolicyAlias.Simple:
					var simpleProcessor = new SimplePolicyProcessor();
					registeredProcessor = simpleProcessor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						handledError = ex;
						processingErrorInfo = info;
					});
					result = simpleProcessor.Execute(() => throw new InvalidOperationException("typed"));

					Assert.That(registeredProcessor, Is.SameAs(simpleProcessor));
					break;
				case PolicyAlias.Fallback:
					var fallbackProcessor = FallbackProcessor.CreateDefault();
					registeredProcessor = fallbackProcessor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						handledError = ex;
						processingErrorInfo = info;
					});

					result = fallbackProcessor.Fallback(() => throw new InvalidOperationException("typed"), (_) => { });

					Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));
					break;
				case PolicyAlias.Retry:
					var retryProcessor = RetryProcessor.CreateDefault();
					registeredProcessor = retryProcessor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						handledError = ex;
						processingErrorInfo = info;
					});

					result = retryProcessor.Retry(() => throw new InvalidOperationException("typed"), 1);

					Assert.That(registeredProcessor, Is.SameAs(retryProcessor));
					break;
			}
			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public void Should_Register_Action_Processor_With_CancellationType_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;
			IPolicyProcessor registeredProcessor = null;

			switch (alias)
			{
				case PolicyAlias.Simple:
					var simpleProcessor = new SimplePolicyProcessor();
					registeredProcessor = simpleProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
						(ex, info) =>
						{
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);

					result = simpleProcessor.Execute(() => throw new InvalidOperationException("typed"));

					Assert.That(registeredProcessor, Is.SameAs(simpleProcessor));
					break;
				case PolicyAlias.Fallback:
					var fallbackProcessor = FallbackProcessor.CreateDefault();
					registeredProcessor = fallbackProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
						(ex, info) =>
						{
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);

					result = fallbackProcessor.Fallback(() => throw new InvalidOperationException("typed"), (_) => { });

					Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));
					break;
				case PolicyAlias.Retry:
					var retryProcessor = RetryProcessor.CreateDefault();
					registeredProcessor = retryProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
						(ex, info) =>
						{
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);

					result = retryProcessor.Retry(() => throw new InvalidOperationException("typed"), 1);

					Assert.That(registeredProcessor, Is.SameAs(registeredProcessor));
					break;
			}

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public void Should_Register_Action_Processor_With_Token_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;
			IPolicyProcessor registeredProcessor = null;
			PolicyResult result = null;

			using (var cts = new CancellationTokenSource())
			{
				switch (alias)
				{
					case PolicyAlias.Simple:
						var simplePolicyProcessor = new SimplePolicyProcessor();

						registeredProcessor = simplePolicyProcessor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
						{
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});

						result = simplePolicyProcessor.Execute(() => throw new InvalidOperationException("typed"), cts.Token);

						Assert.That(registeredProcessor, Is.SameAs(simplePolicyProcessor));
						break;
					case PolicyAlias.Fallback:
						var fallbackProcessor = FallbackProcessor.CreateDefault();

						registeredProcessor = fallbackProcessor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
						{
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});

						result = fallbackProcessor.Fallback(() => throw new InvalidOperationException("typed"), (_) => { }, cts.Token);

						Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));
						break;
					case PolicyAlias.Retry:
						var retryProcessor = RetryProcessor.CreateDefault();

						registeredProcessor = retryProcessor.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
						{
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});

						result = retryProcessor.Retry(() => throw new InvalidOperationException("typed"), 1, cts.Token);
						Assert.That(registeredProcessor, Is.SameAs(retryProcessor));
						break;
				}

				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(handledError.Message, Is.EqualTo("typed"));
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public async Task Should_Register_Async_Processor_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			IPolicyProcessor registeredProcessor = null;
			PolicyResult result = null;

			switch (alias)
			{
				case PolicyAlias.Simple:
					var simpleProcessor = new SimplePolicyProcessor();
					registeredProcessor = simpleProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						handledError = ex;
						processingErrorInfo = info;
					});

					result = await simpleProcessor.ExecuteAsync(async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					});

					Assert.That(registeredProcessor, Is.SameAs(simpleProcessor));
					break;
				case PolicyAlias.Fallback:
					var fallbackProcessor = FallbackProcessor.CreateDefault();
					registeredProcessor = fallbackProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						handledError = ex;
						processingErrorInfo = info;
					});

					result = await fallbackProcessor.FallbackAsync(async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					}, async (_) => await Task.Delay(1));

					Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));
					break;
				case PolicyAlias.Retry:
					var retryProcessor = RetryProcessor.CreateDefault();
					registeredProcessor = retryProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						handledError = ex;
						processingErrorInfo = info;
					});

					result = await retryProcessor.RetryAsync(async (_) =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					}, 1);

					Assert.That(registeredProcessor, Is.SameAs(retryProcessor));
					break;
			}

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public async Task Should_Register_Async_Processor_With_CancellationType_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			IPolicyProcessor registeredProcessor = null;
			PolicyResult result = null;

			switch (alias)
			{
				case PolicyAlias.Simple:
					var simpleProcessor = new SimplePolicyProcessor();
					registeredProcessor = simpleProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
						async (ex, info) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);

					result = await simpleProcessor.ExecuteAsync(async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					});

					Assert.That(registeredProcessor, Is.SameAs(simpleProcessor));
					break;
				case PolicyAlias.Fallback:
					var fallbackProcessor = FallbackProcessor.CreateDefault();
					registeredProcessor = fallbackProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
						async (ex, info) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);

					result = await fallbackProcessor.FallbackAsync(async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					}, async (_) => await Task.Delay(1));

					Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));
					break;
				case PolicyAlias.Retry:
					var retryProcessor = RetryProcessor.CreateDefault();
					registeredProcessor = retryProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						handledError = ex;
						processingErrorInfo = info;
					},
					CancellationType.Precancelable);

					result = await retryProcessor.RetryAsync(async (_) =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					}, 1);
					Assert.That(registeredProcessor, Is.SameAs(retryProcessor));
					break;
			}

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public async Task Should_Register_Async_Processor_With_Token_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;
			IPolicyProcessor registeredProcessor = null;

			PolicyResult result = null;

			using (var cts = new CancellationTokenSource())
			{
				switch (alias)
				{
					case PolicyAlias.Simple:
						var simpleProcessor = new SimplePolicyProcessor();

						registeredProcessor = simpleProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});

						result = await simpleProcessor.ExecuteAsync(
							async _ =>
							{
								await Task.Delay(1);
								throw new InvalidOperationException("typed");
							},
							false,
							cts.Token);

						Assert.That(registeredProcessor, Is.SameAs(simpleProcessor));

						break;
					case PolicyAlias.Fallback:
						var fallbackProcessor = FallbackProcessor.CreateDefault();

						registeredProcessor = fallbackProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});

						result = await fallbackProcessor.FallbackAsync(
							async _ =>
							{
								await Task.Delay(1);
								throw new InvalidOperationException("typed");
							},
							async (_) => await Task.Delay(1),
							false,
							cts.Token);

						Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));

						break;
					case PolicyAlias.Retry:
						var retryProcessor = RetryProcessor.CreateDefault();

						registeredProcessor = retryProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});

						result = await retryProcessor.RetryAsync(
							async (_) =>
							{
								await Task.Delay(1);
								throw new InvalidOperationException("typed");
							},
							new RetryCountInfo(1),
							false,
							cts.Token);

						Assert.That(registeredProcessor, Is.SameAs(retryProcessor));

						break;
				}

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(receivedToken, Is.EqualTo(cts.Token));
			Assert.That(result.NoError, Is.False);
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public void Should_Register_DefaultTypedErrorProcessor_For_Typed_Error(PolicyAlias alias)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			IPolicyProcessor registeredProcessor = null;

			PolicyResult result = null;

			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) =>
			{
				handledError = ex;
				processingErrorInfo = info;
			});

			switch (alias)
			{
				case PolicyAlias.Simple:
					var simpleProcessor = new SimplePolicyProcessor();

					registeredProcessor = simpleProcessor.WithTypedErrorProcessor<InvalidOperationException>(errorProcessor);

					result = simpleProcessor.Execute(() => throw new InvalidOperationException("typed"));

					Assert.That(registeredProcessor, Is.SameAs(simpleProcessor));
					break;
				case PolicyAlias.Fallback:
					var fallbackProcessor = FallbackProcessor.CreateDefault();

					registeredProcessor = fallbackProcessor.WithTypedErrorProcessor(errorProcessor);

					result = fallbackProcessor.Fallback(() => throw new InvalidOperationException("typed"), (_) => { });

					Assert.That(registeredProcessor, Is.SameAs(fallbackProcessor));
					break;
				case PolicyAlias.Retry:
					var retryProcessor = RetryProcessor.CreateDefault();
					registeredProcessor = retryProcessor.WithTypedErrorProcessor(errorProcessor);

					result = retryProcessor.Retry(() => throw new InvalidOperationException("typed"), 1);
					Assert.That(registeredProcessor, Is.SameAs(retryProcessor));
					break;
			}

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}
	}
}
