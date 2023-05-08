using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	public static class ExpressionExtensions
	{
		public static Expression<Func<T, bool>> GetOrCombined<T>(this IEnumerable<Expression<Func<T, bool>>> expressCollection)
		{
			var res = PredicateBuilder.False<T>();
			foreach (var includedFilter in expressCollection)
			{
				res = res.Or(includedFilter);
			}
			return res;
		}
	}
}
