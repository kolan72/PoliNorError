using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public sealed class CatchBlockException : Exception
	{
		public CatchBlockException(Exception processException, Exception handlingException, bool isCritical = false) : this("Error within the catch block.", processException, handlingException, isCritical) {}

		public CatchBlockException(string msg, Exception processException, Exception handlingException, bool isCritical = false) : base(msg, handlingException)
		{
			ProcessingException = processException;
			IsCritical = isCritical;
		}

		/// <summary>
		/// Returns exception that is thrown when a delegate is being handled.
		/// </summary>
		public Exception ProcessingException { get; }

		/// <summary>
		/// Gets a value that determines if the current exception leads to setting PolicyResult.IsFailed to true.
		/// </summary>
		public bool IsCritical { get; }
	}
}
