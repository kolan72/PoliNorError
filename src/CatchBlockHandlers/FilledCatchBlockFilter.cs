using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	public sealed class FilledCatchBlockFilter : CatchBlockFilter
	{
		private FilledCatchBlockFilter(){}

		public static FilledCatchBlockFilter CreateByIncluding<TException>(Func<TException, bool> func = null) where TException : Exception => new FilledCatchBlockFilter().IncludeError(func);

		public static FilledCatchBlockFilter CreateByIncluding(Expression<Func<Exception, bool>> handledErrorFilter) => new FilledCatchBlockFilter().IncludeError(handledErrorFilter);

		public static FilledCatchBlockFilter CreateByExcluding<TException>(Func<TException, bool> func = null) where TException : Exception => new FilledCatchBlockFilter().ExcludeError(func);

		public static FilledCatchBlockFilter CreateByExcluding(Expression<Func<Exception, bool>> handledErrorFilter) => new FilledCatchBlockFilter().ExcludeError(handledErrorFilter);

		public new FilledCatchBlockFilter ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public new FilledCatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			ErrorFilter.AddExcludedErrorFilter(expression);
			return this;
		}

		public new FilledCatchBlockFilter IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public new FilledCatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression)
		{
			ErrorFilter.AddIncludedErrorFilter(expression);
			return this;
		}
	}
}
