using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	public class PolicyResultHandlingException : Exception
	{
		public PolicyResultHandlingException(Exception handleResultException) : base("Error in handling policy result.", handleResultException) { }

		internal PolicyResultHandlingException(Exception handleResultException, int handlerIndex) : base("Error in handling policy result.", handleResultException)
		{
			HandlerIndex = handlerIndex;
		}

		/// <summary>
		/// PolicyResult handler index in the policy collection of handlers.
		/// </summary>
		public int HandlerIndex { get; }
	}
}
