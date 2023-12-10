using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal static class PolicyErrorFiltering
	{
		internal static T ExcludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyBase
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T ExcludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : IPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(func);
			return errorPolicy;
		}

		internal static T IncludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyBase
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T IncludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : IPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(func);
			return errorPolicy;
		}

		internal static T IncludeErrorSet<T, TException1, TException2>(this T errorPolicy) where T : IPolicyBase where TException1 : Exception where TException2 : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorSet<TException1, TException2>();
			return errorPolicy;
		}

		internal static T ExcludeErrorSet<T, TException1, TException2>(this T errorPolicy) where T : IPolicyBase where TException1 : Exception where TException2 : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorSet<TException1, TException2>();
			return errorPolicy;
		}
	}
}
