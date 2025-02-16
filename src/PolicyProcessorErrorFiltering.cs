using System;
using System.Linq.Expressions;
using static PoliNorError.ErrorSet;

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

		internal static T IncludeInnerError<T, TInnerException>(this T policyProcessor, Func<TInnerException, bool> func = null) where T : IPolicyProcessor where TInnerException : Exception
		{
			policyProcessor.AddIncludedInnerErrorFilter(func);
			return policyProcessor;
		}

		internal static T ExcludeInnerError<T, TInnerException>(this T policyProcessor, Func<TInnerException, bool> func = null) where T : IPolicyProcessor where TInnerException : Exception
		{
			policyProcessor.AddExcludedInnerErrorFilter(func);
			return policyProcessor;
		}

		internal static T IncludeErrorSet<T, TException1, TException2>(this T policyProcessor) where T : IPolicyProcessor where TException1 : Exception where TException2 : Exception
		{
			policyProcessor.AddIncludedErrorSet<TException1, TException2>();
			return policyProcessor;
		}

		internal static T IncludeErrorSet<T>(this T policyProcessor, IErrorSet errorSet) where T : IPolicyProcessor
		{
			policyProcessor.AddIncludedErrorSet(errorSet);
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

		internal static T ExcludeErrorSet<T, TException1, TException2>(this T policyProcessor) where T : IPolicyProcessor where TException1 : Exception where TException2 : Exception
		{
			policyProcessor.AddExcludedErrorSet<TException1, TException2>();
			return policyProcessor;
		}

		internal static T ExcludeErrorSet<T>(this T policyProcessor, IErrorSet errorSet) where T : IPolicyProcessor
		{
			policyProcessor.AddExcludedErrorSet(errorSet);
			return policyProcessor;
		}

		internal static void AddIncludedErrorFilter(this IPolicyProcessor policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(handledErrorFilter);
		}

		internal static void AddIncludedErrorFilter<TException>(this IPolicyProcessor policyProcessor, Func<TException, bool> func = null) where TException : Exception
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(func);
		}

		internal static void AddExcludedErrorFilter(this IPolicyProcessor policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(handledErrorFilter);
		}

		internal static void AddExcludedErrorFilter<TException>(this IPolicyProcessor policyProcessor, Func<TException, bool> func = null) where TException : Exception
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(func);
		}

		internal static void AddIncludedErrorSet<TException1, TException2>(this IPolicyProcessor policyProcessor) where TException1 : Exception where TException2 : Exception
		{
			policyProcessor
			.IncludeError<IPolicyProcessor, TException1>()
			.IncludeError<IPolicyProcessor, TException2>();
		}

		internal static void AddIncludedErrorSet(this IPolicyProcessor policyProcessor, IErrorSet errorSet)
		{
			foreach (var item in errorSet.Items)
			{
				policyProcessor.AddIncludedError(item);
			}
		}

		internal static void AddExcludedErrorSet<TException1, TException2>(this IPolicyProcessor policyProcessor) where TException1 : Exception where TException2 : Exception
		{
			policyProcessor
			.ExcludeError<IPolicyProcessor, TException1>()
			.ExcludeError<IPolicyProcessor, TException2>();
		}

		internal static void AddExcludedErrorSet(this IPolicyProcessor policyProcessor, IErrorSet errorSet)
		{
			foreach (var item in errorSet.Items)
			{
				policyProcessor.AddExcludedError(item);
			}
		}

		internal static void AddIncludedInnerErrorFilter<TInnerException>(this IPolicyProcessor policyProcessor, Func<TInnerException, bool> func = null) where TInnerException : Exception
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
		}

		internal static void AddExcludedInnerErrorFilter<TInnerException>(this IPolicyProcessor policyProcessor, Func<TInnerException, bool> func = null) where TInnerException : Exception
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
		}

		internal static void AddIncludedError(this IPolicyProcessor policyProcessor, ErrorSetItem errorSetItem)
		{
			policyProcessor.ErrorFilter.AddIncludedError(errorSetItem);
		}

		internal static void AddExcludedError(this IPolicyProcessor policyProcessor, ErrorSetItem errorSetItem)
		{
			policyProcessor.ErrorFilter.AddExcludedError(errorSetItem);
		}
	}
}
