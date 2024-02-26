using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Represents a filter that has conditions for an exception to be handled.
	/// </summary>
	public sealed class CatchBlockFilter
	{
		internal PolicyProcessor.ExceptionFilter ErrorFilter { get; } = new PolicyProcessor.ExceptionFilter();

		public CatchBlockFilter ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public CatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			ErrorFilter.AddExcludedErrorFilter(expression);
			return this;
		}

		public CatchBlockFilter IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public CatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression)
		{
			ErrorFilter.AddIncludedErrorFilter(expression);
			return this;
		}
	}
}
