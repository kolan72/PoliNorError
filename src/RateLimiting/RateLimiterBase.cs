using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Base class for rate limiters that implement only the synchronous acquisition.
	/// </summary>
	public abstract class RateLimiterBase : IRateLimiter
	{
		/// <summary>
		/// Synchronously attempts to acquire one permit.
		/// </summary>
		/// <param name="token">A cancellation token to cancel the acquisition attempt.</param>
		/// <returns>A <see cref="RateLimitResult"/> describing whether the permit was granted.</returns>
		public abstract RateLimitResult TryAcquire(CancellationToken token = default);

		/// <summary>
		/// Asynchronously attempts to acquire one permit by running <see cref="TryAcquire"/> on the calling thread.
		/// </summary>
		/// <param name="token">A cancellation token to cancel the acquisition attempt.</param>
		/// <returns>A completed task that yields a <see cref="RateLimitResult"/> describing whether the permit was granted.</returns>
		public virtual Task<RateLimitResult> TryAcquireAsync(CancellationToken token = default)
		{
			return Task.FromResult(TryAcquire(token));
		}
	}
}
