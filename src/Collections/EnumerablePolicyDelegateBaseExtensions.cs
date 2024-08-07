﻿using System;
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

		public static void AddIncludedErrorSetFilter<TException1, TException2>(this IEnumerable<PolicyDelegateBase> policies) where TException1 : Exception where TException2 : Exception
		{
			policies.GetPolicies().AddIncludedErrorSetFilter<TException1, TException2>();
		}

		public static void AddIncludedErrorSetFilter(this IEnumerable<PolicyDelegateBase> policies, IErrorSet errorSet)
		{
			policies.GetPolicies().AddIncludedErrorSetFilter(errorSet);
		}

		public static void AddExcludedErrorSetFilter<TException1, TException2>(this IEnumerable<PolicyDelegateBase> policies) where TException1 : Exception where TException2 : Exception
		{
			policies.GetPolicies().AddExcludedErrorSetFilter<TException1, TException2>();
		}

		public static void AddExcludedErrorSetFilter(this IEnumerable<PolicyDelegateBase> policies, IErrorSet errorSet)
		{
			policies.GetPolicies().AddExcludedErrorSetFilter(errorSet);
		}

		public static void AddIncludedInnerErrorFilter<TInnerException>(this IEnumerable<PolicyDelegateBase> policies, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
		{
			policies.GetPolicies().AddIncludedInnerErrorFilter(predicate);
		}

		public static void AddExcludedInnerErrorFilter<TInnerException>(this IEnumerable<PolicyDelegateBase> policies, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
		{
			policies.GetPolicies().AddExcludedInnerErrorFilter(predicate);
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
