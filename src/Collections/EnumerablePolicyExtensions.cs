﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class EnumerablePolicyExtensions
	{
		public static void AddIncludedErrorFilter(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			}
		}

		public static void AddIncludedErrorFilter<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddIncludedErrorFilter(func);
			}
		}

		public static void AddExcludedErrorFilter(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			}
		}

		public static void AddExcludedErrorFilter<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddExcludedErrorFilter(func);
			}
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			policies.SetResultHandler(act.ToCancelableAction(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Action<PolicyResult, CancellationToken> act)
		{
			foreach (var policy in policies)
			{
				(policy as HandleErrorPolicyBase)?.AddPolicyResultHandler(act);
			}
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			SetResultHandler(policies, func.ToCancelableFunc(convertType));
		}

		internal static void SetResultHandler(this IEnumerable<IPolicyBase> policies, Func<PolicyResult, CancellationToken, Task> func)
		{
			foreach (var policy in policies)
			{
				(policy as HandleErrorPolicyBase)?.AddPolicyResultHandler(func);
			}
		}
	}
}