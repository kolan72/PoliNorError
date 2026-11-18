using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class DelegateRateLimiterTests
	{
		[Test]
		public void Should_TryAcquire_InvokeSyncDelegate_WithToken()
		{
			CancellationToken receivedToken = default;
			using (var cts = new CancellationTokenSource())
			{
				var limiter = new DelegateRateLimiter(token =>
				{
					receivedToken = token;
					return RateLimitResult.Permitted;
				});

				var result = limiter.TryAcquire(cts.Token);

				Assert.That(result.IsPermitted, Is.True);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public async Task Should_TryAcquireAsync_BridgeFromSyncDelegate()
		{
			int calls = 0;
			var limiter = new DelegateRateLimiter(() =>
			{
				calls++;
				return RateLimitResult.Denied(TimeSpan.FromSeconds(1));
			});

			var result = await limiter.TryAcquireAsync();

			Assert.That(calls, Is.EqualTo(1));
			Assert.That(result.IsPermitted, Is.False);
			Assert.That(result.RetryAfter, Is.EqualTo(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public async Task Should_TryAcquireAsync_InvokeAsyncDelegate_WithToken()
		{
			CancellationToken receivedToken = default;
			using (var cts = new CancellationTokenSource())
			{
				var limiter = new DelegateRateLimiter(token =>
				{
					receivedToken = token;
					return Task.FromResult(RateLimitResult.Permitted);
				});

				var result = await limiter.TryAcquireAsync(cts.Token);

				Assert.That(result.IsPermitted, Is.True);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_TryAcquire_ThrowNotSupported_When_OnlyAsyncDelegateProvided()
		{
			var limiter = new DelegateRateLimiter((Func<CancellationToken, Task<RateLimitResult>>)
				(token => Task.FromResult(RateLimitResult.Permitted)));

			Assert.Throws<NotSupportedException>(() => limiter.TryAcquire());
		}

		[Test]
		public void Should_TryAcquire_ThrowNotSupported_When_OnlyTokenlessAsyncDelegateProvided()
		{
			var limiter = new DelegateRateLimiter(() => Task.FromResult(RateLimitResult.Permitted));

			Assert.Throws<NotSupportedException>(() => limiter.TryAcquire());
		}

		[Test]
		public async Task Should_TryAcquireAsync_BridgeFromTokenlessSyncDelegate()
		{
			var limiter = new DelegateRateLimiter(() => RateLimitResult.Permitted);

			var result = await limiter.TryAcquireAsync();

			Assert.That(result.IsPermitted, Is.True);
		}

		[Test]
		public async Task Should_TryAcquireAsync_BridgeFromTokenlessAsyncDelegate()
		{
			var limiter = new DelegateRateLimiter(() => Task.FromResult(RateLimitResult.Denied(TimeSpan.FromMilliseconds(5))));

			var result = await limiter.TryAcquireAsync();

			Assert.That(result.IsPermitted, Is.False);
			Assert.That(result.RetryAfter, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
		}

		[Test]
		public void Should_ThrowArgumentNull_When_SyncDelegateIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new DelegateRateLimiter((Func<CancellationToken, RateLimitResult>)null));
			Assert.Throws<ArgumentNullException>(() => new DelegateRateLimiter((Func<RateLimitResult>)null));
		}

		[Test]
		public void Should_ThrowArgumentNull_When_AsyncDelegateIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new DelegateRateLimiter((Func<CancellationToken, Task<RateLimitResult>>)null));
			Assert.Throws<ArgumentNullException>(() => new DelegateRateLimiter((Func<Task<RateLimitResult>>)null));
		}
	}
}
