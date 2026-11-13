using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	/// <summary>
	/// <para>Tests for <see cref="FallbackPolicyBase.ThenFallback(bool)"/>.</para>
	/// <para>
	/// Key behaviours under test:
	///   1. ThenFallback returns a new FallbackPolicy that wraps the source policy without modifying it.
	///   2. The onlyGenericFallbackForGenericDelegate argument is propagated to the returned policy.
	///   3. Handling through the returned policy produces WrappedPolicyResults of the source policy.
	///   4. The outer fallback recovers failures of the source policy (fallback-then-fallback pattern).
	///   5. onlyGenericFallbackForGenericDelegate controls whether non-generic fallbacks serve generic delegates.
	/// </para>
	/// </summary>
	[TestFixture]
	internal class FallbackPolicyBaseThenFallbackTests
	{
		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		private static FallbackPolicyBase CreateFailingInnerPolicy()
			=> new FallbackPolicy().ExcludeError<InvalidOperationException>();

		// -------------------------------------------------------------------------
		// Return value and onlyGenericFallbackForGenericDelegate propagation
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Return_FallbackPolicy_Instance()
		{
			FallbackPolicyBase source = new FallbackPolicy();

			var result = source.ThenFallback();

			Assert.That(result, Is.TypeOf<FallbackPolicy>());
			Assert.That(result.PolicyProcessor, Is.Not.Null);
		}

		[Test]
		public void Should_Return_Different_Instance_Than_Source_Policy()
		{
			FallbackPolicyBase source = new FallbackPolicy();

			var result = source.ThenFallback();

			Assert.That(result, Is.Not.SameAs(source));
		}

		[Test]
		public void Should_Set_OnlyGenericFallbackForGenericDelegate_False_By_Default()
		{
			FallbackPolicyBase source = new FallbackPolicy();

			var result = source.ThenFallback();

			Assert.That(result.OnlyGenericFallbackForGenericDelegate, Is.False);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Propagate_OnlyGenericFallbackForGenericDelegate_To_Returned_Policy(bool onlyGeneric)
		{
			FallbackPolicyBase source = new FallbackPolicy();

			var result = source.ThenFallback(onlyGeneric);

			Assert.That(result.OnlyGenericFallbackForGenericDelegate, Is.EqualTo(onlyGeneric));
		}

		// -------------------------------------------------------------------------
		// Wrapping semantics
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Not_Modify_Source_Policy_When_ThenFallback_Called()
		{
			FallbackPolicyBase source = new FallbackPolicy();

			source.ThenFallback();

			var sourceResult = source.Handle(() => { });

			Assert.That(sourceResult.WrappedPolicyResults, Is.Null);
		}

		[Test]
		public void Should_Wrap_Source_Policy_Producing_WrappedPolicyResults_When_Inner_Fails()
		{
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outer = inner.ThenFallback();

			var result = outer.Handle(() => throw new InvalidOperationException("fail"));

			Assert.That(result.WrappedPolicyResults, Is.Not.Null);
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(result.WrappedPolicyResults.First().Result.IsFailed, Is.True);
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Should_Produce_WrappedPolicyResults_When_NoError()
		{
			FallbackPolicyBase inner = new FallbackPolicy();
			var outer = inner.ThenFallback();

			var result = outer.Handle(() => { });

			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.NoError, Is.True);
		}

		[Test]
		public void Should_Not_Invoke_Outer_Fallback_When_Inner_Handles_Error_Itself()
		{
			var outerFallbackCalled = false;
			FallbackPolicyBase inner = new FallbackPolicy();
			var outer = inner.ThenFallback().WithFallbackFunc(() =>
			{
				outerFallbackCalled = true;
				return 42;
			});

			var result = outer.Handle<int>(() => throw new InvalidOperationException("handled by inner"));

			Assert.That(outerFallbackCalled, Is.False);
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.WrappedPolicyResults.First().Result.IsFailed, Is.False);
		}

		[Test]
		public void Should_Run_Inner_Fallback_Before_Outer_Fallback()
		{
			var innerFallbackCalled = false;
			var outerFallbackCalled = false;
			FallbackPolicyBase inner = new FallbackPolicy().WithFallbackFunc(() =>
			{
				innerFallbackCalled = true;
				return 1;
			});
			var outer = inner.ThenFallback().WithFallbackFunc(() =>
			{
				outerFallbackCalled = true;
				return 2;
			});

			var result = outer.Handle<int>(() => throw new InvalidOperationException("fail"));

			Assert.That(innerFallbackCalled, Is.True);
			Assert.That(outerFallbackCalled, Is.False);
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(1));
		}

		// -------------------------------------------------------------------------
		// Fallback-then-fallback recovery for sync and async delegates
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Recover_Inner_Failure_With_Outer_Sync_FallbackFunc_For_HandleT()
		{
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outer = inner.ThenFallback().WithFallbackFunc(() => 42);

			var result = outer.Handle<int>(() => throw new InvalidOperationException("fail"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(42));
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task Should_Recover_Inner_Failure_With_Outer_Async_Fallback_For_HandleAsync()
		{
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outerFallbackCalled = false;
			var outer = inner.ThenFallback();
			outer.WithAsyncFallbackFunc(async (_) =>
			{
				await Task.Delay(1);
				outerFallbackCalled = true;
			});
			Assert.That(outer.HasAsyncFallbackFunc(), Is.True);

			var result = await outer.HandleAsync(async (_) =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("fail");
			});

			Assert.That(result.IsFailed, Is.False);
			Assert.That(outerFallbackCalled, Is.True);
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task Should_Recover_Inner_Failure_With_Outer_Async_FallbackFunc_For_HandleAsyncT()
		{
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outer = inner.ThenFallback().WithAsyncFallbackFunc(async () =>
			{
				await Task.Delay(1);
				return 42;
			});

			var result = await outer.HandleAsync<int>(async (_) =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("fail");
			});

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(42));
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		// -------------------------------------------------------------------------
		// Policy names
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Set_Outer_Policy_Name_In_Result()
		{
			FallbackPolicyBase inner = new FallbackPolicy().WithPolicyName("InnerPol");
			var outer = inner.ThenFallback().WithPolicyName("OuterFb");

			var result = outer.Handle(() => { });

			Assert.That(result.PolicyName, Is.EqualTo("OuterFb"));
		}

		[Test]
		public void Should_Reflect_Inner_Policy_Name_In_WrappedPolicyResults()
		{
			FallbackPolicyBase inner = new FallbackPolicy().WithPolicyName("InnerPol");
			var outer = inner.ThenFallback();

			var result = outer.Handle(() => { });

			Assert.That(result.WrappedPolicyResults.First().PolicyName, Is.EqualTo("InnerPol"));
		}

		// -------------------------------------------------------------------------
		// Error filter of the outer policy
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Propagate_Inner_Failure_When_Outer_Error_Filter_Unsatisfied()
		{
			var outerFallbackCalled = false;
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outer = inner.ThenFallback()
				.ExcludeError<InvalidOperationException>()
				.WithFallbackFunc(() =>
				{
					outerFallbackCalled = true;
					return 42;
				});

			var result = outer.Handle<int>(() => throw new InvalidOperationException("fail"));

			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			Assert.That(outerFallbackCalled, Is.False);
			Assert.That(result.Errors.First(), Is.TypeOf<InvalidOperationException>());
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		// -------------------------------------------------------------------------
		// onlyGenericFallbackForGenericDelegate behaviour for generic delegates
		// -------------------------------------------------------------------------

		[Test]
		[TestCase(false, true)]
		[TestCase(true, false)]
		public void Should_Respect_OnlyGenericFallbackForGenericDelegate_For_HandleT(bool onlyGeneric, bool expectNonGenericCalled)
		{
			var nonGenericCalled = false;
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outer = inner.ThenFallback(onlyGeneric);
			outer.WithFallbackAction(() => nonGenericCalled = true);
			Assert.That(outer.HasFallbackAction(), Is.True);

			var result = outer.Handle<int>(() => throw new InvalidOperationException("fail"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(nonGenericCalled, Is.EqualTo(expectNonGenericCalled));
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		[Test]
		[TestCase(false, true)]
		[TestCase(true, false)]
		public async Task Should_Respect_OnlyGenericFallbackForGenericDelegate_For_HandleAsyncT(bool onlyGeneric, bool expectNonGenericCalled)
		{
			var nonGenericCalled = false;
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var outer = inner.ThenFallback(onlyGeneric);
			outer.WithAsyncFallbackFunc(async (_) =>
			{
				await Task.Delay(1);
				nonGenericCalled = true;
			});
			Assert.That(outer.HasAsyncFallbackFunc(), Is.True);

			var result = await outer.HandleAsync<int>(async (_) =>
			{
				await Task.Delay(1);
				throw new InvalidOperationException("fail");
			});

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(nonGenericCalled, Is.EqualTo(expectNonGenericCalled));
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		// -------------------------------------------------------------------------
		// Cancellation and null delegate
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Handle_Canceled_Token()
		{
			FallbackPolicyBase inner = new FallbackPolicy();
			var outer = inner.ThenFallback();

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var result = outer.Handle(() => { }, cts.Token);

				Assert.That(result.IsCanceled, Is.True);
			}
		}

		[Test]
		public void Should_Return_NoDelegateException_When_Delegate_Is_Null()
		{
			FallbackPolicyBase inner = new FallbackPolicy();
			var outer = inner.ThenFallback().WithPolicyName("OuterFb");

			Action nullAction = null;
			var result = outer.Handle(nullAction);

			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.DelegateIsNull));
			Assert.That(result.Errors.First().Message, Is.EqualTo("Delegate for policy OuterFb was not set."));
			Assert.That(result.WrappedPolicyResults, Is.Null);
		}

		// -------------------------------------------------------------------------
		// Chaining
		// -------------------------------------------------------------------------

		[Test]
		public void Should_Support_Double_ThenFallback_Chaining()
		{
			FallbackPolicyBase inner = CreateFailingInnerPolicy();
			var mid = inner.ThenFallback();
			var outer = mid.ThenFallback();

			var result = outer.Handle(() => throw new InvalidOperationException("fail"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));

			var midResult = result.WrappedPolicyResults.First().Result;
			Assert.That(midResult.IsFailed, Is.False);
			Assert.That(midResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(midResult.WrappedPolicyResults.First().Result.IsFailed, Is.True);
		}

		[Test]
		public void Should_Not_Affect_Source_Fallback_Funcs_When_Returned_Policy_Configured()
		{
			FallbackPolicyBase source = new FallbackPolicy();
			var outer = source.ThenFallback().WithFallbackAction(() => { });

			Assert.That(source.HasFallbackAction(), Is.False);
			Assert.That(outer.HasFallbackAction(), Is.True);
		}

		// -------------------------------------------------------------------------
		// All FallbackPolicyBase source types
		// -------------------------------------------------------------------------

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass)]
		[TestCase(FallbackTypeForTests.Creator)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc)]
		[TestCase(FallbackTypeForTests.WithAction)]
		public void Should_Recover_Inner_Failure_For_All_FallbackPolicyBase_Source_Types(FallbackTypeForTests fallbackType)
		{
			FallbackPolicyBase source = null;

			switch (fallbackType)
			{
				case FallbackTypeForTests.BaseClass:
					source = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask).WithFallbackAction(() => { });
					break;
				case FallbackTypeForTests.Creator:
					source = new FallbackPolicy();
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					source = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask);
					break;
				case FallbackTypeForTests.WithAction:
					source = new FallbackPolicy().WithFallbackAction(() => { });
					break;
			}

			Assert.That(source, Is.Not.Null);

			source = source.ExcludeError<InvalidOperationException>();

			var outerFallbackCalled = false;
			var outer = source.ThenFallback().WithFallbackFunc(() =>
			{
				outerFallbackCalled = true;
				return 42;
			});

			var result = outer.Handle<int>(() => throw new InvalidOperationException("fail"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(outerFallbackCalled, Is.True);
			Assert.That(result.Result, Is.EqualTo(42));
			Assert.That(result.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(result.WrappedPolicyResults.First().Result.IsFailed, Is.True);
		}
	}
}
