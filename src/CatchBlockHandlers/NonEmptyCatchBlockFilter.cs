using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	public sealed class NonEmptyCatchBlockFilter : CatchBlockFilter
	{
		private NonEmptyCatchBlockFilter(){}

		public static NonEmptyCatchBlockFilter CreateByIncluding<TException>(Func<TException, bool> func = null) where TException : Exception => new NonEmptyCatchBlockFilter().IncludeError(func);

		public static NonEmptyCatchBlockFilter CreateByIncluding(Expression<Func<Exception, bool>> handledErrorFilter) => new NonEmptyCatchBlockFilter().IncludeError(handledErrorFilter);

		public static NonEmptyCatchBlockFilter CreateByExcluding<TException>(Func<TException, bool> func = null) where TException : Exception => new NonEmptyCatchBlockFilter().ExcludeError(func);

		public static NonEmptyCatchBlockFilter CreateByExcluding(Expression<Func<Exception, bool>> handledErrorFilter) => new NonEmptyCatchBlockFilter().ExcludeError(handledErrorFilter);

		public new NonEmptyCatchBlockFilter ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public new NonEmptyCatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			ErrorFilter.AddExcludedErrorFilter(expression);
			return this;
		}

		public new NonEmptyCatchBlockFilter IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public new NonEmptyCatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression)
		{
			ErrorFilter.AddIncludedErrorFilter(expression);
			return this;
		}
	}
}
