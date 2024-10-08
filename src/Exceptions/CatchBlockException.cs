﻿using System;

namespace PoliNorError
{
	/// <summary>
	/// Represents an exception thrown within a catch block.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public sealed class CatchBlockException : Exception
	{
		public CatchBlockException(Exception processException, Exception handlingException, CatchBlockExceptionSource errorSource, bool isCritical = false) : this("Error within the catch block.", processException, handlingException, errorSource, isCritical) {}

		public CatchBlockException(string msg, Exception processException, Exception handlingException, CatchBlockExceptionSource errorSource = CatchBlockExceptionSource.Unknown, bool isCritical = false) : base(msg, handlingException)
		{
			ProcessingException = processException;
			IsCritical = isCritical;
			ExceptionSource = errorSource;
		}

		/// <summary>
		/// Returns exception that is thrown when a delegate is being handled.
		/// </summary>
		public Exception ProcessingException { get; }

		/// <summary>
		/// Gets a value that determines if the current exception leads to setting PolicyResult.IsFailed to true.
		/// </summary>
		public bool IsCritical { get; }

		/// <summary>
		/// Gets the source of the CatchBlockException thrown.
		/// </summary>
		public CatchBlockExceptionSource ExceptionSource { get; }
	}
}
