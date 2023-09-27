using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public class PolicyDelegateCollectionResult : IEnumerable<PolicyDelegateResult>
	{
		internal PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult> policyHandledResults, IEnumerable<PolicyDelegate> policyDelegatesUnused)
		{
			PolicyDelegateResults = policyHandledResults;
			PolicyDelegatesUnused = policyDelegatesUnused;
			IsFailed = PolicyDelegateResults.GetLastResultFailed();
			IsSuccess = PolicyDelegateResults.GetLastResultSuccess();
		}

		public IEnumerable<PolicyDelegateResult> PolicyDelegateResults { get; }

		public IEnumerable<PolicyDelegate> PolicyDelegatesUnused { get; }

		public PolicyResult LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public IEnumerator<PolicyDelegateResult> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResult<T> : IEnumerable<PolicyDelegateResult<T>>
	{
		internal PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult<T>> policyHandledResultsT, IEnumerable<PolicyDelegate<T>> policyDelegatesUnused)
        {
			PolicyDelegateResults = policyHandledResultsT;
			PolicyDelegatesUnused = policyDelegatesUnused;
			IsFailed = PolicyDelegateResults.GetLastResultFailed();
			IsSuccess = PolicyDelegateResults.GetLastResultSuccess();
		}

		public IEnumerable<PolicyDelegateResult<T>> PolicyDelegateResults { get; }

		public IEnumerable<PolicyDelegate<T>> PolicyDelegatesUnused { get; }

		public T Result
		{
			get
			{
				return LastPolicyResult != null ? LastPolicyResult.Result : default;
			}
		}

		public PolicyResult<T> LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public IEnumerator<PolicyDelegateResult<T>> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
