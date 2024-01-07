using NUnit.Framework;
using System;
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
	}
}
