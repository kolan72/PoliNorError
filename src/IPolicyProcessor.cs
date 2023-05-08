using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	public interface IPolicyProcessor : IEnumerable<IErrorProcessor>
	{
		void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter);
		void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter);
		void WithErrorProcessor(IErrorProcessor newErrorProcessor);

		IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters { get; }
		IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters { get; }
	}
}
