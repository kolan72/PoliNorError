using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Exception used by the library as a marker for logical cancellation.
	/// Created by the library (not by a <see cref="CancellationToken"/>) in scenarios such as
	/// when a synchronous policy processor handles a delegate that waits on one or multiple tasks
	/// (for example, via <c>Task.Wait</c> or <c>Task.WaitAll</c>) and cancellation is observed on a linked token.
	/// Consumers should not treat this as a token-driven cancellation thrown by the runtime.
	/// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
	public class ServiceOperationCanceledException : OperationCanceledException
#pragma warning restore RCS1194 // Implement exception constructors.
    {
        internal ServiceOperationCanceledException() { }
    }
}
