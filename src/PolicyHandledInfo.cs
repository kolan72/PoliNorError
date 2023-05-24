using System.Reflection;

namespace PoliNorError
{
	public sealed class PolicyHandledInfo
	{
		public PolicyHandledInfo(IPolicyBase policy, MethodInfo methodInfo)
		{
			Policy = policy;
			PolicyMethodInfo = methodInfo;
		}

		internal static PolicyHandledInfo FromPolicyDelegate(PolicyDelegate policyDelegateInfo)
		{
			return new PolicyHandledInfo(policyDelegateInfo.Policy, policyDelegateInfo.GetMethodInfo());
		}

		internal static PolicyHandledInfo FromPolicyDelegate<T>(PolicyDelegate<T> policyDelegateInfo)
		{
			return new PolicyHandledInfo(policyDelegateInfo.Policy, policyDelegateInfo.GetMethodInfo());
		}

		public IPolicyBase Policy { get; }

		public MethodInfo PolicyMethodInfo { get; }
	}
}
