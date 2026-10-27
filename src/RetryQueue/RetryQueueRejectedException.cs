using System;

namespace PoliNorError
{
	/// <summary>
	/// The exception thrown when a <see cref="RetryQueue"/> does not accept a retry session:
	/// the queue capacity is reached with the <see cref="RetryQueueOverflowPolicy.Reject"/> overflow policy,
	/// or the queue has already been completed or canceled.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class RetryQueueRejectedException : InvalidOperationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RetryQueueRejectedException"/> class.
		/// </summary>
		/// <param name="msg">The message that describes the error.</param>
		public RetryQueueRejectedException(string msg) : base(msg) {}
	}
}