using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	/// <summary>
	/// Tests for the parameterized extension methods added to FallbackPolicyBaseExtensions.
	/// Each test exercises the extension through the public WithFallbackFunc / WithAsyncFallbackFunc
	/// surface on FallbackPolicy (which delegates straight to the extension).
	/// </summary>
	internal class FallbackPolicyBaseExtensionsTests
	{
		// -------------------------------------------------------------------------
		// WithFallbackFunc<TFallback, TParam, T>(Func<TParam, T>, CancellationType)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithFallbackFunc_WithParamFunc_RegisterFunc_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackFunc<string, int>(param => param.Length);

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackFunc<string, int>(), Is.True);
		}

		[Test]
		public void Should_WithFallbackFunc_WithParamFunc_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithFallbackFunc<string, int>(param => param.Length);

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_WithFallbackFunc_WithParamFunc_AndCancellationType_RegisterFunc(CancellationType convertType)
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackFunc<string, int>(param => param.Length, convertType);

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackFunc<string, int>(), Is.True);
		}

		[Test]
		public void Should_WithFallbackFunc_WithParamFunc_ProduceCorrectResult_WhenHandled()
		{
			var policy = new FallbackPolicy()
				.WithFallbackFunc<string, int>(param => param.Length);

			var result = policy.Handle<string, int>(_ => throw new Exception("fail"), "hello");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		[Test]
		public void Should_WithFallbackFunc_WithParamFunc_ReplaceExistingEntry_WhenCalledTwice()
		{
			var policy = new FallbackPolicy();
			policy.WithFallbackFunc<string, int>(_ => 1);
			policy.WithFallbackFunc<string, int>(_ => 99);

			var result = policy.Handle<string, int>(_ => throw new Exception(), "x");

			Assert.That(result.Result, Is.EqualTo(99));
		}

		// -------------------------------------------------------------------------
		// WithFallbackFunc<TFallback, TParam, T>(Func<TParam, CancellationToken, T>)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithFallbackFunc_WithParamCancellableFunc_RegisterFunc_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackFunc<string, int>((param, _) => param.Length);

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackFunc<string, int>(), Is.True);
		}

		[Test]
		public void Should_WithFallbackFunc_WithParamCancellableFunc_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithFallbackFunc<string, int>((param, _) => param.Length);

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		public void Should_WithFallbackFunc_WithParamCancellableFunc_ProduceCorrectResult_WhenHandled()
		{
			var policy = new FallbackPolicy()
				.WithFallbackFunc<string, int>((param, _) => param.Length);

			var result = policy.Handle<string, int>(_ => throw new Exception("fail"), "hello");

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		[Test]
		public void Should_WithFallbackFunc_WithParamCancellableFunc_PassesCancellationToken_WhenHandled()
		{
			using (var cts = new CancellationTokenSource())
			{
				bool tokenWasPassed = false;

				var policy = new FallbackPolicy()
					.WithFallbackFunc<string, int>((param, ct) =>
					{
						tokenWasPassed = ct == cts.Token;
						return param.Length;
					});

				policy.Handle<string, int>(_ => throw new Exception(), "hi", cts.Token);

				Assert.That(tokenWasPassed, Is.True);
			}
		}

		// -------------------------------------------------------------------------
		// WithFallbackAction<TFallback, TParam>(Action<TParam>, CancellationType)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithFallbackAction_WithParamAction_RegisterAction_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackAction<string>(_ => { });

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackAction<string>(), Is.True);
		}

		[Test]
		public void Should_WithFallbackAction_WithParamAction_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithFallbackAction<string>(_ => { });

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_WithFallbackAction_WithParamAction_AndCancellationType_RegisterAction(CancellationType convertType)
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackAction<string>(_ => { }, convertType);

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackAction<string>(), Is.True);
		}

		[Test]
		public void Should_WithFallbackAction_WithParamAction_ExecutesFallback_WhenHandled()
		{
			bool executed = false;
			var policy = new FallbackPolicy()
				.WithFallbackAction<string>(_ => executed = true);

			policy.Handle(_ => throw new Exception(), "hello");

			Assert.That(executed, Is.True);
		}

		[Test]
		public void Should_WithFallbackAction_WithParamAction_PassesParam_WhenHandled()
		{
			string captured = null;
			var policy = new FallbackPolicy()
				.WithFallbackAction<string>(param => captured = param);

			policy.Handle(_ => throw new Exception(), "hello");

			Assert.That(captured, Is.EqualTo("hello"));
		}

		// -------------------------------------------------------------------------
		// WithFallbackAction<TFallback, TParam>(Action<TParam, CancellationToken>)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithFallbackAction_WithParamCancellableAction_RegisterAction_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackAction<string>((_, __) => { });

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackAction<string>(), Is.True);
		}

		[Test]
		public void Should_WithFallbackAction_WithParamCancellableAction_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithFallbackAction<string>((_, __) => { });

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		public void Should_WithFallbackAction_WithParamCancellableAction_ExecutesFallback_WhenHandled()
		{
			bool executed = false;
			var policy = new FallbackPolicy()
				.WithFallbackAction<string>((_, __) => executed = true);

			policy.Handle(_ => throw new Exception(), "hello");

			Assert.That(executed, Is.True);
		}

		[Test]
		public void Should_WithFallbackAction_WithParamCancellableAction_PassesCancellationToken_WhenHandled()
		{
			using (var cts = new CancellationTokenSource())
			{
				bool tokenWasPassed = false;

				var policy = new FallbackPolicy()
					.WithFallbackAction<string>((_, ct) => tokenWasPassed = ct == cts.Token);

				policy.Handle(_ => throw new Exception(), "hi", cts.Token);

				Assert.That(tokenWasPassed, Is.True);
			}
		}

		// -------------------------------------------------------------------------
		// WithAsyncFallbackFunc<TFallback, TParam, T>(Func<TParam, Task<T>>, CancellationType)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamTaskFunc_RegisterFunc_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithAsyncFallbackFunc<string, int>(param => Task.FromResult(param.Length));

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string, int>(), Is.True);
		}

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamTaskFunc_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithAsyncFallbackFunc<string, int>(param => Task.FromResult(param.Length));

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_WithAsyncFallbackFunc_WithParamTaskFunc_AndCancellationType_RegisterFunc(CancellationType convertType)
		{
			var policy = new FallbackPolicy();

			policy.WithAsyncFallbackFunc<string, int>(param => Task.FromResult(param.Length), convertType);

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string, int>(), Is.True);
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamTaskFunc_ProduceCorrectResult_WhenHandled()
		{
			var policy = new FallbackPolicy()
				.WithAsyncFallbackFunc<string, int>(param => Task.FromResult(param.Length));

			var result = await policy.HandleAsync<string, int>(
				(_, __) => throw new Exception("fail"), "hello", CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamTaskFunc_ReplaceExistingEntry_WhenCalledTwice()
		{
			var policy = new FallbackPolicy();
			policy.WithAsyncFallbackFunc<string, int>(_ => Task.FromResult(1));
			policy.WithAsyncFallbackFunc<string, int>(_ => Task.FromResult(99));

			var result = await policy.HandleAsync<string, int>(
				(_, __) => throw new Exception(), "x", CancellationToken.None);

			Assert.That(result.Result, Is.EqualTo(99));
		}

		// -------------------------------------------------------------------------
		// WithAsyncFallbackFunc<TFallback, TParam, T>(Func<TParam, CancellationToken, Task<T>>)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamCancellableTaskFunc_RegisterFunc_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithAsyncFallbackFunc<string, int>((param, _) => Task.FromResult(param.Length));

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string, int>(), Is.True);
		}

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamCancellableTaskFunc_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithAsyncFallbackFunc<string, int>((param, _) => Task.FromResult(param.Length));

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamCancellableTaskFunc_ProduceCorrectResult_WhenHandled()
		{
			var policy = new FallbackPolicy()
				.WithAsyncFallbackFunc<string, int>((param, _) => Task.FromResult(param.Length));

			var result = await policy.HandleAsync<string, int>(
				(_, __) => throw new Exception("fail"), "hello", CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamCancellableTaskFunc_PassesCancellationToken_WhenHandled()
		{
			using (var cts = new CancellationTokenSource())
			{
				bool tokenWasPassed = false;

				var policy = new FallbackPolicy()
					.WithAsyncFallbackFunc<string, int>((param, ct) =>
					{
						tokenWasPassed = ct == cts.Token;
						return Task.FromResult(param.Length);
					});

				await policy.HandleAsync<string, int>(
					(_, __) => throw new Exception(), "hi", cts.Token);

				Assert.That(tokenWasPassed, Is.True);
			}
		}

		// -------------------------------------------------------------------------
		// WithAsyncFallbackFunc<TFallback, TParam>(Func<TParam, Task>, CancellationType)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamVoidTaskFunc_RegisterFunc_WhenCalled()
		{
			var policy = new FallbackPolicy();

			policy.WithAsyncFallbackFunc<string>(_ => Task.CompletedTask);

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string>(), Is.True);
		}

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamVoidTaskFunc_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithAsyncFallbackFunc<string>(_ => Task.CompletedTask);

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		[TestCase(CancellationType.Precancelable)]
		[TestCase(CancellationType.Cancelable)]
		public void Should_WithAsyncFallbackFunc_WithParamVoidTaskFunc_AndCancellationType_RegisterFunc(CancellationType convertType)
		{
			var policy = new FallbackPolicy();

			policy.WithAsyncFallbackFunc<string>(_ => Task.CompletedTask, convertType);

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string>(), Is.True);
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamVoidTaskFunc_ExecutesFallback_WhenHandled()
		{
			bool executed = false;
			var policy = new FallbackPolicy()
				.WithAsyncFallbackFunc<string>(_ => { executed = true; return Task.CompletedTask; });

			await policy.HandleAsync(
				(_, __) => throw new Exception(), "hello", false, CancellationToken.None);

			Assert.That(executed, Is.True);
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamVoidTaskFunc_PassesParam_WhenHandled()
		{
			string captured = null;
			var policy = new FallbackPolicy()
				.WithAsyncFallbackFunc<string>(param => { captured = param; return Task.CompletedTask; });

			await policy.HandleAsync(
				(_, __) => throw new Exception(), "hello", false, CancellationToken.None);

			Assert.That(captured, Is.EqualTo("hello"));
		}

		// -------------------------------------------------------------------------
		// WithAsyncFallbackFunc<TFallback, TParam>(Func<TParam, CancellationToken, Task>)
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamCancellableVoidTaskFunc_RegisterFunc_WhenCalled()
		{
			var policy = new FallbackPolicy();

			_ = policy.WithAsyncFallbackFunc<string>((__, _) => Task.CompletedTask);

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string>(), Is.True);
		}

		[Test]
		public void Should_WithAsyncFallbackFunc_WithParamCancellableVoidTaskFunc_ReturnSameInstance_ForChaining()
		{
			var policy = new FallbackPolicy();

			var returned = policy.WithAsyncFallbackFunc<string>((__, _) => Task.CompletedTask);

			Assert.That(returned, Is.SameAs(policy));
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamCancellableVoidTaskFunc_ExecutesFallback_WhenHandled()
		{
			bool executed = false;
			var policy = new FallbackPolicy()
				.WithAsyncFallbackFunc<string>((__, _) => { executed = true; return Task.CompletedTask; });

			await policy.HandleAsync(
				(_, __) => throw new Exception(), "hello", false, CancellationToken.None);

			Assert.That(executed, Is.True);
		}

		[Test]
		public async Task Should_WithAsyncFallbackFunc_WithParamCancellableVoidTaskFunc_PassesCancellationToken_WhenHandled()
		{
			using (var cts = new CancellationTokenSource())
			{
				bool tokenWasPassed = false;

				var policy = new FallbackPolicy()
					.WithAsyncFallbackFunc<string>((_, ct) =>
					{
						tokenWasPassed = ct == cts.Token;
						return Task.CompletedTask;
					});

				_ = await policy.HandleAsync<string>(
					(_, __) => throw new Exception(), "hi", false, cts.Token);

				Assert.That(tokenWasPassed, Is.True);
			}
		}

		// -------------------------------------------------------------------------
		// Cross-type isolation: parameterized and non-parameterized entries coexist
		// -------------------------------------------------------------------------

		[Test]
		public void Should_WithFallbackFunc_WithParam_NotAffect_NonParamEntry_ForSameT()
		{
			var policy = new FallbackPolicy()
				.WithFallbackFunc<int>(_ => 42);

			policy.WithFallbackFunc<string, int>(param => param.Length);

			// Non-parameterized entry must still be present and return its own value.
			Assert.That(policy._fallbackFuncsProvider.HasFallbackFunc<int>(), Is.True);
			var nonParamResult = policy._fallbackFuncsProvider.GetFallbackFunc<int>()(CancellationToken.None);
			Assert.That(nonParamResult, Is.EqualTo(42));
		}

		[Test]
		public void Should_WithFallbackFunc_DifferentTParam_SameT_RegisterSeparateEntries()
		{
			var policy = new FallbackPolicy();

			policy.WithFallbackFunc<string, int>(param => param.Length);
			policy.WithFallbackFunc<int, int>(param => param * 2);

			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackFunc<string, int>(), Is.True);
			Assert.That(policy._fallbackFuncsProvider.HasParamFallbackFunc<int, int>(), Is.True);
		}

		[Test]
		public void Should_WithAsyncFallbackFunc_DifferentTParam_SameT_RegisterSeparateEntries()
		{
			var policy = new FallbackPolicy();

			policy.WithAsyncFallbackFunc<string, int>(param => Task.FromResult(param.Length));
			policy.WithAsyncFallbackFunc<int, int>(param => Task.FromResult(param * 2));

			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<string, int>(), Is.True);
			Assert.That(policy._fallbackFuncsProvider.HasAsyncParamFallbackFunc<int, int>(), Is.True);
		}
	}
}
