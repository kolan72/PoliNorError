using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Represents a filter that has conditions for an exception to be handled.
	/// </summary>
	public class CatchBlockFilter
	{
		public static CatchBlockFilter Empty() => new CatchBlockFilter();

		internal PolicyProcessor.ExceptionFilter ErrorFilter { get; } = new PolicyProcessor.ExceptionFilter();

		public CatchBlockFilter ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			return this.ExcludeError<CatchBlockFilter, TException>(func);
		}

		public CatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.ExcludeError<CatchBlockFilter>(expression);
		}

		public CatchBlockFilter IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			return this.IncludeError<CatchBlockFilter, TException>(func);
		}

		public CatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.IncludeError<CatchBlockFilter>(expression);
		}
	}
}
