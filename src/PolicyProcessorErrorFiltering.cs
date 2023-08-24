using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal static class PolicyProcessorErrorFiltering
	{
		internal static T IncludeError<T, TException>(this T policyProcessor, Func<TException, bool> func = null) where T : IPolicyProcessor where TException : Exception
		{
			policyProcessor.AddIncludedErrorFilter(func);
			return policyProcessor;
		}

		internal static T IncludeError<T>(this T policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyProcessor
		{
			policyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			return policyProcessor;
		}

		internal static T ExcludeError<T, TException>(this T policyProcessor, Func<TException, bool> func = null) where T : IPolicyProcessor where TException : Exception
		{
			policyProcessor.AddExcludedErrorFilter(func);
			return policyProcessor;
		}

		internal static T ExcludeError<T>(this T policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyProcessor
		{
			policyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return policyProcessor;
		}

		internal static void AddIncludedErrorFilter(this IPolicyProcessor policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(handledErrorFilter);
		}

		internal static void AddIncludedErrorFilter<TException>(this IPolicyProcessor policyProcessor, Func<TException, bool> func = null) where TException : Exception
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
		}

		internal static void AddExcludedErrorFilter(this IPolicyProcessor policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(handledErrorFilter);
		}

		internal static void AddExcludedErrorFilter<TException>(this IPolicyProcessor policyProcessor, Func<TException, bool> func = null) where TException : Exception
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
		}
	}
}
