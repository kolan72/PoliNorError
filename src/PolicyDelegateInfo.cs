using System.Reflection;

namespace PoliNorError
{
	public sealed class PolicyDelegateInfo
	{
		public PolicyDelegateInfo(IPolicyBase policy, MethodInfo methodInfo)
		{
			Policy = policy;
			PolicyMethodInfo = methodInfo;
		}

		internal static PolicyDelegateInfo FromPolicyDelegate(PolicyDelegate policyDelegateInfo)
		{
			return new PolicyDelegateInfo(policyDelegateInfo.Policy, policyDelegateInfo.GetMethodInfo());
		}

		internal static PolicyDelegateInfo FromPolicyDelegate<T>(PolicyDelegate<T> policyDelegateInfo)
		{
			return new PolicyDelegateInfo(policyDelegateInfo.Policy, policyDelegateInfo.GetMethodInfo());
		}

		public IPolicyBase Policy { get; }

		public MethodInfo PolicyMethodInfo { get; }
	}
}
