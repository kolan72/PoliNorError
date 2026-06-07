using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackPolicyWithTypedErrorProcessorTests
	{
		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public void Should_Register_Action_Processor_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicy = new FallbackPolicy();
					var registeredPolicy = fallbackPolicy.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						handledError = ex;
						processingErrorInfo = info;
					});
					Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
					fallbackPolicyBase = fallbackPolicy;
					Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info) =>
					{
						handledError = ex;
						processingErrorInfo = info;
					});
					Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
					Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
					break;
			}

			result = fallbackPolicyBase.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			if (result.Errors.Any())
			{
				Assert.That(handledError.Message, Is.EqualTo("typed"));
			}
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public void Should_Register_Action_Processor_With_CancellationType_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicy = new FallbackPolicy();
					var registeredPolicy = fallbackPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(
						(ex, info) =>
						{
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);
					Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
					Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
					fallbackPolicyBase = fallbackPolicy;
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessorOf<InvalidOperationException>(
						(ex, info) =>
						{
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);
					Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
					Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
					break;
			}

			result = fallbackPolicyBase.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			if (result.Errors.Any())
			{
				Assert.That(handledError.Message, Is.EqualTo("typed"));
			}
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public void Should_Register_Action_Processor_With_Token_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;
			PolicyResult result = null;

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			using (var cts = new CancellationTokenSource())
			{
				switch (fallbackType)
				{
					case FallbackTypeForTests.Creator:
						fallbackPolicy = new FallbackPolicy();
						var registeredPolicy = fallbackPolicy.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
						{
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});
						Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
						Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
						fallbackPolicyBase = fallbackPolicy;
						break;
					case FallbackTypeForTests.BaseClass:
						fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
						var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessorOf<InvalidOperationException>((ex, info, token) =>
						{
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});
						Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
						Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
						break;
				}
				result = fallbackPolicyBase.Handle(() => throw new InvalidOperationException("typed"), cts.Token);

				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				if (result.Errors.Any())
				{
					Assert.That(handledError.Message, Is.EqualTo("typed"));
				}
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
			}
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public async Task Should_Register_Async_Processor_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicy = new FallbackPolicy();
					var registeredPolicy = fallbackPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						handledError = ex;
						processingErrorInfo = info;
					});
					Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
					Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
					fallbackPolicyBase = fallbackPolicy;
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info) =>
					{
						await Task.Delay(1);
						handledError = ex;
						processingErrorInfo = info;
					});
					Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
					Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
					break;
			}

			result = await fallbackPolicyBase.HandleAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public async Task Should_Register_Async_Processor_With_CancellationType_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicy = new FallbackPolicy();
					var registeredPolicy = fallbackPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(
						async (ex, info) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);
					Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
					Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
					fallbackPolicyBase = fallbackPolicy;
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessorOf<InvalidOperationException>(
						async (ex, info) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
						},
						CancellationType.Precancelable);
					Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
					Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
					break;
			}

			result = await fallbackPolicyBase.HandleAsync(async _ =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("typed");
			});

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public async Task Should_Register_Async_Processor_With_Token_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			CancellationToken receivedToken = default;
			PolicyResult result = null;

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			using (var cts = new CancellationTokenSource())
			{
				switch (fallbackType)
				{
					case FallbackTypeForTests.Creator:
						fallbackPolicy = new FallbackPolicy();

						var registeredPolicy = fallbackPolicy.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});
						Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
						Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
						fallbackPolicyBase = fallbackPolicy;
						break;
					case FallbackTypeForTests.BaseClass:
						fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
						var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessorOf<InvalidOperationException>(async (ex, info, token) =>
						{
							await Task.Delay(1);
							handledError = ex;
							processingErrorInfo = info;
							receivedToken = token;
						});
						Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
						Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
						break;
				}

				result = await fallbackPolicyBase.HandleAsync(
					async _ =>
					{
						await Task.Delay(1);
						throw new InvalidOperationException("typed");
					},
					false,
					cts.Token);

				Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
				Assert.That(handledError.Message, Is.EqualTo("typed"));
				Assert.That(processingErrorInfo, Is.Not.Null);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
				Assert.That(result.NoError, Is.False);
			}
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		public void Should_Register_DefaultTypedErrorProcessor_For_Typed_Error(FallbackTypeForTests fallbackType)
		{
			InvalidOperationException handledError = null;
			ProcessingErrorInfo processingErrorInfo = null;
			PolicyResult result = null;

			var errorProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((ex, info) =>
			{
				handledError = ex;
				processingErrorInfo = info;
			});

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicy = new FallbackPolicy();
					var registeredPolicy = fallbackPolicy.WithTypedErrorProcessor(errorProcessor);

					Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
					Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
					fallbackPolicyBase = fallbackPolicy;
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					var registeredPolicyBase = fallbackPolicyBase.WithTypedErrorProcessor(errorProcessor);

					Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
					Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
					break;
			}

			result = fallbackPolicyBase.Handle(() => throw new InvalidOperationException("typed"));

			Assert.That(handledError, Is.SameAs(result.Errors.FirstOrDefault()));
			Assert.That(handledError.Message, Is.EqualTo("typed"));
			Assert.That(processingErrorInfo, Is.Not.Null);
			Assert.That(result.NoError, Is.False);
		}
	}
}