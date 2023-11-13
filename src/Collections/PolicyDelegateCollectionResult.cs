using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public class PolicyDelegateCollectionResult : PolicyDelegateCollectionResultBase, IEnumerable<PolicyDelegateResult>
	{
		internal PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult> policyHandledResults, IEnumerable<PolicyDelegate> policyDelegatesUnused, LastPolicyResultState lastPolicyResultState = null, PolicyResultFailedReason failedReason = PolicyResultFailedReason.None)
			:base(policyHandledResults, lastPolicyResultState, failedReason)
		{
			PolicyDelegateResults = policyHandledResults;
			PolicyDelegatesUnused = policyDelegatesUnused;
		}

		public IEnumerable<PolicyDelegateResult> PolicyDelegateResults { get; }

		public IEnumerable<PolicyDelegate> PolicyDelegatesUnused { get; }

		public PolicyResult LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public IEnumerator<PolicyDelegateResult> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResult<T> : PolicyDelegateCollectionResultBase, IEnumerable<PolicyDelegateResult<T>>
	{
		internal PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult<T>> policyHandledResultsT, IEnumerable<PolicyDelegate<T>> policyDelegatesUnused, LastPolicyResultState lastPolicyResultState = null, PolicyResultFailedReason failedReason = PolicyResultFailedReason.None)
			: base(policyHandledResultsT, lastPolicyResultState, failedReason)
		{
			PolicyDelegateResults = policyHandledResultsT;
			PolicyDelegatesUnused = policyDelegatesUnused;
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

		public IEnumerator<PolicyDelegateResult<T>> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResultBase
	{
		internal PolicyDelegateCollectionResultBase(IEnumerable<PolicyDelegateResultBase> policyHandledResults, LastPolicyResultState lastPolicyResultState = null, PolicyResultFailedReason failedReason = PolicyResultFailedReason.None)
		{
			IsFailed = policyHandledResults.GetLastResultFailed() || (lastPolicyResultState?.IsFailed == true);
			IsSuccess = policyHandledResults.GetLastResultSuccess();
			LastPolicyResultFailedReason = policyHandledResults.LastOrDefault()?.FailedReason ?? failedReason;
		}

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public PolicyResultFailedReason LastPolicyResultFailedReason { get; internal set; }
	}
}
