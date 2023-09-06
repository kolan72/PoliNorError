using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	public class NoDelegateException : Exception
	{
		internal NoDelegateException() : this("The delegate to handle is null.") {}

		public NoDelegateException(string msg) : base(msg){}
		public NoDelegateException(IPolicyBase policy)  : this(GetExceptionMessage(policy.PolicyName)) {}

		private static string GetExceptionMessage(string policyName) => $"Delegate for policy {policyName} was not set.";
	}
}
