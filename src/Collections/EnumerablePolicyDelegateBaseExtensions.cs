using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class EnumerablePolicyDelegateBaseExtensions
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

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Action<PolicyResult, CancellationToken> act)
		{
			policyDelegateInfos.Select(pd => pd.Policy).SetResultHandler(act);
		}

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			policyDelegateInfos.Select(pd => pd.Policy).SetResultHandler(act, convertType);
		}

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			policyDelegateInfos.SetResultHandler(func.ToCancelableFunc(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<PolicyResult, CancellationToken, Task> func)
		{
			policyDelegateInfos.Select(pd => pd.Policy).SetResultHandler(func);
		}

		internal static PolicyDelegateHandleType GetHandleType(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			if (policyDelegateInfos.Any(si => si.UseSync == SyncPolicyDelegateType.Sync))
			{
				if (policyDelegateInfos.Any(si => si.UseSync == SyncPolicyDelegateType.Async))
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

		internal static void AddIncludedErrorFilter(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddIncludedErrorFilter(handledErrorFilter);
		}

		internal static void AddIncludedErrorFilter<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddIncludedErrorFilter(func);
		}

		internal static void AddExcludedErrorFilter(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddExcludedErrorFilter(handledErrorFilter);
		}

		internal static void AddExcludedErrorFilter<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateInfos.Select(pd => pd.Policy).AddExcludedErrorFilter(func);
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
