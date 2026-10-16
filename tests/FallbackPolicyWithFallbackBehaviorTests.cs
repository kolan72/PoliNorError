using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackPolicyWithFallbackBehaviorTests
	{
		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		[TestCase(FallbackTypeForTests.WithAction)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc)]
		public void Should_Return_Same_Policy_For_Chaining(FallbackTypeForTests fallbackType)
		{
			var behavior = FallbackBehavior<int>.Create(42);

			FallbackPolicyBase fallbackPolicyBase = null;
			FallbackPolicy fallbackPolicy = null;
			FallbackPolicyWithAction fallbackPolicyWithAction = null;
			FallbackPolicyWithAsyncFunc fallbackPolicyWithAsyncFunc = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicy = new FallbackPolicy();
					var registeredPolicy = fallbackPolicy.WithFallbackBehavior(behavior);
					Assert.That(registeredPolicy, Is.SameAs(fallbackPolicy));
					Assert.That(registeredPolicy, Is.AssignableTo<FallbackPolicy>());
					fallbackPolicyBase = fallbackPolicy;
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					var registeredPolicyBase = fallbackPolicyBase.WithFallbackBehavior(behavior);
					Assert.That(registeredPolicyBase, Is.SameAs(fallbackPolicyBase));
					Assert.That(registeredPolicyBase, Is.AssignableTo<FallbackPolicyBase>());
					break;
				case FallbackTypeForTests.WithAction:
					fallbackPolicyWithAction = new FallbackPolicy().WithFallbackAction(() => { });
					var registeredPolicyWithAction = fallbackPolicyWithAction.WithFallbackBehavior(behavior);
					Assert.That(registeredPolicyWithAction, Is.SameAs(fallbackPolicyWithAction));
					Assert.That(registeredPolicyWithAction, Is.AssignableTo<FallbackPolicyWithAction>());
					fallbackPolicyBase = fallbackPolicyWithAction;
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					fallbackPolicyWithAsyncFunc = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask);
					var registeredPolicyWithAsyncFunc = fallbackPolicyWithAsyncFunc.WithFallbackBehavior(behavior);
					Assert.That(registeredPolicyWithAsyncFunc, Is.SameAs(fallbackPolicyWithAsyncFunc));
					Assert.That(registeredPolicyWithAsyncFunc, Is.AssignableTo<FallbackPolicyWithAsyncFunc>());
					fallbackPolicyBase = fallbackPolicyWithAsyncFunc;
					break;
			}

			Assert.That(fallbackPolicyBase, Is.Not.Null);
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		[TestCase(FallbackTypeForTests.WithAction)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc)]
		public void Should_Return_Sync_Fallback_Value_When_Error_Occurs(FallbackTypeForTests fallbackType)
		{
			const int expectedValue = 42;
			var behavior = FallbackBehavior<int>.Create(expectedValue);

			FallbackPolicyBase fallbackPolicyBase = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicyBase = new FallbackPolicy().WithFallbackBehavior(behavior);
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy()
						.WithAsyncFallbackFunc((_) => Task.CompletedTask)
						.WithFallbackAction(() => { })
						.WithFallbackBehavior(behavior);
					break;
				case FallbackTypeForTests.WithAction:
					fallbackPolicyBase = new FallbackPolicy().WithFallbackAction(() => { }).WithFallbackBehavior(behavior);
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackBehavior(behavior);
					break;
			}

			var result = fallbackPolicyBase.Handle((Func<int>)(() => throw new Exception("fail")));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.BaseClass)]
		[TestCase(FallbackTypeForTests.WithAction)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc)]
		public async Task Should_Return_Async_Fallback_Value_When_Error_Occurs(FallbackTypeForTests fallbackType)
		{
			const int expectedValue = 42;
			var behavior = FallbackBehavior<int>.CreateAsync(expectedValue);

			FallbackPolicyBase fallbackPolicyBase = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.Creator:
					fallbackPolicyBase = new FallbackPolicy().WithFallbackBehavior(behavior);
					break;
				case FallbackTypeForTests.BaseClass:
					fallbackPolicyBase = new FallbackPolicy()
						.WithAsyncFallbackFunc((_) => Task.CompletedTask)
						.WithFallbackAction(() => { })
						.WithFallbackBehavior(behavior);
					break;
				case FallbackTypeForTests.WithAction:
					fallbackPolicyBase = new FallbackPolicy().WithFallbackAction(() => { }).WithFallbackBehavior(behavior);
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					fallbackPolicyBase = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackBehavior(behavior);
					break;
			}

			var result = await fallbackPolicyBase.HandleAsync<int>(async (_) =>
			{
				await Task.Delay(1);
				throw new Exception("fail");
			});

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
		}
	}
}
