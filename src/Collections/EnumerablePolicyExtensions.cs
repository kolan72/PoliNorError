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
		public static void AddIncludedErrorFilter<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			policies.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
		}

		public static void AddIncludedErrorFilter(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			}
		}

		public static void AddExcludedErrorFilter<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			policies.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
		}

		public static void AddExcludedErrorFilter(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			}
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(act);
			}
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act, CancellationType convertType)
		{
			policies.SetResultHandler(act.ToCancelableAction(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult, CancellationToken> act)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(act);
			}
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(func);
			}
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func, CancellationType convertType)
		{
			SetResultHandler(policies, func.ToCancelableFunc(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, CancellationToken, Task> func)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(func);
			}
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>> act)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(act);
			}
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>> act, CancellationType convertType)
		{
			policies.SetResultHandler(act.ToCancelableAction(convertType));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Action<PolicyResult<T>, CancellationToken> act)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(act);
			}
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, Task> func)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(func);
			}
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			SetResultHandler(policies, func.ToCancelableFunc(convertType));
		}

		internal static void SetResultHandler<T>(this IEnumerable<IPolicyBase> policies, Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			foreach (var policy in policies)
			{
				(policy as Policy)?.AddPolicyResultHandlerInner(func);
			}
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
	}
}
