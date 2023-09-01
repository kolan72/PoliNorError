namespace PoliNorError
{
	internal static class EnumerablePolicyDelegateResultExtensions
	{
		internal static void AddPolicyDelegateResult(this FlexSyncEnumerable<PolicyDelegateResult> handledResults, PolicyDelegate si, PolicyResult policyResult)
		{
			handledResults.Add(new PolicyDelegateResult(policyResult, si.Policy.PolicyName, si.GetMethodInfo()));
		}

		internal static void AddPolicyDelegateResult<T>(this FlexSyncEnumerable<PolicyDelegateResult<T>> handledResults, PolicyDelegate<T> si, PolicyResult<T> policyResult)
		{
			handledResults.Add(new PolicyDelegateResult<T>(policyResult, si.Policy.PolicyName, si.GetMethodInfo()));
		}
	}
}
