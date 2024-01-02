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
		[TestCase(FallbackType.BaseClass, true, true)]
		[TestCase(FallbackType.BaseClass, true, false)]
		[TestCase(FallbackType.BaseClass, false, false)]
		[TestCase(FallbackType.BaseClass, false, true)]
		[TestCase(FallbackType.Creator, true, true)]
		[TestCase(FallbackType.Creator, true, false)]
		[TestCase(FallbackType.Creator, false, false)]
		[TestCase(FallbackType.Creator, false, true)]
		[TestCase(FallbackType.WithAsyncFunc, true, true)]
		[TestCase(FallbackType.WithAsyncFunc, true, false)]
		[TestCase(FallbackType.WithAsyncFunc, false, false)]
		[TestCase(FallbackType.WithAsyncFunc, false, true)]
		[TestCase(FallbackType.WithAction, true, true)]
		[TestCase(FallbackType.WithAction, true, false)]
		[TestCase(FallbackType.WithAction, false, false)]
		[TestCase(FallbackType.WithAction, false, true)]
		public async Task Should_Fallback_WithInnerErrorProcessor_HandleError_Correctly(FallbackType fallbackType, bool sync, bool withCancellationType)
		{
			async Task shorthandHandlerFunc<T>(T pol) where T : FallbackPolicyBase, IWithInnerErrorProcessor<T>
			{
				await PolicyWithInnerErrorProcessorForTest.Handle(pol, sync, withCancellationType);
			}

			switch (fallbackType)
			{
				case FallbackType.BaseClass:
					await shorthandHandlerFunc(new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1)).WithFallbackAction((_) => { }));
					break;
				case FallbackType.Creator:
					await shorthandHandlerFunc(new FallbackPolicy().WithFallbackFunc(() => 1));
					break;
				case FallbackType.WithAsyncFunc:
					await shorthandHandlerFunc(new FallbackPolicy().WithAsyncFallbackFunc(async () => await Task.Delay(1)));
					break;
				case FallbackType.WithAction:
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

		public enum FallbackType
		{
			BaseClass,
			Creator,
			WithAsyncFunc,
			WithAction
		}
	}
}
