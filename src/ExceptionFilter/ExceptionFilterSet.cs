using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal class ExceptionFilterSet
	{
		internal List<Expression<Func<Exception, bool>>> IncludedErrorFilters { get; } = new List<Expression<Func<Exception, bool>>>();

		internal List<Expression<Func<Exception, bool>>> ExcludedErrorFilters { get; } = new List<Expression<Func<Exception, bool>>>();

		internal void IncludeFilter(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			IncludedErrorFilters.Add(handledErrorFilter);
		}

		internal void ExcludeFilter(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			ExcludedErrorFilters.Add(handledErrorFilter);
		}

		internal void AppendFilter(ExceptionFilterSet exceptionFilter)
		{
			foreach (var filter in exceptionFilter.IncludedErrorFilters)
			{
				IncludedErrorFilters.Add(filter);
			}
			foreach (var filter in exceptionFilter.ExcludedErrorFilters)
			{
				ExcludedErrorFilters.Add(filter);
			}
		}

		internal Func<Exception, bool> CompilePredicate()
		{
			var (includeMode, includeExpression) = GetIncludedErrorFilterPredicateTuple();
			var (excludeMode, excludeExpression) = GetExcludedErrorFilterPredicateTuple();

			var resMode = includeMode | excludeMode;

			if ((resMode & ErrorFilterModes.Include) != 0)
			{
				return (resMode & ErrorFilterModes.Exclude) != 0 ? includeExpression.And(excludeExpression).Compile() : includeExpression.Compile();
			}
			else if ((resMode & ErrorFilterModes.Exclude) != 0)
			{
				return excludeExpression.Compile();
			}
			else
			{
				return (_) => true;
			}
		}

		private (ErrorFilterModes mode, Expression<Func<Exception, bool>> expression) GetIncludedErrorFilterPredicateTuple()
		{
			if (IncludedErrorFilters.Count == 0)
				return (ErrorFilterModes.None, null);

			return (ErrorFilterModes.Include, IncludedErrorFilters.GetOrCombined());
		}

		private (ErrorFilterModes mode, Expression<Func<Exception, bool>> expression) GetExcludedErrorFilterPredicateTuple()
		{
			if (ExcludedErrorFilters.Count == 0)
				return (ErrorFilterModes.None, null);

			var res = ExcludedErrorFilters.GetOrCombined().Not();

			return (ErrorFilterModes.Exclude, res);
		}

		[Flags]
		private enum ErrorFilterModes
		{
			None = 0,
			Include = 1,
			Exclude = 2,
			IncludeExclude = Include | Exclude
		}
	}
}
