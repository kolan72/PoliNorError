using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public sealed class CatchBlockException : Exception
	{
		public CatchBlockException(Exception processException, Exception handlingException, bool isCritical = false) : this("Error in catch block.", processException, handlingException, isCritical) {}

		public CatchBlockException(string msg, Exception processException, Exception handlingException, bool isCritical = false) : base(msg, handlingException)
		{
			ProcessException = processException;
			IsCritical = isCritical;
		}

		public Exception ProcessException { get; }

		public bool IsCritical { get; }
	}
}
