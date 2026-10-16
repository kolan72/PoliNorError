using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	/// <summary>
	/// <para>Tests for <see cref="FallbackPolicyBase.Handle{TParam}(Action{TParam}, TParam, CancellationToken)"/>.</para>
	/// <para>
	/// Key behaviours under test:
	///   1. When a parameterized fallback action is registered for TParam, it is called on exception.
	///   2. When no parameterized fallback action is registered, the non-parameterized fallback action is used.
	///   3. When neither is registered, the policy still succeeds (default no-op fallback).
	///   4. When the delegate succeeds, no fallback is invoked at all.
	///   5. The param value is correctly forwarded to the parameterized fallback action.
	///   6. The CancellationToken is forwarded to the parameterized fallback action.
	///   7. Parameterized and non-parameterized entries coexist correctly.
	/// </para>
	/// </summary>
	[TestFixture]
	internal class FallbackPolicyBaseHandleTParamTests
	{
		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		private static FallbackPolicy PolicyFromProvider(FallbackFuncsProvider provider)
			=> new FallbackPolicy(provider);

		private static void AlwaysThrow<TParam>(TParam _)
			=> throw new InvalidOperationException("forced failure");

		// -------------------------------------------------------------------------
		// Parameterized fallback action is called when registered
		// -------------------------------------------------------------------------

		[Test]
		public void Should_UseParamFallbackAction_WhenRegistered_AndDelegateThrows()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((_, __) => fallbackCalled = true);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow, "hello");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(fallbackCalled, Is.True);
		}

		[Test]
		public void Should_ForwardParamToParamFallbackAction_WhenDelegateThrows()
		{
			// The fallback captures the param — proves it is forwarded correctly.
			const string param = "hello";
			string capturedParam = null;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((p, _) => capturedParam = p);
			var policy = PolicyFromProvider(provider);

			policy.Handle(AlwaysThrow, param);

			Assert.That(capturedParam, Is.EqualTo(param));
		}

		[Test]
		public void Should_UseParamFallbackAction_WithNonCancelableConversion_WhenDelegateThrows()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((_) => fallbackCalled = true, CancellationType.Precancelable);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow, "world");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(fallbackCalled, Is.True);
		}

		[Test]
		public void Should_UseParamFallbackAction_WithCancelableConversion_WhenDelegateThrows()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((_) => fallbackCalled = true, CancellationType.Cancelable);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow, "world");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(fallbackCalled, Is.True);
		}

		[Test]
		public void Should_UseCorrectParamFallbackAction_WhenMultipleTParamTypesRegistered()
		{
			var stringFallbackCalled = false;
			var intFallbackCalled    = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((_, __) => stringFallbackCalled = true);
			provider.AddOrReplaceFallbackAction<int>((_, __) => intFallbackCalled = true);
			var policy = PolicyFromProvider(provider);

			policy.Handle(AlwaysThrow, "hi");
			policy.Handle(AlwaysThrow, 5);

			Assert.That(stringFallbackCalled, Is.True);
			Assert.That(intFallbackCalled, Is.True);
		}

		// -------------------------------------------------------------------------
		// Falls back to non-parameterized fallback action when no parameterized one is set
		// -------------------------------------------------------------------------

		[Test]
		public void Should_UseNonParamFallbackAction_WhenParamFallbackActionNotRegistered_AndDelegateThrows()
		{
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.Fallback = (_) => nonParamFallbackCalled = true;
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow, "any");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(nonParamFallbackCalled, Is.True);
		}

		[Test]
		public void Should_UseNonParamFallbackAction_WhenParamFallbackActionRegisteredForDifferentTParam()
		{
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized for int — NOT for string.
			provider.AddOrReplaceFallbackAction<int>((_, __) => { });
			// Non-parameterized fallback action.
			provider.Fallback = (_) => nonParamFallbackCalled = true;
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no string entry, so falls back to non-param.
			var result = policy.Handle(AlwaysThrow, "test");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(nonParamFallbackCalled, Is.True);
		}

		[Test]
		public void Should_Succeed_WhenNeitherParamNorNonParamFallbackActionRegistered_AndDelegateThrows()
		{
			// No fallback registered — default no-op fallback is used, policy still succeeds.
			var provider = FallbackFuncsProvider.Create();
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow, "x");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
		}

		// -------------------------------------------------------------------------
		// No fallback invoked when delegate succeeds
		// -------------------------------------------------------------------------

		[Test]
		public void Should_NotInvokeParamFallbackAction_WhenDelegateSucceeds()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((_, __) => fallbackCalled = true);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle((_) => { }, "hello");

			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(fallbackCalled, Is.False);
		}

		// -------------------------------------------------------------------------
		// CancellationToken is forwarded to the parameterized fallback action
		// -------------------------------------------------------------------------

		[Test]
		public void Should_PassCancellationToken_ToParamFallbackAction()
		{
			using (var cts = new CancellationTokenSource())
			{
				var tokenSeenAsCanceled = false;

				var provider = FallbackFuncsProvider.Create();
				provider.AddOrReplaceFallbackAction<string>((_, ct) => tokenSeenAsCanceled = ct.IsCancellationRequested);
				var policy = PolicyFromProvider(provider);

				// Execute without cancelling — token should NOT be cancelled inside fallback.
				policy.Handle(AlwaysThrow, "x", cts.Token);

				Assert.That(tokenSeenAsCanceled, Is.False);
			}
		}

		// -------------------------------------------------------------------------
		// Parameterized fallback action coexists with non-parameterized
		// -------------------------------------------------------------------------

		[Test]
		public void Should_UseParamFallbackAction_NotNonParamFallbackAction_WhenBothRegistered()
		{
			var paramFallbackCalled    = false;
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackAction<string>((_, __) => paramFallbackCalled = true);
			provider.Fallback = (_) => nonParamFallbackCalled = true;
			var policy = PolicyFromProvider(provider);

			policy.Handle(AlwaysThrow, "test");

			// Parameterized entry wins over non-parameterized.
			Assert.That(paramFallbackCalled, Is.True);
			Assert.That(nonParamFallbackCalled, Is.False);
		}

		[Test]
		public void Should_UseNonParamFallbackAction_WhenOnlyNonParamRegistered_EvenIfOtherParamTypeExists()
		{
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized for int only.
			provider.AddOrReplaceFallbackAction<int>((_, __) => { });
			// Non-parameterized fallback action.
			provider.Fallback = (_) => nonParamFallbackCalled = true;
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no string entry → falls back to non-param.
			policy.Handle(AlwaysThrow, "test");

			Assert.That(nonParamFallbackCalled, Is.True);
		}
	}
}
