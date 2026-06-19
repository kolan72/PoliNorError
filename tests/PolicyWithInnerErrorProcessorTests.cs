using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyWithInnerErrorProcessorTests
	{
		[Test]
		[TestCase(PolicyAlias.Simple, true, true)]
		[TestCase(PolicyAlias.Simple, true, false)]
		[TestCase(PolicyAlias.Simple, false, false)]
		[TestCase(PolicyAlias.Simple, false, true)]
		[TestCase(PolicyAlias.Retry, true, true)]
		[TestCase(PolicyAlias.Retry, true, false)]
		[TestCase(PolicyAlias.Retry, false, false)]
		[TestCase(PolicyAlias.Retry, false, true)]
		public async Task Should_WithInnerErrorProcessor_HandleError_Correctly(PolicyAlias policyAlias, bool sync, bool withCancellationType)
		{
			async Task shorthandHandlerFunc<T>(T pol) where T : IPolicyBase, IWithInnerErrorProcessor<T>
			{
				await PolicyWithInnerErrorProcessorForTest.Handle(pol, sync, withCancellationType);
			}
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					await shorthandHandlerFunc(new SimplePolicy());
					break;
				case PolicyAlias.Retry:
					await shorthandHandlerFunc(new RetryPolicy(1));
					break;
			}
		}

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass, true, true)]
		[TestCase(FallbackTypeForTests.BaseClass, true, false)]
		[TestCase(FallbackTypeForTests.BaseClass, false, false)]
		[TestCase(FallbackTypeForTests.BaseClass, false, true)]
		[TestCase(FallbackTypeForTests.Creator, true, true)]
		[TestCase(FallbackTypeForTests.Creator, true, false)]
		[TestCase(FallbackTypeForTests.Creator, false, false)]
		[TestCase(FallbackTypeForTests.Creator, false, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false, true)]
		[TestCase(FallbackTypeForTests.WithAction, true, true)]
		[TestCase(FallbackTypeForTests.WithAction, true, false)]
		[TestCase(FallbackTypeForTests.WithAction, false, false)]
		[TestCase(FallbackTypeForTests.WithAction, false, true)]
		public async Task Should_Fallback_WithInnerErrorProcessor_HandleError_Correctly(FallbackTypeForTests fallbackType, bool sync, bool withCancellationType)
		{
			async Task shorthandHandlerFunc<T>(T pol) where T : FallbackPolicyBase, IWithInnerErrorProcessor<T>
			{
				await PolicyWithInnerErrorProcessorForTest.Handle(pol, sync, withCancellationType);
			}

			switch (fallbackType)
			{
				case FallbackTypeForTests.BaseClass:
					await shorthandHandlerFunc(new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { }));
					break;
				case FallbackTypeForTests.Creator:
					await shorthandHandlerFunc(new FallbackPolicy().WithFallbackFunc(() => 1));
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					await shorthandHandlerFunc(new FallbackPolicy().WithAsyncFallbackFunc(async () => await Task.Delay(1)));
					break;
				case FallbackTypeForTests.WithAction:
					await shorthandHandlerFunc(new FallbackPolicy().WithFallbackAction(() => {}));
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public async Task Handle<T>(T policy, bool sync, bool withCancellationType) where T : FallbackPolicyBase, IWithInnerErrorProcessor<T>
		{
			await PolicyWithInnerErrorProcessorForTest.Handle(policy, sync, withCancellationType);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static void Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Sync_Handle_Matching_Inner_Exception(PolicyAlias policyAlias)
		{
			var innerExceptionHandled = false;
			var innerException = new InvalidOperationException("Inner exception");
			var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
				(InvalidOperationException ex, ProcessingErrorInfo _) =>
				{
					innerExceptionHandled = true;
					Assert.That(ex.Message, Is.EqualTo("Inner exception"));
				});

			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					var simplePolicy = new SimplePolicy();

					simplePolicy.WithInnerErrorProcessor(innerErrorProcessor);

					result = simplePolicy.Handle(() => throw new AggregateException("Main exception", innerException));

					Assert.That(result.Errors.Count(), Is.EqualTo(1));
					break;

				case PolicyAlias.Retry:
					var retryPolicy = new RetryPolicy(1);

					retryPolicy.WithInnerErrorProcessor(innerErrorProcessor);

					result = retryPolicy.Handle(() => throw new AggregateException("Main exception", innerException));

					Assert.That(result.Errors.Count(), Is.EqualTo(2));
					break;
			}

			Assert.That(innerExceptionHandled, Is.True);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static async Task Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Async_Handle_Matching_Inner_Exception(PolicyAlias policyAlias)
		{
			var innerExceptionHandled = false;

			var innerException = new InvalidOperationException("Inner exception");
			var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
				async (InvalidOperationException ex, ProcessingErrorInfo _) =>
				{
					await Task.Delay(1);
					innerExceptionHandled = true;
					Assert.That(ex.Message, Is.EqualTo("Inner exception"));
				});

			PolicyResult result;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					var simplePolicy = new SimplePolicy();
					simplePolicy.WithInnerErrorProcessor(innerErrorProcessor);
					result = await simplePolicy.HandleAsync(async (_) => { await Task.Delay(1); throw new AggregateException("Main exception", innerException); });

					Assert.That(result.Errors.Count(), Is.EqualTo(1));
					break;
				case PolicyAlias.Retry:
					var retryPolicy = new RetryPolicy(1);
					retryPolicy.WithInnerErrorProcessor(innerErrorProcessor);
					result = await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); throw new AggregateException("Main exception", innerException); });

					Assert.That(result.Errors.Count(), Is.EqualTo(2));
					break;
			}
			Assert.That(innerExceptionHandled, Is.True);
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static void Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Not_Handle_When_Inner_Exception_Type_Does_Not_Match(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var processorCalled = false;

						var nonMatchingInnerException = new ArgumentException("Wrong type");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							(InvalidOperationException _, ProcessingErrorInfo __) => processorCalled = true);

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = policy.Handle(() => throw new AggregateException("Main exception", nonMatchingInnerException));

						Assert.That(processorCalled, Is.False);
						Assert.That(result.Errors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var processorCalled = false;

						var nonMatchingInnerException = new ArgumentException("Wrong type");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							(InvalidOperationException _, ProcessingErrorInfo __) => processorCalled = true);

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = policy.Handle(() => throw new AggregateException("Main exception", nonMatchingInnerException));

						Assert.That(processorCalled, Is.False);
						Assert.That(result.Errors.Count(), Is.EqualTo(2));
						break;
					}
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static void Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Not_Handle_When_No_Inner_Exception(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var processorCalled = false;

						var innerErrorProcessor = new DefaultInnerErrorProcessor<ArgumentException>(
							(ArgumentException _, ProcessingErrorInfo __) => processorCalled = true);

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = policy.Handle(() => throw new InvalidOperationException("Exception without inner exception"));

						Assert.That(processorCalled, Is.False);
						Assert.That(result.Errors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var processorCalled = false;

						var innerErrorProcessor = new DefaultInnerErrorProcessor<ArgumentException>(
							(ArgumentException _, ProcessingErrorInfo __) => processorCalled = true);

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = policy.Handle(() => throw new InvalidOperationException("Exception without inner exception"));

						Assert.That(processorCalled, Is.False);
						Assert.That(result.Errors.Count(), Is.EqualTo(2));
						break;
					}
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static void Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Work_With_Multiple_Inner_Exception_Types(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var invalidOperationExceptionHandled = false;
						var argumentExceptionHandled = false;

						var innerException = new InvalidOperationException("First inner");

						policy.WithInnerErrorProcessor(
							new DefaultInnerErrorProcessor<InvalidOperationException>(
								(InvalidOperationException ex, ProcessingErrorInfo _) =>
								{
									invalidOperationExceptionHandled = true;
									Assert.That(ex.Message, Is.EqualTo("First inner"));
								}));

						policy.WithInnerErrorProcessor(
							new DefaultInnerErrorProcessor<ArgumentException>(
								(ArgumentException _, ProcessingErrorInfo __) => argumentExceptionHandled = true));

						result = policy.Handle(() => throw new AggregateException("Main exception", innerException));

						Assert.That(invalidOperationExceptionHandled, Is.True);
						Assert.That(argumentExceptionHandled, Is.False);
						Assert.That(result.Errors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var invalidOperationExceptionHandled = false;
						var argumentExceptionHandled = false;

						var innerException = new InvalidOperationException("First inner");

						policy.WithInnerErrorProcessor(
							new DefaultInnerErrorProcessor<InvalidOperationException>(
								(InvalidOperationException ex, ProcessingErrorInfo _) =>
								{
									invalidOperationExceptionHandled = true;
									Assert.That(ex.Message, Is.EqualTo("First inner"));
								}));

						policy.WithInnerErrorProcessor(
							new DefaultInnerErrorProcessor<ArgumentException>(
								(ArgumentException _, ProcessingErrorInfo __) => argumentExceptionHandled = true));

						result = policy.Handle(() => throw new AggregateException("Main exception", innerException));

						Assert.That(invalidOperationExceptionHandled, Is.True);
						Assert.That(argumentExceptionHandled, Is.False);
						Assert.That(result.Errors.Count(), Is.EqualTo(2));
						break;
					}
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static void Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Handle_Exception_From_Inner_Processor(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var processorException = new ArgumentException("Processor error");

						var innerException = new InvalidOperationException("Inner exception");

						policy.WithInnerErrorProcessor(
							new DefaultInnerErrorProcessor<InvalidOperationException>(
								(InvalidOperationException _, ProcessingErrorInfo __) => throw processorException));

						result = policy.Handle(() => throw new AggregateException("Main exception", innerException));

						Assert.That(result.CatchBlockErrors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var processorException = new ArgumentException("Processor error");

						var innerException = new InvalidOperationException("Inner exception");

						policy.WithInnerErrorProcessor(
							new DefaultInnerErrorProcessor<InvalidOperationException>(
								(InvalidOperationException _, ProcessingErrorInfo __) => throw processorException));

						result = policy.Handle(() => throw new AggregateException("Main exception", innerException));

						Assert.That(result.CatchBlockErrors.Count(), Is.EqualTo(1));
						break;
					}
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static void Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Sync_With_CancellationToken_Handle_Matching_Inner_Exception(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var innerExceptionHandled = false;

						var innerException = new InvalidOperationException("Inner exception");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							(InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken __) =>
							{
								innerExceptionHandled = true;
								Assert.That(ex.Message, Is.EqualTo("Inner exception"));
							});

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = policy.Handle(() => throw new AggregateException("Main exception", innerException));

						Assert.That(innerExceptionHandled, Is.True);
						Assert.That(result.Errors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var innerExceptionHandled = false;

						var innerException = new InvalidOperationException("Inner exception");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							(InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken __) =>
							{
								innerExceptionHandled = true;
								Assert.That(ex.Message, Is.EqualTo("Inner exception"));
							});

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = policy.Handle(() => throw new AggregateException("Main exception", innerException));

						Assert.That(innerExceptionHandled, Is.True);
						Assert.That(result.Errors.Count(), Is.EqualTo(2));
						break;
					}
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static async Task Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Async_With_CancellationToken_Handle_Matching_Inner_Exception(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var innerExceptionHandled = false;

						var innerException = new InvalidOperationException("Inner exception");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							async (InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken __) =>
							{
								await Task.Delay(1);
								innerExceptionHandled = true;
								Assert.That(ex.Message, Is.EqualTo("Inner exception"));
							});

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new AggregateException("Main exception", innerException); });

						Assert.That(innerExceptionHandled, Is.True);
						Assert.That(result.Errors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var innerExceptionHandled = false;

						var innerException = new InvalidOperationException("Inner exception");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							async (InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken __) =>
							{
								await Task.Delay(1);
								innerExceptionHandled = true;
								Assert.That(ex.Message, Is.EqualTo("Inner exception"));
							});

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new AggregateException("Main exception", innerException); });

						Assert.That(innerExceptionHandled, Is.True);
						Assert.That(result.Errors.Count(), Is.EqualTo(2));
						break;
					}
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple)]
		[TestCase(PolicyAlias.Retry)]
		public static async Task Should_WithInnerErrorProcessor_DefaultInnerErrorProcessor_Async_With_CancellationType_Handle_Matching_Inner_Exception(PolicyAlias policyAlias)
		{
			PolicyResult result = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					{
						var policy = new SimplePolicy();
						var innerExceptionHandled = false;

						var innerException = new InvalidOperationException("Inner exception");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							async (InvalidOperationException ex, ProcessingErrorInfo _) =>
							{
								await Task.Delay(1);
								innerExceptionHandled = true;
								Assert.That(ex.Message, Is.EqualTo("Inner exception"));
							},
							CancellationType.Precancelable);

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new AggregateException("Main exception", innerException); });

						Assert.That(innerExceptionHandled, Is.True);
						Assert.That(result.Errors.Count(), Is.EqualTo(1));
						break;
					}
				case PolicyAlias.Retry:
					{
						var policy = new RetryPolicy(1);
						var innerExceptionHandled = false;

						var innerException = new InvalidOperationException("Inner exception");
						var innerErrorProcessor = new DefaultInnerErrorProcessor<InvalidOperationException>(
							async (InvalidOperationException ex, ProcessingErrorInfo _) =>
							{
								await Task.Delay(1);
								innerExceptionHandled = true;
								Assert.That(ex.Message, Is.EqualTo("Inner exception"));
							},
							CancellationType.Precancelable);

						policy.WithInnerErrorProcessor(innerErrorProcessor);
						result = await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new AggregateException("Main exception", innerException); });

						Assert.That(innerExceptionHandled, Is.True);
						Assert.That(result.Errors.Count(), Is.EqualTo(2));
						break;
					}
			}
		}
	}
}
