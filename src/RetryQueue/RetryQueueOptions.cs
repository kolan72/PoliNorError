using System;

namespace PoliNorError
{
	/// <summary>
	/// Describes the behavior of a <see cref="RetryQueue"/> when its capacity limit is reached.
	/// </summary>
	public enum RetryQueueOverflowPolicy
	{
		/// <summary>
		/// The enqueueing call waits until a free slot becomes available (backpressure).
		/// </summary>
		Wait,

		/// <summary>
		/// The enqueueing call throws a <see cref="RetryQueueRejectedException"/>.
		/// </summary>
		Reject
	}

	/// <summary>
	/// Provides configuration options for a <see cref="RetryQueue"/>.
	/// </summary>
	public sealed class RetryQueueOptions
	{
		/// <summary>
		/// Gets or sets the maximum number of retry sessions that can be held by the queue at the same time.
		/// The value <c>0</c> means the queue is unbounded. The default value is <c>0</c>.
		/// </summary>
		public int Capacity { get; set; }

		/// <summary>
		/// Gets or sets the behavior used when <see cref="Capacity"/> is reached and greater than zero.
		/// The default value is <see cref="RetryQueueOverflowPolicy.Wait"/>.
		/// </summary>
		public RetryQueueOverflowPolicy Overflow { get; set; } = RetryQueueOverflowPolicy.Wait;
	}
}