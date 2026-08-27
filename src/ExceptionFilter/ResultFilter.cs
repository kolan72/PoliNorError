using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	public class ResultFilter<T>
	{
		public IEnumerable<Expression<Func<T, bool>>> ResultFilters => FilterSet.ResultFilters;

		internal ResultFilterSet<T> FilterSet { get; } = new ResultFilterSet<T>();

		public ResultFilter<T> ExcludeResult(Expression<Func<T, bool>> expression)
		{
			AddExcludedResultFilter(expression);
			return this;
		}

		internal void AddExcludedResultFilter(Expression<Func<T, bool>> handledErrorFilter)
		{
			FilterSet.ExcludeResult(handledErrorFilter);
		}

		internal void AppendFilter(ResultFilter<T> resultFilter)
		{
			FilterSet.AppendFilter(resultFilter.FilterSet);
		}

		internal Func<T, bool> GetIsSuccessful()
		{
			return FilterSet.CompilePredicate();
		}

		internal ResultFilterSlim<T> GetSlim()
		{
			return new ResultFilterSlim<T>(this);
		}
	}
}