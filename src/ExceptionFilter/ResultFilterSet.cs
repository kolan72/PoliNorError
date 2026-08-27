using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal class ResultFilterSet<T>
	{
		private static readonly Func<T, bool> _defaultFilter = _ => true;

		internal List<Expression<Func<T, bool>>> ResultFilters { get; } = new List<Expression<Func<T, bool>>>();

		internal void ExcludeResult(Expression<Func<T, bool>> resultFilter)
		{
			ResultFilters.Add(resultFilter);
		}

		internal void AppendFilter(ResultFilterSet<T> resultFilter)
		{
			foreach (var filter in resultFilter.ResultFilters)
			{
				ResultFilters.Add(filter);
			}
		}

		internal Func<T, bool> CompilePredicate()
		{
			if (ResultFilters.Count == 0)
			{
				return _defaultFilter;
			}

			return ResultFilters.GetOrCombined().Not().Compile();
		}
	}
}
