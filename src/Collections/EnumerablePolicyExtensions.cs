using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class EnumerablePolicyExtensions
	{
		public static void AddIncludedErrorFilterForAll<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			policies.ActionForAll((p) => p.PolicyProcessor.AddIncludedErrorFilter(func));
		}

		public static void AddIncludedErrorFilterForAll(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policies.ActionForAll((p) => p.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter));
		}

		public static void AddExcludedErrorFilterForAll<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddExcludedErrorFilter(func);
			}
		}

		public static void AddExcludedErrorFilterForAll(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			}
		}

		public static void AddIncludedErrorFilterForLast<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			policies.LastOrDefault()?.PolicyProcessor.AddIncludedErrorFilter(func);
		}

		public static void AddIncludedErrorFilterForLast(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policies.LastOrDefault()?.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
		}

		public static void AddExcludedErrorFilterForLast<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			policies.LastOrDefault()?.PolicyProcessor.AddExcludedErrorFilter(func);
		}

		public static void AddExcludedErrorFilterForLast(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policies.LastOrDefault()?.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
		}

		public static void AddIncludedErrorSetFilter<TException1, TException2>(this IEnumerable<IPolicyBase> policies) where TException1 : Exception where TException2 : Exception
		{
			policies.LastOrDefault()?.PolicyProcessor.AddIncludedErrorSet<TException1, TException2>();
		}

		public static void AddIncludedErrorSetFilter(this IEnumerable<IPolicyBase> policies, IErrorSet errorSet)
		{
			policies.LastOrDefault()?.PolicyProcessor.AddIncludedErrorSet(errorSet);
		}

		public static void AddExcludedErrorSetFilter<TException1, TException2>(this IEnumerable<IPolicyBase> policies) where TException1 : Exception where TException2 : Exception
		{
			policies.LastOrDefault()?.PolicyProcessor.AddExcludedErrorSet<TException1, TException2>();
		}

		public static void AddExcludedErrorSetFilter(this IEnumerable<IPolicyBase> policies, IErrorSet errorSet)
		{
			policies.LastOrDefault()?.PolicyProcessor.AddExcludedErrorSet(errorSet);
		}

		public static void AddIncludedInnerErrorFilter<TInnerException>(this IEnumerable<IPolicyBase> policies, Func<TInnerException, bool> func = null) where TInnerException : Exception
		{
			policies.LastOrDefault()?.PolicyProcessor.AddIncludedInnerErrorFilter(func);
		}

		public static void AddExcludedInnerErrorFilter<TInnerException>(this IEnumerable<IPolicyBase> policies, Func<TInnerException, bool> func = null) where TInnerException : Exception
		{
			policies.LastOrDefault()?.PolicyProcessor.AddExcludedInnerErrorFilter(func);
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(act));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act, CancellationType convertType)
		{
			policies.SetResultHandler(act.ToCancelableAction(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult, CancellationToken> act)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(act));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(func));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func, CancellationType convertType)
		{
			SetResultHandler(policies, func.ToCancelableFunc(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, CancellationToken, Task> func)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(func));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>> act)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(act));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>> act, CancellationType convertType)
		{
			policies.SetResultHandler(act.ToCancelableAction(convertType));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>, CancellationToken> act)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(act));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, Task> func)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(func));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			SetResultHandler(policies, func.ToCancelableFunc(convertType));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			policies.ActionForAll((p) => (p as Policy)?.AddPolicyResultHandlerInner(func));
		}

		internal static Policy LastPolicy(this IEnumerable<IPolicyBase> policies)
		{
			return policies.LastOrDefault() as Policy;
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(act);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act, CancellationType convertType)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(act, convertType);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner(this IEnumerable<IPolicyBase> policies, Action<PolicyResult, CancellationToken> act)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(act);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(func);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func, CancellationType convertType)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(func, convertType);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, CancellationToken, Task> func)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(func);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>> act)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(act);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>> act, CancellationType convertType)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(act, convertType);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>, CancellationToken> act)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(act);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, Task> func)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(func);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(func, convertType);
		}

		internal static void AddPolicyResultHandlerToLastPolicyInner<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			policies.LastPolicy()?.AddPolicyResultHandlerInner(func);
		}

		internal static void SetPolicyResultFailedIfInner(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, bool> predicate)
		{
			policies.LastPolicy()?.SetPolicyResultFailedIfInner(predicate);
		}

		internal static void SetPolicyResultFailedIfInner<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, bool> predicate)
		{
			policies.LastPolicy()?.SetPolicyResultFailedIfInner(predicate);
		}
	}
}
