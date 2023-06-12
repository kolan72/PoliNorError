using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	internal static class PolicyDelegateResultCollectionExtensions
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

		public static PolicyDelegateCollectionResultStatus GetStatus(this IEnumerable<PolicyDelegateResult> policyHandledResultBaseCollection)
		{
			if (!policyHandledResultBaseCollection.Any())
				return PolicyDelegateCollectionResultStatus.Created;
			return GetHandledResultStatus(policyHandledResultBaseCollection.Last());
		}

		public static PolicyDelegateCollectionResultStatus GetStatus<T>(this IEnumerable<PolicyDelegateResult<T>> policyHandledResultBaseCollection)
		{
			if (!policyHandledResultBaseCollection.Any())
				return PolicyDelegateCollectionResultStatus.Created;
			return GetHandledResultStatus(policyHandledResultBaseCollection.Last().ToPolicyDelegateResult());
		}

		private static PolicyDelegateCollectionResultStatus GetHandledResultStatus(PolicyDelegateResult lastHandledResult)
		{
			if (lastHandledResult.Result == null)
				return PolicyDelegateCollectionResultStatus.None;

			if (lastHandledResult.Result.IsFailed || lastHandledResult.Result.IsCanceled)
			{
				var res = PolicyDelegateCollectionResultStatus.None;
				if (lastHandledResult.Result.IsFailed)
					res |= PolicyDelegateCollectionResultStatus.Faulted;

				if (lastHandledResult.Result.IsCanceled)
					res |= PolicyDelegateCollectionResultStatus.Canceled;

				return res;
			}
			else if (lastHandledResult.Result.IsOk)
			{
				return PolicyDelegateCollectionResultStatus.LastPolicyOk;
			}
			else
			{
				return PolicyDelegateCollectionResultStatus.LastPolicySuccess;
			}
		}
	}
}
