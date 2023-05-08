namespace PoliNorError
{
	internal static class PolicyHandledResultCollectionExtensions
	{
        internal static PolicyResult AddPolicyHandledResultCanceled(this FlexSyncEnumerable<PolicyHandledResult> handledResults, PolicyDelegate si)
        {
            var curPolResult = PolicyResult.ForSync();
            curPolResult.SetCanceled();

            handledResults.AddPolicyHandledResult(si, curPolResult);
            return curPolResult;
        }

        internal static PolicyResult<T> AddPolicyHandledResultCanceled<T>(this FlexSyncEnumerable<PolicyHandledResult<T>> handledResults, PolicyDelegate<T> si)
        {
            var curPolResult = PolicyResult<T>.ForSync();
            curPolResult.SetCanceled();

            handledResults.AddPolicyHandledResult(si, curPolResult);
            return curPolResult;
        }

        internal static void AddPolicyHandledResult(this FlexSyncEnumerable<PolicyHandledResult> handledResults, PolicyDelegate si, PolicyResult policyResult)
        {
            handledResults.Add(new PolicyHandledResult(PolicyHandledInfo.FromPolicyDelegate(si), policyResult));
        }

        internal static void AddPolicyHandledResult<T>(this FlexSyncEnumerable<PolicyHandledResult<T>> handledResults, PolicyDelegate<T> si, PolicyResult<T> policyResult)
        {
            handledResults.Add(new PolicyHandledResult<T>(PolicyHandledInfo.FromPolicyDelegate(si), policyResult));
        }
    }
}
