using System.Reflection;

namespace PoliNorError
{
	public sealed class PolicyDelegateResult
	{
		public PolicyDelegateResult(PolicyResult result, string policyName, MethodInfo methodInfo)
		{
			Result = result;
			PolicyName = policyName;
			PolicyMethodInfo = methodInfo;
		}

		public PolicyResult Result { get; }

		public string PolicyName { get; }

		public MethodInfo PolicyMethodInfo { get; }
	}

	public sealed class PolicyDelegateResult<T>
	{
		public PolicyDelegateResult(PolicyResult<T> result, string policyName, MethodInfo methodInfo)
		{
			Result = result;
			PolicyName = policyName;
			PolicyMethodInfo = methodInfo;
		}

		public PolicyResult<T> Result { get; }

		public string PolicyName { get; }

		public MethodInfo PolicyMethodInfo { get; }
	}
}
