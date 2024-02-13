using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal sealed class ErrorFilter : PolicyProcessor.ExceptionFilter
	{
		private ErrorFilter() { }

		public static ErrorFilter FromIncludedError<TException>(Func<TException, bool> func = null) where TException : Exception => new ErrorFilter().Include(func);

		public static ErrorFilter FromIncludedError(Expression<Func<Exception, bool>> handledErrorFilter) => new ErrorFilter().Include(handledErrorFilter);

		public static ErrorFilter FromExcludedError<TException>(Func<TException, bool> func = null) where TException : Exception => new ErrorFilter().Exclude(func);

		public static ErrorFilter FromExcludedError(Expression<Func<Exception, bool>> handledErrorFilter) => new ErrorFilter().Exclude(handledErrorFilter);

		public ErrorFilter Include(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public ErrorFilter Include<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}

		public ErrorFilter Exclude(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		public ErrorFilter Exclude<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			return this;
		}
	}
}
