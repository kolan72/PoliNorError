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
		public static T AddPolicyResultHandlerForAll<T>(this T policyDelegateCollection, Action<PolicyResult, CancellationToken> act) where T: IEnumerable<PolicyDelegateBase>
		{
			policyDelegateCollection.SetResultHandler(act);
			return policyDelegateCollection;
		}

		public static T AddPolicyResultHandlerForAll<T>(this T policyDelegateCollection, Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IEnumerable<PolicyDelegateBase>
		{
			policyDelegateCollection.SetResultHandler(act, convertType);
			return policyDelegateCollection;
		}

		public static T AddPolicyResultHandlerForAll<T>(this T policyDelegateCollection, Func<PolicyResult, CancellationToken, Task> func) where T : IEnumerable<PolicyDelegateBase>
		{
			policyDelegateCollection.SetResultHandler(func);
			return policyDelegateCollection;
		}

		public static T AddPolicyResultHandlerForAll<T>(this T policyDelegateCollection, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IEnumerable<PolicyDelegateBase>
		{
			policyDelegateCollection.SetResultHandler(func, convertType);
			return policyDelegateCollection;
		}

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
			foreach (var polInfo in policyDelegateInfos)
			{
				(polInfo.Policy as HandleErrorPolicyBase).AddPolicyResultHandler(act);
			}
		}

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			foreach (var polInfo in policyDelegateInfos)
			{
				(polInfo.Policy as HandleErrorPolicyBase).AddPolicyResultHandler(act, convertType);
			}
		}

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<PolicyResult, CancellationToken, Task> func)
		{
			foreach (var polInfo in policyDelegateInfos)
			{
				(polInfo.Policy as HandleErrorPolicyBase).AddPolicyResultHandler(func);
			}
		}

		internal static void SetResultHandler(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			foreach (var polInfo in policyDelegateInfos)
			{
				(polInfo.Policy as HandleErrorPolicyBase).AddPolicyResultHandler(func, convertType);
			}
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
			foreach (var polInfo in policyDelegateInfos)
			{
				polInfo.Policy.PolicyProcessor.ErrorFilter.AddIncludedErrorFilter(handledErrorFilter);
			}
		}

		internal static void AddIncludedErrorFilter<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			foreach (var polInfo in policyDelegateInfos)
			{
				polInfo.Policy.PolicyProcessor.AddIncludedErrorFilter(func);
			}
		}

		internal static void AddExcludedErrorFilter(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var polInfo in policyDelegateInfos)
			{
				polInfo.Policy.PolicyProcessor.ErrorFilter.AddExcludedErrorFilter(handledErrorFilter);
			}
		}

		internal static void AddExcludedErrorFilter<TException>(this IEnumerable<PolicyDelegateBase> policyDelegateInfos, Func<TException, bool> func = null) where TException : Exception
		{
			foreach (var polInfo in policyDelegateInfos)
			{
				polInfo.Policy.PolicyProcessor.AddExcludedErrorFilter(func);
			}
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

		internal static void ThrowIfNotLastPolicyWithoutDelegateExists(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			if (policyDelegateInfos.SkipLast().AnyWithoutDelegate())
			{
				throw new InconsistencyPolicyException("Only the last element can be without a delegate!");
			}
		}

		internal static SettingPolicyDelegateResult CheckLastPolicyDelegateCanBeSet(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			if (policyDelegateInfos.IsEmpty())
				return SettingPolicyDelegateResult.Empty;
			if (policyDelegateInfos.LastOrDefaultIfEmpty().DelegateExists)
				return SettingPolicyDelegateResult.AlreadySet;

			return SettingPolicyDelegateResult.None;
		}

		internal static IEnumerable<IPolicyBase> GetPolicies(this IEnumerable<PolicyDelegateBase> policyDelegateInfos)
		{
			return policyDelegateInfos.Select(si => si.Policy);
		}
	}
}
