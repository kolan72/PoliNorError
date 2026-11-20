using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// An <see cref="IRateLimiter"/> that delegates permit acquisition to a user-supplied function.
	/// Also convenient as a test fake.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Contract — an async-only limiter cannot acquire synchronously.</b> An instance constructed with a
	/// <c>Func&lt;CancellationToken, Task&lt;RateLimitResult&gt;&gt;</c> or <c>Func&lt;Task&lt;RateLimitResult&gt;&gt;</c>
	/// delegate throws <see cref="NotSupportedException"/> from its synchronous <see cref="TryAcquire"/> member.
	/// This behavior is a deliberate, supported trade-off: use the asynchronous <c>HandleAsync</c> methods of
	/// <see cref="RateLimitPolicy"/> with such a limiter — never block on the asynchronous acquisition.
	/// </para>
	/// </remarks>
	public sealed class DelegateRateLimiter : IRateLimiter
	{
		private readonly Func<CancellationToken, RateLimitResult> _sync;
		private readonly Func<CancellationToken, Task<RateLimitResult>> _async;

		private DelegateRateLimiter(Func<CancellationToken, RateLimitResult> sync, Func<CancellationToken, Task<RateLimitResult>> notsync)
		{
			_sync = sync;
			_async = notsync;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateRateLimiter"/> class with a synchronous acquisition delegate.
		/// </summary>
		/// <param name="acquire">The delegate invoked to acquire a permit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="acquire"/> is <see langword="null"/>.</exception>
		public DelegateRateLimiter(Func<CancellationToken, RateLimitResult> acquire)
			: this((acquire ?? throw new ArgumentNullException(nameof(acquire))), null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateRateLimiter"/> class with an asynchronous acquisition delegate.
		/// </summary>
		/// <param name="acquireAsync">The delegate invoked to acquire a permit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="acquireAsync"/> is <see langword="null"/>.</exception>
		public DelegateRateLimiter(Func<CancellationToken, Task<RateLimitResult>> acquireAsync)
			: this(null, acquireAsync ?? throw new ArgumentNullException(nameof(acquireAsync))) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateRateLimiter"/> class with a synchronous, token-less acquisition delegate.
		/// </summary>
		/// <param name="acquire">The delegate invoked to acquire a permit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="acquire"/> is <see langword="null"/>.</exception>
		public DelegateRateLimiter(Func<RateLimitResult> acquire)
			: this(acquire != null ? (Func<CancellationToken, RateLimitResult>)(token => acquire()) : null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateRateLimiter"/> class with an asynchronous, token-less acquisition delegate.
		/// </summary>
		/// <param name="acquireAsync">The delegate invoked to acquire a permit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="acquireAsync"/> is <see langword="null"/>.</exception>
		public DelegateRateLimiter(Func<Task<RateLimitResult>> acquireAsync)
			: this(acquireAsync != null ? (Func<CancellationToken, Task<RateLimitResult>>)(token => acquireAsync()) : null) { }

		/// <summary>
		/// Synchronously attempts to acquire one permit by invoking the configured delegate.
		/// </summary>
		/// <param name="token">A cancellation token passed to the configured delegate.</param>
		/// <returns>A <see cref="RateLimitResult"/> describing whether the permit was granted.</returns>
		/// <exception cref="NotSupportedException">The limiter was created with an asynchronous-only delegate.</exception>
		public RateLimitResult TryAcquire(CancellationToken token = default)
		{
			if (_sync == null)
			{
				throw new NotSupportedException(
					$"This {nameof(DelegateRateLimiter)} was created with an asynchronous delegate and cannot acquire synchronously. Use HandleAsync instead of Handle.");
			}
			return _sync(token);
		}

		/// <summary>
		/// Asynchronously attempts to acquire one permit by invoking the configured delegate.
		/// A synchronous delegate is bridged with <see cref="Task.FromResult{TResult}(TResult)"/>.
		/// </summary>
		/// <param name="token">A cancellation token passed to the configured delegate.</param>
		/// <returns>A task that yields a <see cref="RateLimitResult"/> describing whether the permit was granted.</returns>
		public Task<RateLimitResult> TryAcquireAsync(CancellationToken token = default)
		{
			if (_async != null)
			{
				return _async(token);
			}
			return Task.FromResult(_sync(token));
		}
	}
}
