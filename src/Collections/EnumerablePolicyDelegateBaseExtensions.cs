using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class EnumerablePolicyDelegateBaseExtensions
	{
		internal static bool AnyWithoutDelegate(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			return policyDelegateInfos.Any(Predicates.Not(PolicyDelegatePredicates.WithDelegateFunc));
		}

		internal static bool AnyWithDelegate(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			return policyDelegateInfos.Any(PolicyDelegatePredicates.WithDelegateFunc);
		}

		internal static bool WithDelegateExistsAndLastAndNewWithoutDelegate(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, PolicyDelegateBase newDelegateInfo)
		{
			return policyDelegateInfos.LastOrDefault().IsNotNullAndWithoutDelegate() && newDelegateInfo.IsNotNullAndWithoutDelegate() && policyDelegateInfos.AnyWithDelegate();
		}

		internal static bool WithoutDelegateExistsAndLastWithDelegate(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			return policyDelegateInfos.LastOrDefault().IsNotNullAndWithDelegate() && policyDelegateInfos.AnyWithoutDelegate();
		}

		internal static PolicyDelegateHandleType GetHandleType(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			if (policyDelegateInfos.Any(si => si.SyncType == SyncPolicyDelegateType.Sync))
			{
				if (policyDelegateInfos.Any(si => si.SyncType == SyncPolicyDelegateType.Async))
				{
					return PolicyDelegateHandleType.Misc;
				}
				else
				{
					return PolicyDelegateHandleType.Sync;
				}
			}
			else
			{
				return PolicyDelegateHandleType.Async;
			}
		}

		internal static void AddIncludedErrorFilterForAll(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddIncludedErrorFilterForAll(handledErrorFilter);
		}

		internal static void AddIncludedErrorFilterForAll<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddIncludedErrorFilterForAll(func);
		}

		internal static void AddExcludedErrorFilterForAll(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddExcludedErrorFilterForAll(handledErrorFilter);
		}

		internal static void AddExcludedErrorFilterForAll<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddExcludedErrorFilterForAll(func);
		}

		internal static void AddIncludedErrorFilterForLast(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateInfos.GetPolicies().AddIncludedErrorFilterForLast(handledErrorFilter);
		}

		internal static void AddIncludedErrorFilterForLast<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateInfos.GetPolicies().AddIncludedErrorFilterForLast(func);
		}

		internal static void AddExcludedErrorFilterForLast(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateInfos.GetPolicies().AddExcludedErrorFilterForLast(handledErrorFilter);
		}

		internal static void AddExcludedErrorFilterForLast<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateInfos.GetPolicies().AddExcludedErrorFilterForLast(func);
		}

		internal static void ThrowIfInconsistency(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, PolicyDelegateBase newDelegateInfo)
		{
			if (policyDelegateInfos.WithDelegateExistsAndLastAndNewWithoutDelegate(newDelegateInfo))
			{
				throw new InconsistencyPolicyException("Can not add more than one policy without delegate if there is a policy with delegate.");
			}
			if (policyDelegateInfos.WithoutDelegateExistsAndLastWithDelegate())
			{
				throw new InconsistencyPolicyException("Can not add policy with delegate if there is a policy without delegate.");
			}
		}

		internal static void ThrowIfAnyPolicyWithoutDelegateExists(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			if (policyDelegateInfos.AnyWithoutDelegate())
			{
				throw new InconsistencyPolicyException("All elements should be with a delegate!");
			}
		}

		internal static IEnumerable<IPolicyBase> GetPolicies(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			return policyDelegateInfos.Select(si => si.Policy);
		}
	}
}
