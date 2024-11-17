using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	public class PolicyResultHandlerFailedException<T> : Exception
	{
		public PolicyResultHandlerFailedException(PolicyResult<T> result) => Result = result;

		///<inheritdoc cref="PolicyResult{T}"/>
		public PolicyResult<T> Result { get; }
	}
}
