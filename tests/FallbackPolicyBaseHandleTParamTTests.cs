using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	/// <summary>
	/// <para>Tests for <see cref="FallbackPolicyBase.Handle{TParam,T}(Func{TParam,T}, TParam, CancellationToken)"/>.</para>
	/// <para>
	/// Key behaviours under test:
	///   1. When a parameterized fallback is registered for (TParam, T), it is called on exception.
	///   2. When no parameterized fallback is registered, the non-parameterized fallback for T is used.
	///   3. When neither is registered, the result is the default value of T.
	///   4. When the delegate succeeds, no fallback is invoked at all.
	///   5. The param value is correctly forwarded to the parameterized fallback.
	///   6. Cancellation is respected.
	/// </para>
	/// </summary>
	[TestFixture]
	internal class FallbackPolicyBaseHandleTParamTTests
	{
		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		/// <summary>
		/// Returns a <see cref="FallbackPolicy"/> built from a <see cref="FallbackFuncsProvider"/>
		/// so we can register parameterized fallbacks via <c>AddOrReplaceFallbackFunc&lt;TParam,T&gt;</c>.
		/// </summary>
		private static FallbackPolicy PolicyFromProvider(FallbackFuncsProvider provider)
			=> new FallbackPolicy(provider);

		/// <summary>A delegate that always throws <see cref="InvalidOperationException"/>.</summary>
		private static T AlwaysThrow<TParam, T>(TParam _)
			=> throw new InvalidOperationException("forced failure");

		// -------------------------------------------------------------------------
		// Parameterized fallback is called when registered
		// -------------------------------------------------------------------------

		[Test]
		public void Should_UseParamFallback_WhenRegistered_AndDelegateThrows()
		{
			const string param = "hello";
			const int expectedFallback = 99;

			var provider = FallbackFuncsProvider.Create();
			_ = provider.AddOrReplaceFallbackFunc<string, int>((_, __) => expectedFallback);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, param);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(expectedFallback));
		}

		[Test]
		public void Should_ForwardParamToParamFallback_WhenDelegateThrows()
		{
			// The fallback returns the length of the string param — proves param is forwarded.
			const string param = "hello";

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<string, int>((p, _) => p.Length);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, param);

			Assert.That(result.Result, Is.EqualTo(param.Length));
		}

		[Test]
		public void Should_UseParamFallback_WithNonCancelableConversion_WhenDelegateThrows()
		{
			const string param = "world";
			const int expectedFallback = 42;

			var provider = FallbackFuncsProvider.Create();
			// Register via the Func<TParam, T> overload (non-cancelable conversion).
			provider.AddOrReplaceFallbackFunc<string, int>((_) => expectedFallback, CancellationType.Precancelable);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, param);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(expectedFallback));
		}

		[Test]
		public void Should_UseParamFallback_WithCancelableConversion_WhenDelegateThrows()
		{
			const string param = "world";
			const int expectedFallback = 7;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<string, int>((_) => expectedFallback, CancellationType.Cancelable);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, param);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(expectedFallback));
		}

		[Test]
		public void Should_UseCorrectParamFallback_WhenMultipleParamTypesRegistered()
		{
			// Two parameterized fallbacks for the same T but different TParam.
			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<string, int>((p, _) => p.Length);
			provider.AddOrReplaceFallbackFunc<int, int>((p, _) => p * 10);
			var policy = PolicyFromProvider(provider);

			var resultFromString = policy.Handle(AlwaysThrow<string, int>, "hi");
			var resultFromInt    = policy.Handle<int, int>(AlwaysThrow<int, int>, 5);

			Assert.That(resultFromString.Result, Is.EqualTo(2));   // "hi".Length
			Assert.That(resultFromInt.Result,    Is.EqualTo(50));  // 5 * 10
		}

		// -------------------------------------------------------------------------
		// Falls back to non-parameterized fallback when no parameterized one is set
		// -------------------------------------------------------------------------

		[Test]
		public void Should_UseNonParamFallback_WhenParamFallbackNotRegistered_AndDelegateThrows()
		{
			const int nonParamFallbackValue = 55;

			// Only a non-parameterized fallback for int is registered.
			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<int>(_ => nonParamFallbackValue);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, "any");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(nonParamFallbackValue));
		}

		[Test]
		public void Should_UseNonParamFallback_WhenParamFallbackRegisteredForDifferentTParam()
		{
			const int nonParamFallbackValue = 77;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized fallback for (int, int) — NOT for (string, int).
			provider.AddOrReplaceFallbackFunc<int, int>((p, _) => p * 2);
			// Non-parameterized fallback for int.
			provider.AddOrReplaceFallbackFunc<int>(_ => nonParamFallbackValue);
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no (string, int) entry, so falls back to non-param.
			var result = policy.Handle(AlwaysThrow<string, int>, "test");

			Assert.That(result.Result, Is.EqualTo(nonParamFallbackValue));
		}

		[Test]
		public void Should_ReturnDefaultT_WhenNeitherParamNorNonParamFallbackRegistered_AndDelegateThrows()
		{
			// No fallback registered at all — GetFallbackFunc returns default(T).
			var provider = FallbackFuncsProvider.Create();
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, "x");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		// -------------------------------------------------------------------------
		// No fallback invoked when delegate succeeds
		// -------------------------------------------------------------------------

		[Test]
		public void Should_NotInvokeFallback_WhenDelegateSucceeds()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<string, int>((_, __) => { fallbackCalled = true; return -1; });
			var policy = PolicyFromProvider(provider);

			// Delegate succeeds — returns the string length.
			var result = policy.Handle((p) => p.Length, "hello");

			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.Result, Is.EqualTo(5));
			Assert.That(fallbackCalled, Is.False);
		}

		[Test]
		public void Should_ReturnDelegateResult_WhenDelegateSucceeds_AndParamFallbackRegistered()
		{
			const int delegateResult = 123;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<string, int>((_, __) => 999);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle((_) => delegateResult, "ignored");

			Assert.That(result.Result, Is.EqualTo(delegateResult));
		}

		// -------------------------------------------------------------------------
		// Parameterized fallback receives the CancellationToken
		// -------------------------------------------------------------------------

		[Test]
		public void Should_PassCancellationToken_ToParamFallback()
		{
			using (var cts = new CancellationTokenSource())
			{
				var tokenSeenByCanceled = false;

				var provider = FallbackFuncsProvider.Create();
				provider.AddOrReplaceFallbackFunc<string, int>((_, ct) =>
				{
					tokenSeenByCanceled = ct.IsCancellationRequested;
					return 0;
				});
				var policy = PolicyFromProvider(provider);

				// Execute without cancelling — token should NOT be cancelled inside fallback.
				policy.Handle(AlwaysThrow<string, int>, "x", cts.Token);

				Assert.That(tokenSeenByCanceled, Is.False);
			}
		}

		// -------------------------------------------------------------------------
		// Parameterized fallback coexists with non-parameterized for same T
		// -------------------------------------------------------------------------

		[Test]
		public void Should_UseParamFallback_NotNonParamFallback_WhenBothRegisteredForSameT()
		{
			const int paramFallbackValue    = 11;
			const int nonParamFallbackValue = 22;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceFallbackFunc<string, int>((_, __) => paramFallbackValue);
			provider.AddOrReplaceFallbackFunc<int>(_ => nonParamFallbackValue);
			var policy = PolicyFromProvider(provider);

			var result = policy.Handle(AlwaysThrow<string, int>, "test");

			// Parameterized entry wins over non-parameterized.
			Assert.That(result.Result, Is.EqualTo(paramFallbackValue));
		}

		[Test]
		public void Should_UseNonParamFallback_WhenOnlyNonParamRegistered_EvenIfOtherParamTypeExists()
		{
			const int nonParamFallbackValue = 33;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized for (int, int) only.
			provider.AddOrReplaceFallbackFunc<int, int>((p, _) => p);
			// Non-parameterized for int.
			provider.AddOrReplaceFallbackFunc<int>(_ => nonParamFallbackValue);
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no (string, int) entry → falls back to non-param.
			var result = policy.Handle(AlwaysThrow<string, int>, "test");

			Assert.That(result.Result, Is.EqualTo(nonParamFallbackValue));
		}
	}
}
