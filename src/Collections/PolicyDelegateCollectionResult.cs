using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public class PolicyDelegateCollectionResult : IEnumerable<PolicyDelegateResult>
	{
		public PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult> policyHandledResults, IEnumerable<PolicyDelegate> policyDelegatesUnused)
		{
			PolicyDelegateResults = policyHandledResults;
			PolicyDelegatesUnused = policyDelegatesUnused;
		}

        public Exception LastFailedError
		{
			get
			{
				return PolicyDelegateResults.Last().Result.Errors.LastOrDefault();
			}
		}

		public IEnumerable<PolicyDelegateResult> PolicyDelegateResults { get; }

		public IEnumerable<PolicyDelegate> PolicyDelegatesUnused { get; }

		public PolicyResult LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public IEnumerator<PolicyDelegateResult> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResult<T> : IEnumerable<PolicyDelegateResult<T>>
	{
		public PolicyDelegateCollectionResult(IEnumerable<PolicyDelegateResult<T>> policyHandledResultsT, IEnumerable<PolicyDelegate<T>> policyDelegatesUnused)
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
				return LastPolicyResult != null ? LastPolicyResult.Result : default;
			}
		}

		public PolicyResult<T> LastPolicyResult => PolicyDelegateResults.LastOrDefault()?.Result;

		public IEnumerator<PolicyDelegateResult<T>> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
