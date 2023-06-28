namespace PoliNorError
{
	internal static class EnumerablePolicyDelegateResultExtensions
	{
		internal static PolicyResult AddPolicyDelegateResultCanceled(this FlexSyncEnumerable<PolicyDelegateResult> handledResults, PolicyDelegate si)
		{
			var curPolResult = PolicyResult.ForSync();
			curPolResult.SetCanceled();

			handledResults.AddPolicyDelegateResult(si, curPolResult);
			return curPolResult;
		}

		internal static PolicyResult<T> AddPolicyDelegateResultCanceled<T>(this FlexSyncEnumerable<PolicyDelegateResult<T>> handledResults, PolicyDelegate<T> si)
		{
			var curPolResult = PolicyResult<T>.ForSync();
			curPolResult.SetCanceled();

			handledResults.AddPolicyDelegateResult(si, curPolResult);
			return curPolResult;
		}

		internal static void AddPolicyDelegateResult(this FlexSyncEnumerable<PolicyDelegateResult> handledResults, PolicyDelegate si, PolicyResult policyResult)
		{
			handledResults.Add(new PolicyDelegateResult(PolicyDelegateInfo.FromPolicyDelegate(si), policyResult));
		}

		internal static void AddPolicyDelegateResult<T>(this FlexSyncEnumerable<PolicyDelegateResult<T>> handledResults, PolicyDelegate<T> si, PolicyResult<T> policyResult)
		{
			handledResults.Add(new PolicyDelegateResult<T>(PolicyDelegateInfo.FromPolicyDelegate(si), policyResult));
		}
	}
}
