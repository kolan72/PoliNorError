using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	public abstract partial class PolicyProcessor
	{
		public class ExceptionFilter
		{
			public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => FilterSet.IncludedErrorFilters;

			public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => FilterSet.ExcludedErrorFilters;

			internal ExceptionFilterSet FilterSet { get; } = new ExceptionFilterSet();

			internal void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				FilterSet.IncludeFilter(handledErrorFilter);
			}

			internal void AddIncludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			internal void AddIncludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			internal void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				FilterSet.ExcludeFilter(handledErrorFilter);
			}

			internal void AddExcludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			internal void AddExcludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			internal void AppendFilter(ExceptionFilter exceptionFilter)
			{
				FilterSet.AppendFilter(exceptionFilter.FilterSet);
			}

			internal Func<Exception, bool> GetCanHandle()
			{
				return FilterSet.CompilePredicate();
			}
		}
	}
}
