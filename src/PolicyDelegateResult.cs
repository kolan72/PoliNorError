using System.Reflection;

namespace PoliNorError
{
	public sealed class PolicyDelegateResult : PolicyDelegateResultBase
	{
		internal PolicyDelegateResult(PolicyResult result, string policyName, MethodInfo methodInfo) : base(policyName, methodInfo)
		{
			Result = result;
		}

		public PolicyResult Result { get; }
	}

	public sealed class PolicyDelegateResult<T> : PolicyDelegateResultBase
	{
		internal PolicyDelegateResult(PolicyResult<T> result, string policyName, MethodInfo methodInfo) : base(policyName, methodInfo)
		{
			Result = result;
		}

		public PolicyResult<T> Result { get; }
	}

	public abstract class PolicyDelegateResultBase
	{
		protected PolicyDelegateResultBase(string policyName, MethodInfo methodInfo)
		{
			PolicyName = policyName;
			PolicyMethodInfo = methodInfo;
		}

		public string PolicyName { get; }

		public MethodInfo PolicyMethodInfo { get; }
	}
}
