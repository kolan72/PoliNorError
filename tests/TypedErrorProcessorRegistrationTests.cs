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

		[Test]
		public void Should_WithTypedErrorProcessorOf_Handle_Typed_Exception_For_Single_Policy_Collection()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						exceptionHandled = true;
						processingErrorInfo = info;
					});

			var result = policyCollection.HandleDelegate(() => throw new InvalidOperationException("typed"));

			Assert.That(exceptionHandled, Is.True);
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_Handle_Typed_Exception_For_Multiple_Policies()
		{
			var exceptionHandled = false;

			var policyCollection = PolicyCollection.Create();
			for (int i = 0; i < 2; i++)
			{
				policyCollection.WithSimple().SetPolicyResultFailedIf((_) => true);
			}

			policyCollection
				.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { exceptionHandled = true; })
				.WithSimple();

			var result = policyCollection.HandleDelegate(() => throw new InvalidOperationException("typed"));

			Assert.That(exceptionHandled, Is.True);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_CancellationType_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						exceptionHandled = true;
						processingErrorInfo = info;
					}, CancellationType.Precancelable);

			var result = policyCollection.HandleDelegate(() => throw new InvalidOperationException("typed"));

			Assert.That(exceptionHandled, Is.True);
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_CancellationToken_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;

			using (var cts = new CancellationTokenSource())
			{
				var policyCollection = PolicyCollection
						.Create()
						.WithSimple()
						.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
						{
							exceptionHandled = true;
							processingErrorInfo = info;
							receivedToken = token;
						});

				var result = policyCollection.HandleDelegate(() => throw new InvalidOperationException("typed"), cts.Token);

				Assert.That(exceptionHandled, Is.True);
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
			}
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_Async_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						exceptionHandled = true;
						processingErrorInfo = info;
					});

			var result = await policyCollection.HandleDelegateAsync((Func<CancellationToken, Task>)(async (_) => { await Task.Delay(1); throw new InvalidOperationException("typed"); }));

			Assert.That(exceptionHandled, Is.True);
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_Pass_Exception_Message()
		{
			var exceptionMessage = string.Empty;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						exceptionMessage = ex.Message;
					});

			policyCollection.HandleDelegate(() => throw new InvalidOperationException("expected message"));

			Assert.That(exceptionMessage, Is.EqualTo("expected message"));
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_DefaultTypedErrorProcessor_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;

			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) =>
			{
				exceptionHandled = true;
				processingErrorInfo = info;
			});

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessor(errorProcessor);

			var result = policyCollection.HandleDelegate(() => throw new InvalidOperationException("typed"));

			Assert.That(exceptionHandled, Is.True);
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_Not_Handle_NonMatching_Exception()
		{
			var processorCalled = false;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { processorCalled = true; });

			policyCollection.HandleDelegate(() => throw new ArgumentException("wrong type"));

			Assert.That(processorCalled, Is.False);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Fallback)]
		[TestCase(PolicyAlias.Retry)]
		public void Should_WithTypedErrorProcessorOf_Chainable_Return_PolicyCollection(PolicyAlias alias)
		{
			var policyCollection = PolicyCollection.Create();
			switch (alias)
			{
				case PolicyAlias.Simple:
					policyCollection = policyCollection.WithSimple();
					break;
				case PolicyAlias.Fallback:
					policyCollection = policyCollection.WithFallback(() => { });
					break;
				case PolicyAlias.Retry:
					policyCollection = policyCollection.WithRetry(1);
					break;
			}

			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { }), Is.SameAs(policyCollection));
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_Async_With_CancellationType_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						exceptionHandled = true;
						processingErrorInfo = info;
					}, CancellationType.Precancelable);

			var result = await policyCollection.HandleDelegateAsync(async (_) =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(exceptionHandled, Is.True);
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_Async_With_CancellationToken_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;

			using (var cts = new CancellationTokenSource())
			{
				var policyCollection = PolicyCollection
						.Create()
						.WithSimple()
						.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
						{
							await Task.Delay(1);
							exceptionHandled = true;
							processingErrorInfo = info;
							receivedToken = token;
						});

				var result = await policyCollection.HandleDelegateAsync(async (_) =>
				{
					await Task.Delay(1);
					throw new InvalidOperationException("typed");
				}, cts.Token);

				Assert.That(exceptionHandled, Is.True);
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
			}
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_Async_Pass_Exception_Message()
		{
			var exceptionMessage = string.Empty;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						exceptionMessage = ex.Message;
					});

			await policyCollection.HandleDelegateAsync(async (_) =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("expected message");
			});

			Assert.That(exceptionMessage, Is.EqualTo("expected message"));
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_Async_Not_Handle_NonMatching_Exception()
		{
			var processorCalled = false;

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						processorCalled = true;
					});

			await policyCollection.HandleDelegateAsync(async (_) =>
			{
				await Task.Delay(1);
				throw new ArgumentException("wrong type");
			});

			Assert.That(processorCalled, Is.False);
		}

		[Test]
		public async Task Should_WithTypedErrorProcessor_Async_With_DefaultTypedErrorProcessor_Handle_Typed_Exception()
		{
			var exceptionHandled = false;
			ProcessingErrorInfo processingErrorInfo = null;

			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>(async (ex, info) =>
			{
				await Task.Delay(1);
				exceptionHandled = true;
				processingErrorInfo = info;
			});

			var policyCollection = PolicyCollection
					.Create()
					.WithSimple()
					.WithTypedErrorProcessor(errorProcessor);

			var result = await policyCollection.HandleDelegateAsync(async (_) =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(exceptionHandled, Is.True);
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.LastPolicyResult.Errors.FirstOrDefault(), Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_On_Empty_PolicyCollection_Not_Throw()
		{
			var policyCollection = PolicyCollection.Create();

			Assert.That(() => policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { }), Throws.Nothing);
			Assert.That(() => policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) => { }), Throws.Nothing);
			Assert.That(() => policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { }, CancellationType.Precancelable), Throws.Nothing);
			Assert.That(() => policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => Task.CompletedTask), Throws.Nothing);
			Assert.That(() => policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => Task.CompletedTask, CancellationType.Precancelable), Throws.Nothing);
			Assert.That(() => policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) => Task.CompletedTask), Throws.Nothing);
			Assert.That(() => policyCollection.WithTypedErrorProcessor(new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) => { })), Throws.Nothing);
		}

		[Test]
		public void Should_All_WithTypedErrorProcessor_Overloads_Return_Same_PolicyCollection()
		{
			var policyCollection = PolicyCollection.Create().WithSimple();

			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { }), Is.SameAs(policyCollection));
			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) => { }), Is.SameAs(policyCollection));
			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => { }, CancellationType.Precancelable), Is.SameAs(policyCollection));
			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => Task.CompletedTask), Is.SameAs(policyCollection));
			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) => Task.CompletedTask, CancellationType.Precancelable), Is.SameAs(policyCollection));
			Assert.That(policyCollection.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) => Task.CompletedTask), Is.SameAs(policyCollection));
			Assert.That(policyCollection.WithTypedErrorProcessor(new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) => { })), Is.SameAs(policyCollection));
		}
	}
}
