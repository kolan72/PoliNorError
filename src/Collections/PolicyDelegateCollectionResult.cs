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

		// Served from the base-class cache: previously an O(n) LastOrDefault() re-enumeration per access.
		public PolicyResult LastPolicyResult => (LastHandledResult as PolicyDelegateResult)?.Result;

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
				return !IsSuccess ? default : LastPolicyResult.GetResultOrDefault();
			}
		}

		public PolicyResult<T> LastPolicyResult => (LastHandledResult as PolicyDelegateResult<T>)?.Result;

		public IEnumerator<PolicyDelegateResult<T>> GetEnumerator() => PolicyDelegateResults.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class PolicyDelegateCollectionResultBase
	{
		internal PolicyDelegateCollectionResultBase(IEnumerable<PolicyDelegateResultBase> policyHandledResults, LastPolicyResultState lastPolicyResultState = null, PolicyResultFailedReason failedReason = PolicyResultFailedReason.None)
		{
			// LastOrDefault() replaces the previous Any() + Last() pair: one enumeration instead of two.
			var lastResult = policyHandledResults.LastOrDefault();
			if (lastResult == null)
			{
				IsFailed = lastPolicyResultState?.IsFailed == true;
				IsCanceled = lastPolicyResultState?.IsCanceled == true;
				LastPolicyResultFailedReason = failedReason;
			}
			else
			{
				IsFailed = lastResult.IsFailed;
				IsCanceled = lastResult.IsCanceled;
				LastPolicyResultFailedReason = lastResult.FailedReason;
				IsSuccess = !IsFailed && !IsCanceled;
				LastHandledResult = lastResult;
			}
		}

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public bool IsCanceled { get; }

		public PolicyResultFailedReason LastPolicyResultFailedReason { get; internal set; }

		/// <summary>Last handled result captured during construction; null when nothing was handled.</summary>
		protected PolicyDelegateResultBase LastHandledResult { get; }
	}
}
