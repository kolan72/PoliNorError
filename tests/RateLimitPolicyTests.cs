using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class RateLimitPolicyTests
	{
		[Test]
		public void Should_Execute_Delegate_And_Consume_One_Permit_When_Permitted()
		{
			int acquired = 0;
			var limiter = new DelegateRateLimiter(() =>
			{
				acquired++;
				return RateLimitResult.Permitted;
			});
			var policy = new RateLimitPolicy(limiter);

			bool ran = false;
			var result = policy.Handle(() => ran = true);

			Assert.That(ran, Is.True);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.IsFailed, Is.False);
			Assert.That(acquired, Is.EqualTo(1));
		}

		[Test]
		public void Should_Return_FailedRateLimitedResult_When_Denied()
		{
			var retryAfter = TimeSpan.FromMilliseconds(250);
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Denied(retryAfter));
			var policy = new RateLimitPolicy(limiter);

			bool handlerRan = false;
			policy.AddPolicyResultHandler<bool>(_ => handlerRan = true);

			bool ran = false;
			var result = policy.Handle(() => ran = true);

			Assert.That(ran, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.RateLimited));
			Assert.That(result.IsRateLimited(), Is.True);
			Assert.That(result.GetRetryAfter(), Is.EqualTo(retryAfter));
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.NoError, Is.False);
			Assert.That(result.PolicyName, Is.EqualTo("RateLimitPolicy"));
			Assert.That(handlerRan, Is.True);

			Assert.That(result.LastError, Is.TypeOf<RateLimitException>());
			Assert.That(result.UnprocessedError, Is.TypeOf<RateLimitException>());
			Assert.That(((RateLimitException)result.LastError).RetryAfter, Is.EqualTo(retryAfter));
			Assert.That(result.Errors.OfType<RateLimitException>().Any(), Is.True);
		}

		[Test]
		public void Should_Return_DefaultResult_When_GenericHandleIsDenied()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Denied(TimeSpan.FromSeconds(1)));
			var policy = new RateLimitPolicy(limiter);

			bool ran = false;
			var result = policy.Handle(() =>
			{
				ran = true;
				return 42;
			});

			Assert.That(ran, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsRateLimited(), Is.True);
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		[Test]
		public void Should_Execute_GenericDelegate_When_Permitted()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Permitted);
			var policy = new RateLimitPolicy(limiter);

			var result = policy.Handle(() => 42);

			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(42));
		}

		[Test]
		public void Should_Return_DelegateIsNull_And_NoPermitConsumed_When_DelegateIsNull()
		{
			int acquired = 0;
			var limiter = new DelegateRateLimiter(() =>
			{
				acquired++;
				return RateLimitResult.Permitted;
			});
			var policy = new RateLimitPolicy(limiter);

			var result = policy.Handle((Action)null);

			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.DelegateIsNull));
			Assert.That(acquired, Is.Zero);
		}

		[Test]
		public void Should_Return_CanceledEarly_And_NoPermitConsumed_When_TokenPreCanceled()
		{
			int acquired = 0;
			var limiter = new DelegateRateLimiter(() =>
			{
				acquired++;
				return RateLimitResult.Permitted;
			});
			var policy = new RateLimitPolicy(limiter);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				bool ran = false;
				var result = policy.Handle(() => ran = true, cts.Token);

				Assert.That(ran, Is.False);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.NoError, Is.True);
				Assert.That(result.IsRateLimited(), Is.False);
				Assert.That(acquired, Is.Zero);
			}
		}

		[Test]
		public void Should_Return_CanceledResult_When_LimiterThrowsOCE_WithCanceledToken()
		{
			using (var cts = new CancellationTokenSource())
			{
				Func<CancellationToken, RateLimitResult> acquire = token =>
				{
					cts.Cancel();
					throw new OperationCanceledException(token);
				};
				var policy = new RateLimitPolicy(new DelegateRateLimiter(acquire));

				bool ran = false;
				var result = policy.Handle(() => ran = true, cts.Token);

				Assert.That(ran, Is.False);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.IsRateLimited(), Is.False);
			}
		}

		[Test]
		public void Should_Return_PolicyProcessorFailed_When_LimiterThrowsOtherException()
		{
			var limiter = new DelegateRateLimiter(
				(Func<CancellationToken, RateLimitResult>)(token => throw new InvalidOperationException("limiter failure")));
			var policy = new RateLimitPolicy(limiter);

			bool ran = false;
			var result = policy.Handle(() => ran = true);

			Assert.That(ran, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
			Assert.That(result.IsRateLimited(), Is.False);
			Assert.That(result.LastError, Is.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void Should_Return_PolicyProcessorFailed_When_LimiterThrowsOCE_WithUnrelatedToken()
		{
			using (var otherCts = new CancellationTokenSource())
			using (var cts = new CancellationTokenSource())
			{
				Func<CancellationToken, RateLimitResult> acquire =
					token => throw new OperationCanceledException(otherCts.Token);
				var policy = new RateLimitPolicy(new DelegateRateLimiter(acquire));

				var result = policy.Handle(() => { }, cts.Token);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
				Assert.That(result.IsCanceled, Is.False);
				Assert.That(result.IsRateLimited(), Is.False);
			}
		}

		[Test]
		public async Task Should_Execute_AsyncDelegate_When_Permitted()
		{
			int acquired = 0;
			var limiter = new DelegateRateLimiter((Func<CancellationToken, Task<RateLimitResult>>)(token =>
			{
				acquired++;
				return Task.FromResult(RateLimitResult.Permitted);
			}));
			var policy = new RateLimitPolicy(limiter);

			bool ran = false;
			var result = await policy.HandleAsync(ct =>
			{
				ran = true;
				return Task.CompletedTask;
			});

			Assert.That(ran, Is.True);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(acquired, Is.EqualTo(1));
		}

		[Test]
		public async Task Should_Return_FailedRateLimitedResult_Async_When_Denied()
		{
			var retryAfter = TimeSpan.FromMilliseconds(120);
			var limiter = new DelegateRateLimiter(
				(Func<CancellationToken, Task<RateLimitResult>>)(token => Task.FromResult(RateLimitResult.Denied(retryAfter))));
			var policy = new RateLimitPolicy(limiter);

			bool handlerRan = false;
			policy.AddPolicyResultHandler(_ => handlerRan = true);

			bool ran = false;
			var result = await policy.HandleAsync(ct =>
			{
				ran = true;
				return Task.CompletedTask;
			});

			Assert.That(ran, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.RateLimited));
			Assert.That(result.IsRateLimited(), Is.True);
			Assert.That(result.GetRetryAfter(), Is.EqualTo(retryAfter));
			Assert.That(result.UnprocessedError, Is.TypeOf<RateLimitException>());
			Assert.That(result.NoError, Is.False);
			Assert.That(handlerRan, Is.True);
		}

		[Test]
		public async Task Should_Return_DefaultResult_Async_When_GenericHandleIsDenied()
		{
			var limiter = new DelegateRateLimiter(
				(Func<CancellationToken, Task<RateLimitResult>>)(token => Task.FromResult(RateLimitResult.Denied())));
			var policy = new RateLimitPolicy(limiter);

			bool ran = false;
			var result = await policy.HandleAsync(ct =>
			{
				ran = true;
				return Task.FromResult(42);
			});

			Assert.That(ran, Is.False);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsRateLimited(), Is.True);
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		[Test]
		public async Task Should_Return_DelegateIsNull_Async_And_NoPermitConsumed_When_DelegateIsNull()
		{
			int acquired = 0;
			var limiter = new DelegateRateLimiter(() =>
			{
				acquired++;
				return RateLimitResult.Permitted;
			});
			var policy = new RateLimitPolicy(limiter);

			var result = await policy.HandleAsync((Func<CancellationToken, Task>)null);

			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.DelegateIsNull));
			Assert.That(acquired, Is.Zero);
		}

		[Test]
		public async Task Should_Return_CanceledEarly_Async_And_NoPermitConsumed_When_TokenPreCanceled()
		{
			int acquired = 0;
			var limiter = new DelegateRateLimiter(() =>
			{
				acquired++;
				return RateLimitResult.Permitted;
			});
			var policy = new RateLimitPolicy(limiter);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				bool ran = false;
				var result = await policy.HandleAsync(ct =>
				{
					ran = true;
					return Task.CompletedTask;
				}, token: cts.Token);

				Assert.That(ran, Is.False);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.NoError, Is.True);
				Assert.That(acquired, Is.Zero);
			}
		}

		[Test]
		public void Should_ThrowArgumentNullException_When_LimiterIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new RateLimitPolicy(null));
		}

		[Test]
		public void Should_ExposeHelpers_OnNonRateLimitedResult()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Permitted);
			var policy = new RateLimitPolicy(limiter);

			var result = policy.Handle(() => { });

			Assert.That(result.IsRateLimited(), Is.False);
			Assert.That(result.GetRetryAfter(), Is.Null);
		}
	}
}
