using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	/// <summary>
	/// <para>Tests for <see cref="FallbackPolicyBase.HandleAsync{TParam}(Func{TParam,CancellationToken,Task}, TParam, bool, CancellationToken)"/>.</para>
	/// <para>
	/// Key behaviours under test:
	///   1. When a parameterized async fallback is registered for TParam, it is called on exception.
	///   2. When no parameterized async fallback is registered, the non-parameterized async fallback is used.
	///   3. When neither is registered, the policy still succeeds (default no-op Task fallback).
	///   4. When the delegate succeeds, no fallback is invoked at all.
	///   5. The param value is correctly forwarded to the parameterized fallback.
	///   6. The CancellationToken is forwarded to the parameterized fallback.
	///   7. Parameterized and non-parameterized entries coexist correctly.
	/// </para>
	/// </summary>
	[TestFixture]
	internal class FallbackPolicyBaseHandleAsyncTParamTests
	{
		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		private static FallbackPolicy PolicyFromProvider(FallbackFuncsProvider provider)
			=> new FallbackPolicy(provider);

		private static Task AlwaysThrowAsync<TParam>(TParam _, CancellationToken __)
			=> throw new InvalidOperationException("forced failure");

		// -------------------------------------------------------------------------
		// Parameterized async fallback is called when registered
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_UseParamAsyncFallback_WhenRegistered_AndDelegateThrows()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((_, __) => { fallbackCalled = true; return Task.CompletedTask; });
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync, "hello", false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(fallbackCalled, Is.True);
		}

		[Test]
		public async Task Should_ForwardParamToParamAsyncFallback_WhenDelegateThrows()
		{
			const string param = "hello";
			string capturedParam = null;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((p, _) => { capturedParam = p; return Task.CompletedTask; });
			var policy = PolicyFromProvider(provider);

			await policy.HandleAsync(AlwaysThrowAsync, param, false, CancellationToken.None);

			Assert.That(capturedParam, Is.EqualTo(param));
		}

		[Test]
		public async Task Should_UseParamAsyncFallback_WithNonCancelableConversion_WhenDelegateThrows()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((_) => { fallbackCalled = true; return Task.CompletedTask; }, CancellationType.Precancelable);
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync, "world", false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(fallbackCalled, Is.True);
		}

		[Test]
		public async Task Should_UseParamAsyncFallback_WithCancelableConversion_WhenDelegateThrows()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((_) => { fallbackCalled = true; return Task.CompletedTask; }, CancellationType.Cancelable);
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync, "world", false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(fallbackCalled, Is.True);
		}

		[Test]
		public async Task Should_UseCorrectParamAsyncFallback_WhenMultipleTParamTypesRegistered()
		{
			var stringFallbackCalled = false;
			var intFallbackCalled    = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((_, __) => { stringFallbackCalled = true; return Task.CompletedTask; });
			provider.AddOrReplaceAsyncFallbackFunc<int>((_, __) => { intFallbackCalled = true; return Task.CompletedTask; });
			var policy = PolicyFromProvider(provider);

			await policy.HandleAsync(AlwaysThrowAsync, "hi", false, CancellationToken.None);
			await policy.HandleAsync(AlwaysThrowAsync, 5, false, CancellationToken.None);

			Assert.That(stringFallbackCalled, Is.True);
			Assert.That(intFallbackCalled, Is.True);
		}

		// -------------------------------------------------------------------------
		// Falls back to non-parameterized async fallback when no parameterized one is set
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_UseNonParamAsyncFallback_WhenParamAsyncFallbackNotRegistered_AndDelegateThrows()
		{
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.FallbackAsync = async (_) => { await Task.Delay(1); nonParamFallbackCalled = true; };
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync, "any", false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
			Assert.That(nonParamFallbackCalled, Is.True);
		}

		[Test]
		public async Task Should_UseNonParamAsyncFallback_WhenParamAsyncFallbackRegisteredForDifferentTParam()
		{
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized for int — NOT for string.
			provider.AddOrReplaceAsyncFallbackFunc<int>((_, __) => Task.CompletedTask);
			// Non-parameterized async fallback.
			provider.FallbackAsync = async (_) => { await Task.Delay(1); nonParamFallbackCalled = true; };
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no string entry, so falls back to non-param.
			var result = await policy.HandleAsync(AlwaysThrowAsync, "test", false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(nonParamFallbackCalled, Is.True);
		}

		[Test]
		public async Task Should_Succeed_WhenNeitherParamNorNonParamAsyncFallbackRegistered_AndDelegateThrows()
		{
			// No fallback registered — default no-op Task fallback is used, policy still succeeds.
			var provider = FallbackFuncsProvider.Create();
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync(AlwaysThrowAsync, "x", false, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsPolicySuccess, Is.True);
		}

		// -------------------------------------------------------------------------
		// No fallback invoked when delegate succeeds
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_NotInvokeParamAsyncFallback_WhenDelegateSucceeds()
		{
			var fallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((_, __) => { fallbackCalled = true; return Task.CompletedTask; });
			var policy = PolicyFromProvider(provider);

			var result = await policy.HandleAsync((_, __) => Task.CompletedTask, "hello", false, CancellationToken.None);

			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(fallbackCalled, Is.False);
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
				provider.AddOrReplaceAsyncFallbackFunc<string>((_, ct) =>
				{
					tokenSeenAsCanceled = ct.IsCancellationRequested;
					return Task.CompletedTask;
				});
				var policy = PolicyFromProvider(provider);

				// Execute without cancelling — token should NOT be cancelled inside fallback.
				await policy.HandleAsync(AlwaysThrowAsync, "x", false, cts.Token);

				Assert.That(tokenSeenAsCanceled, Is.False);
			}
		}

		// -------------------------------------------------------------------------
		// Parameterized async fallback coexists with non-parameterized
		// -------------------------------------------------------------------------

		[Test]
		public async Task Should_UseParamAsyncFallback_NotNonParamAsyncFallback_WhenBothRegistered()
		{
			var paramFallbackCalled    = false;
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			provider.AddOrReplaceAsyncFallbackFunc<string>((_, __) => { paramFallbackCalled = true; return Task.CompletedTask; });
			provider.FallbackAsync = async (_) => { await Task.Delay(1); nonParamFallbackCalled = true; };
			var policy = PolicyFromProvider(provider);

			await policy.HandleAsync(AlwaysThrowAsync, "test", false, CancellationToken.None);

			// Parameterized entry wins over non-parameterized.
			Assert.That(paramFallbackCalled, Is.True);
			Assert.That(nonParamFallbackCalled, Is.False);
		}

		[Test]
		public async Task Should_UseNonParamAsyncFallback_WhenOnlyNonParamRegistered_EvenIfOtherParamTypeExists()
		{
			var nonParamFallbackCalled = false;

			var provider = FallbackFuncsProvider.Create();
			// Parameterized for int only.
			provider.AddOrReplaceAsyncFallbackFunc<int>((_, __) => Task.CompletedTask);
			// Non-parameterized async fallback.
			provider.FallbackAsync = async (_) => { await Task.Delay(1); nonParamFallbackCalled = true; };
			var policy = PolicyFromProvider(provider);

			// Calling with string param — no string entry → falls back to non-param.
			await policy.HandleAsync(AlwaysThrowAsync, "test", false, CancellationToken.None);

			Assert.That(nonParamFallbackCalled, Is.True);
		}
	}
}
