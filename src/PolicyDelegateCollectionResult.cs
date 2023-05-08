using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public class PolicyDelegateCollectionResult : IEnumerable<PolicyHandledResult>
	{
		public PolicyDelegateCollectionResult(IEnumerable<PolicyHandledResult> policyHandledResults, IEnumerable<PolicyDelegate> policyDelegatesUnused)
		{
			PolicyHandledResults = policyHandledResults;
			PolicyDelegatesUnused = policyDelegatesUnused;
		}

        public PolicyDelegateCollectionResultStatus Status => PolicyHandledResults.GetStatus();

        public Exception LastFailedError
		{
			get
			{
				if (Status != PolicyDelegateCollectionResultStatus.Faulted)
					return null;

				return PolicyHandledResults.Last().Result.Errors.LastOrDefault();
			}
		}

		public IEnumerable<PolicyDelegate> PolicyDelegatesUnused { get; }
		public IEnumerable<PolicyHandledResult> PolicyHandledResults { get; }

		public IEnumerator<PolicyHandledResult> GetEnumerator() => PolicyHandledResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResult<T> : IEnumerable<PolicyHandledResult<T>>
	{
		public PolicyDelegateCollectionResult(IEnumerable<PolicyHandledResult<T>> policyHandledResultsT, IEnumerable<PolicyDelegate<T>> policyDelegatesUnused)
        {
			PolicyHandledResults = policyHandledResultsT;
			PolicyDelegatesUnused = policyDelegatesUnused;
		}

		IEnumerator<PolicyHandledResult<T>> IEnumerable<PolicyHandledResult<T>>.GetEnumerator() => PolicyHandledResults.GetEnumerator();

		public T Result
		{
			get
			{
				var status = PolicyHandledResults.GetStatus();
				if (status == PolicyDelegateCollectionResultStatus.Created)
					return default;
				var lastHandledResult = PolicyHandledResults.Last();
				return lastHandledResult.Result.Result;
			}
		}

		public IEnumerable<PolicyHandledResult<T>> PolicyHandledResults { get; }

		public IEnumerable<PolicyDelegate<T>> PolicyDelegatesUnused { get; }

		public IEnumerator<PolicyHandledResult<T>> GetEnumerator() => PolicyHandledResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public PolicyDelegateCollectionResultStatus Status => PolicyHandledResults.GetStatus();
	}

	[Flags]
	public enum PolicyDelegateCollectionResultStatus
	{
		None = 0,
		Created = 1,
		LastPolicySuccess = 2,
		LastPolicyOk = 4,
		Canceled = 8,
		Faulted = 16,
		FaultedCanceled = Faulted | Canceled
	}
}
