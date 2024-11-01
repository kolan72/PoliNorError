using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	public class PolicyResultHandlerFailedException : Exception
	{
		public PolicyResultHandlerFailedException(){}

		public PolicyResultHandlerFailedException(PolicyResult result) => Result = result;

		///<inheritdoc cref="PolicyResult"/>
		public PolicyResult Result { get; }
	}
}
