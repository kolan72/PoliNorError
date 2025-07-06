using System;
using System.Threading;

namespace PoliNorError
{
#pragma warning disable RCS1194 // Implement exception constructors.
	/// <summary>
	/// Mainly used for debugging purposes.
	/// Catching this exception implies that some obsolete code paths remain unrefactored.
	/// </summary>
	public class OperationFailedAndCanceledException : OperationCanceledException
#pragma warning restore RCS1194 // Implement exception constructors.
	{
		/// <summary>
		/// This token is needed so the exception token is not the default one.
		/// </summary>
		private static CancellationToken canceledToken = new CancellationToken(true);

		internal OperationFailedAndCanceledException() : base(canceledToken)
		{ }
	}
}
