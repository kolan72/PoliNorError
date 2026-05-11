using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	/// <summary>
	/// <para>Tests for <see cref="FallbackPolicyBase.HandleAsync{TParam,T}(Func{TParam,CancellationToken,Task{T}}, TParam, CancellationToken)"/>.</para>
	/// <para>
	/// Key behaviours under test:
	///   1. When a parameterized async fallback is registered for (TParam, T), it is called on exception.
	///   2. When no parameterized async fallback is registered, the non-parameterized async fallback for T is used.
	///   3. When neither is registered, the result is the default value of T.
	///   4. When the delegate succeeds, no fallback is invoked at all.
	///   5. The param value is correctly forwarded to the parameterized fallback.
	///   6. Cancellation token is forwarded to the fallback.
	///   7. Parameterized and non-parameterized entries coexist correctly.
	/// </para>
	/// </summary>
	[TestFixture]
	internal class FallbackPolicyBaseHandleAsyncTParamTTests
	{
		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		private static FallbackPolicy PolicyFromProvider(FallbackFuncsProvider provider)
			=> new FallbackPolicy(provider);

		private static Task<T> AlwaysThrowAsync<TParam, T>(TParam _, CancellationToken __)
			=> throw new InvalidOperationException("forced failure");

		// -------------------------------------------------------------------------
		// Parameterized async fallback is called when registered
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_UseParamAsyncFallback_WhenRegistered_AndDelegateThrows()
		{
			const string param = "hello";
			const int expectedFallback = 99;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((_, __) => Task.FromResult(expectedFallback));
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, param, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(expectedFallback));
		}

		[Test]
		public async Task Should_ForwardParamToParamAsyncFallback_WhenDelegateThrows()
		{
			// The fallback returns the length of the string param — proves param is forwarded.
			const string param = "hello";

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((p, _) => Task.FromResult(p.Length));
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, param, CancellationToken.None);

			Assert.That(result.Result, Is.EqualTo(param.Length));
		}

		[Test]
		public async Task Should_UseParamAsyncFallback_WithNonCancelableConversion_WhenDelegateThrows()
		{
			const string param = "world";
			const int expectedFallback = 42;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((_) => Task.FromResult(expectedFallback), CancellationType.Precancelable);
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, param, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(expectedFallback));
		}

		[Test]
		public async Task Should_UseParamAsyncFallback_WithCancelableConversion_WhenDelegateThrows()
		{
			const string param = "world";
			const int expectedFallback = 7;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((_) => Task.FromResult(expectedFallback), CancellationType.Cancelable);
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, param, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(expectedFallback));
		}

		[Test]
		public async Task Should_UseCorrectParamAsyncFallback_WhenMultipleParamTypesRegistered()
		{
			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((p, _) => Task.FromResult(p.Length));
			provider.AddOrReplaceAsyncFallbackFunc<int, int>((p, _) => Task.FromResult(p * 10));
			var policy = PolicyFromProvider(provider);

			var resultFromString = await policy.HandleAsync(AlwaysThrowAsync<string, int>, "hi", CancellationToken.None);
			var resultFromInt    = await policy.HandleAsync(AlwaysThrowAsync<int, int>, 5, CancellationToken.None);

			Assert.That(resultFromString.Result, Is.EqualTo(2));   // "hi".Length
			Assert.That(resultFromInt.Result,    Is.EqualTo(50));  // 5 * 10
		}

		// -------------------------------------------------------------------------
		// Falls back to non-parameterized async fallback when no parameterized one is set
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_UseNonParamAsyncFallback_WhenParamAsyncFallbackNotRegistered_AndDelegateThrows()
		{
			const int nonParamFallbackValue = 55;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc(async (_) => { await Task.Delay(1); return nonParamFallbackValue; });
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, "any", CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(nonParamFallbackValue));
		}

		[Test]
		public async Task Should_UseNonParamAsyncFallback_WhenParamAsyncFallbackRegisteredForDifferentTParam()
		{
			const int nonParamFallbackValue = 77;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized fallback for (int, int) — NOT for (string, int).
			provider.AddOrReplaceAsyncFallbackFunc<int, int>((p, _) => Task.FromResult(p * 2));
			// Non-parameterized async fallback for int.
			provider.AddOrReplaceAsyncFallbackFunc(async (_) => { await Task.Delay(1); return nonParamFallbackValue; });
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no (string, int) entry, so falls back to non-param.
			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, "test", CancellationToken.None);

			Assert.That(result.Result, Is.EqualTo(nonParamFallbackValue));
		}

		[Test]
		public async Task Should_ReturnDefaultT_WhenNeitherParamNorNonParamAsyncFallbackRegistered_AndDelegateThrows()
		{
			var provider = FallbackFuncsProvider.Create();
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, "x", CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		// -------------------------------------------------------------------------
		// No fallback invoked when delegate succeeds
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_NotInvokeAsyncFallback_WhenDelegateSucceeds()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((_, __) => { fallbackCalled = true; return Task.FromResult(-1); });
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync((p, _) => Task.FromResult(p.Length), "hello", CancellationToken.None);

			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.Result, Is.EqualTo(5));
			Assert.That(fallbackCalled, Is.False);
		}

		[Test]
		public async Task Should_ReturnDelegateResult_WhenDelegateSucceeds_AndParamAsyncFallbackRegistered()
		{
			const int delegateResult = 123;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((_, __) => Task.FromResult(999));
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync((_, __) => Task.FromResult(delegateResult), "ignored", CancellationToken.None);

			Assert.That(result.Result, Is.EqualTo(delegateResult));
		}

		// -------------------------------------------------------------------------
		// CancellationToken is forwarded to the parameterized async fallback
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_PassCancellationToken_ToParamAsyncFallback()
		{
			using (var cts = new CancellationTokenSource())
			{
				var tokenSeenAsCanceled = false;

				var provider = FallbackFuncsProvider.Create();
				provider.AddOrReplaceAsyncFallbackFunc<string, int>((_, ct) =>
				{
					tokenSeenAsCanceled = ct.IsCancellationRequested;
					return Task.FromResult(0);
				});
				var policy = PolicyFromProvider(provider);

				await policy.HandleAsync(AlwaysThrowAsync<string, int>, "x", cts.Token);

				Assert.That(tokenSeenAsCanceled, Is.False);
			}
		}

		// -------------------------------------------------------------------------
		// Parameterized async fallback coexists with non-parameterized for same T
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_UseParamAsyncFallback_NotNonParamAsyncFallback_WhenBothRegisteredForSameT()
		{
			const int paramFallbackValue    = 11;
			const int nonParamFallbackValue = 22;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string, int>((_, __) => Task.FromResult(paramFallbackValue));
			provider.AddOrReplaceAsyncFallbackFunc(async (_) => { await Task.Delay(1); return nonParamFallbackValue; });
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, "test", CancellationToken.None);

			// Parameterized entry wins over non-parameterized.
			Assert.That(result.Result, Is.EqualTo(paramFallbackValue));
		}

		[Test]
		public async Task Should_UseNonParamAsyncFallback_WhenOnlyNonParamRegistered_EvenIfOtherParamTypeExists()
		{
			const int nonParamFallbackValue = 33;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized for (int, int) only.
			provider.AddOrReplaceAsyncFallbackFunc<int, int>((p, _) => Task.FromResult(p));
			// Non-parameterized async fallback for int.
			provider.AddOrReplaceAsyncFallbackFunc<int>(async (_) => { await Task.Delay(1); return nonParamFallbackValue; });
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no (string, int) entry → falls back to non-param.
			var result = await policy.HandleAsync(AlwaysThrowAsync<string, int>, "test", CancellationToken.None);

			Assert.That(result.Result, Is.EqualTo(nonParamFallbackValue));
		}
	}
}
