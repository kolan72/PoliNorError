using System;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class PolicyAddException : InvalidOperationException
	{
		public PolicyAddException(string msg) : base(msg){}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class InconsistencyPolicyException : PolicyAddException
	{
		private const string MSG = "Add policy will lead to inconsistency because of setting common delegate will be impossible.";

		public InconsistencyPolicyException() : base(MSG){}

		public InconsistencyPolicyException(string msg) : base(msg) {}
	}
}
