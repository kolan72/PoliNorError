using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Represents a non-empty filter that has conditions for an exception to be handled.
	/// </summary>
	public sealed class NonEmptyCatchBlockFilter : CatchBlockFilter
	{
		private NonEmptyCatchBlockFilter(){}

		public static NonEmptyCatchBlockFilter CreateByIncluding<TException>(Func<TException, bool> func = null) where TException : Exception => new NonEmptyCatchBlockFilter().IncludeError(func);

		public static NonEmptyCatchBlockFilter CreateByIncluding(Expression<Func<Exception, bool>> handledErrorFilter) => new NonEmptyCatchBlockFilter().IncludeError(handledErrorFilter);

		public static NonEmptyCatchBlockFilter CreateByExcluding<TException>(Func<TException, bool> func = null) where TException : Exception => new NonEmptyCatchBlockFilter().ExcludeError(func);

		public static NonEmptyCatchBlockFilter CreateByExcluding(Expression<Func<Exception, bool>> handledErrorFilter) => new NonEmptyCatchBlockFilter().ExcludeError(handledErrorFilter);

		public new NonEmptyCatchBlockFilter ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			return this.ExcludeError<NonEmptyCatchBlockFilter, TException>(func);
		}

		public new NonEmptyCatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.ExcludeError<NonEmptyCatchBlockFilter>(expression);
		}

		public new NonEmptyCatchBlockFilter IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			return this.IncludeError<NonEmptyCatchBlockFilter, TException>(func);
		}

		public new NonEmptyCatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.IncludeError<NonEmptyCatchBlockFilter>(expression);
		}

		/// <summary>
		/// Creates the <see cref="CatchBlockFilteredHandler"/> handler with this filter.
		/// </summary>
		/// <returns></returns>
		public CatchBlockFilteredHandler ToCatchBlockHandler()
		{
			return new CatchBlockFilteredHandler(this);
		}
	}
}
