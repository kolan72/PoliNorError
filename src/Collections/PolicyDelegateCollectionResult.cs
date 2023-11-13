using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public class PolicyDelegateCollectionResult : IEnumerable<PolicyDelegateResult>
	{
		internal PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult> policyHandledResults, IEnumerable<PolicyDelegate> policyDelegatesUnused, LastPolicyResultState lastPolicyResultState = null, PolicyResultFailedReason failedReason = PolicyResultFailedReason.None)
		{
			PolicyDelegateResults = policyHandledResults;
			PolicyDelegatesUnused = policyDelegatesUnused;
			IsFailed = PolicyDelegateResults.GetLastResultFailed() || (lastPolicyResultState?.IsFailed == true);
			IsSuccess = PolicyDelegateResults.GetLastResultSuccess();
			LastPolicyResultFailedReason = PolicyDelegateResults.LastOrDefault()?.FailedReason ?? failedReason;
		}

		public IEnumerable<PolicyDelegateResult> PolicyDelegateResults { get; }

		public IEnumerable<PolicyDelegate> PolicyDelegatesUnused { get; }

		public PolicyResult LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public PolicyResultFailedReason LastPolicyResultFailedReason { get; internal set; }

		public IEnumerator<PolicyDelegateResult> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResult<T> : IEnumerable<PolicyDelegateResult<T>>
	{
		internal PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult<T>> policyHandledResultsT, IEnumerable<PolicyDelegate<T>> policyDelegatesUnused, LastPolicyResultState lastPolicyResultState = null, PolicyResultFailedReason failedReason = PolicyResultFailedReason.None)
        {
			PolicyDelegateResults = policyHandledResultsT;
			PolicyDelegatesUnused = policyDelegatesUnused;
			IsFailed = PolicyDelegateResults.GetLastResultFailed() || (lastPolicyResultState?.IsFailed == true);
			IsSuccess = PolicyDelegateResults.GetLastResultSuccess();
			LastPolicyResultFailedReason = PolicyDelegateResults.LastOrDefault()?.FailedReason ?? failedReason;
		}

		public IEnumerable<PolicyDelegateResult<T>> PolicyDelegateResults { get; }

		public IEnumerable<PolicyDelegate<T>> PolicyDelegatesUnused { get; }

		public T Result
		{
			get
			{
				return LastPolicyResult.GetResultOrDefault();
			}
		}

		public PolicyResult<T> LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public PolicyResultFailedReason LastPolicyResultFailedReason { get; internal set; }

		public IEnumerator<PolicyDelegateResult<T>> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
