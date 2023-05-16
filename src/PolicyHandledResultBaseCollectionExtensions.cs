using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	internal static class PolicyHandledResultBaseCollectionExtensions
	{
		public static PolicyDelegateCollectionResultStatus GetStatus(this IEnumerable<PolicyHandledResult> policyHandledResultBaseCollection)
		{
			if (!policyHandledResultBaseCollection.Any())
				return PolicyDelegateCollectionResultStatus.Created;
			return GetHandledResultStatus(policyHandledResultBaseCollection.Last());
		}

		public static PolicyDelegateCollectionResultStatus GetStatus<T>(this IEnumerable<PolicyHandledResult<T>> policyHandledResultBaseCollection)
		{
			if (!policyHandledResultBaseCollection.Any())
				return PolicyDelegateCollectionResultStatus.Created;
			return GetHandledResultStatus(policyHandledResultBaseCollection.Last().ToPolicyHandledResult());
		}

		private static PolicyDelegateCollectionResultStatus GetHandledResultStatus(PolicyHandledResult lastHandledResult)
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
