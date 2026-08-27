using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	public class ResultFilter<T>
	{
		public IEnumerable<Expression<Func<T, bool>>> ExcludedErrorFilters => FilterSet.ExcludedErrorFilters;

		internal ResultFilterSet<T> FilterSet { get; } = new ResultFilterSet<T>();

		public ResultFilter<T> ExcludeError(Expression<Func<T, bool>> expression)
		{
			AddExcludedErrorFilter(expression);
			return this;
		}

		internal void AddExcludedErrorFilter(Expression<Func<T, bool>> handledErrorFilter)
		{
			FilterSet.ExcludeFilter(handledErrorFilter);
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