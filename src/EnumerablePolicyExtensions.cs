using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PoliNorError
{
	internal static class EnumerablePolicyExtensions
	{
		public static void AddIncludedErrorFilter(this IEnumerable<IPolicyBase> policies, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.ErrorFilter.AddIncludedErrorFilter(handledErrorFilter);
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
				pol.PolicyProcessor.ErrorFilter.AddExcludedErrorFilter(handledErrorFilter);
			}
		}

		public static void AddExcludedErrorFilter<TException>(this IEnumerable<IPolicyBase> policies, Func<TException, bool> func = null) where TException : Exception
		{
			foreach (var pol in policies)
			{
				pol.PolicyProcessor.AddExcludedErrorFilter(func);
			}
		}
	}
}
