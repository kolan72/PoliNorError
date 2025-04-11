using System;
using System.Linq.Expressions;

namespace PoliNorError.Extensions.PolicyErrorFiltering
{
	public static class PolicyErrorFiltering
	{
		public static T ExcludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : Policy
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		public static T ExcludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : Policy where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(func);
			return errorPolicy;
		}

		public static T IncludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : Policy
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		public static T IncludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : Policy where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(func);
			return errorPolicy;
		}

		public static T IncludeErrorSet<T, TException1, TException2>(this T errorPolicy) where T : Policy where TException1 : Exception where TException2 : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorSet<TException1, TException2>();
			return errorPolicy;
		}

		public static T IncludeErrorSet<T>(this T errorPolicy, IErrorSet errorSet) where T : Policy
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorSet(errorSet);
			return errorPolicy;
		}

		public static T ExcludeErrorSet<T, TException1, TException2>(this T errorPolicy) where T : Policy where TException1 : Exception where TException2 : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorSet<TException1, TException2>();
			return errorPolicy;
		}

		public static T ExcludeErrorSet<T>(this T errorPolicy, IErrorSet errorSet) where T : Policy
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorSet(errorSet);
			return errorPolicy;
		}

		public static T IncludeInnerError<T, TInnerException>(this T errorPolicy, Func<TInnerException, bool> func = null) where T : Policy where TInnerException : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedInnerErrorFilter(func);
			return errorPolicy;
		}

		public static T ExcludeInnerError<T, TInnerException>(this T errorPolicy, Func<TInnerException, bool> func = null) where T : Policy where TInnerException : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedInnerErrorFilter(func);
			return errorPolicy;
		}

		public static T AddErrorFilter<T>(this T policyProcessor, Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory) where T : IPolicyProcessor
		{
			policyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return policyProcessor;
		}

		public static T AddErrorFilter<T>(this T policyProcessor, NonEmptyCatchBlockFilter filter) where T : IPolicyProcessor
		{
			policyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return policyProcessor;
		}
	}
}
