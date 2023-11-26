using System.Collections.Generic;
using System.Linq;

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

		internal static bool GetLastResultFailed(this IEnumerable<PolicyDelegateResultBase> policyDelegateResults)
		{
			return policyDelegateResults.LastOrDefault()?.IsFailed == true;
		}

		internal static bool GetLastResultSuccess(this IEnumerable<PolicyDelegateResultBase> policyDelegateResults)
		{
			return policyDelegateResults.LastOrDefault()?.IsSuccess == true;
		}

		internal static bool GetLastResultCanceled(this IEnumerable<PolicyDelegateResultBase> policyDelegateResults)
		{
			return policyDelegateResults.LastOrDefault()?.IsCanceled == true;
		}
	}
}
