using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal class ResultFilterSet<T>
	{
		private static readonly Func<T, bool> _defaultFilter = _ => true;

		internal List<Expression<Func<T, bool>>> ExcludedErrorFilters { get; } = new List<Expression<Func<T, bool>>>();

		internal void ExcludeFilter(Expression<Func<T, bool>> handledErrorFilter)
		{
			ExcludedErrorFilters.Add(handledErrorFilter);
		}

		internal void AppendFilter(ResultFilterSet<T> resultFilter)
		{
			foreach (var filter in resultFilter.ExcludedErrorFilters)
			{
				ExcludedErrorFilters.Add(filter);
			}
		}

		internal Func<T, bool> CompilePredicate()
		{
			if (ExcludedErrorFilters.Count == 0)
			{
				return _defaultFilter;
			}

			return ExcludedErrorFilters.GetOrCombined().Not().Compile();
		}
	}
}
