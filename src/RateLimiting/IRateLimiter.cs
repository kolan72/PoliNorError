using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Decides whether an execution attempt is allowed to proceed.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementations must be thread-safe. A permitted acquisition consumes one unit of the budget
	/// immediately and the unit is <b>not</b> refunded when the gated delegate later fails or is canceled.
	/// <see cref="RateLimitResult.RetryAfter"/> is ignored when the result is permitted.
	/// </para>
	/// <para>
	/// Cancellation is signaled by throwing <see cref="OperationCanceledException"/> — never by returning a denial,
	/// and a canceled acquisition never consumes a permit.
	/// </para>
	/// <para>
	/// One instance represents one budget scope; a single limiter instance may be shared by several policies.
	/// </para>
	/// </remarks>
	public interface IRateLimiter
	{
		/// <summary>
		/// Synchronously attempts to acquire one permit.
		/// </summary>
		/// <param name="token">A cancellation token to cancel the acquisition attempt.</param>
		/// <returns>A <see cref="RateLimitResult"/> describing whether the permit was granted.</returns>
		RateLimitResult TryAcquire(CancellationToken token = default);

		/// <summary>
		/// Asynchronously attempts to acquire one permit.
		/// </summary>
		/// <param name="token">A cancellation token to cancel the acquisition attempt.</param>
		/// <returns>A task that yields a <see cref="RateLimitResult"/> describing whether the permit was granted.</returns>
		Task<RateLimitResult> TryAcquireAsync(CancellationToken token = default);
	}
}
