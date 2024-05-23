using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal static class CatchBlockFilterExtensions
	{
		public static T IncludeError<T, TException>(this T filter, Func<TException, bool> func = null) where T : CatchBlockFilter where TException : Exception
		{
			filter.ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return filter;
		}

		public static T IncludeError<T>(this T filter, Expression<Func<Exception, bool>> expression) where T : CatchBlockFilter
		{
			filter.ErrorFilter.AddIncludedErrorFilter(expression);
			return filter;
		}

		public static T ExcludeError<T, TException>(this T filter, Func<TException, bool> func = null) where T : CatchBlockFilter where TException : Exception
		{
			filter.ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return filter;
		}

		public static T ExcludeError<T>(this T filter, Expression<Func<Exception, bool>> expression) where T : CatchBlockFilter
		{
			filter.ErrorFilter.AddExcludedErrorFilter(expression);
			return filter;
		}
	}
}
