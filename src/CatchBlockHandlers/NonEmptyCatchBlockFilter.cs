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

		public static NonEmptyCatchBlockFilter CreateByIncluding(IErrorSet errorSet)
		{
			var filter = new NonEmptyCatchBlockFilter();
			foreach (var item in errorSet.Items)
			{
				filter.ErrorFilter.AddIncludedError(item);
			}
			return filter;
		}

		public static NonEmptyCatchBlockFilter CreateByIncluding<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception => CreateByIncluding<TException>(null, errorType);

		public static NonEmptyCatchBlockFilter CreateByIncluding<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception => new NonEmptyCatchBlockFilter().IncludeError(func, errorType);

		public static NonEmptyCatchBlockFilter CreateByIncluding(Expression<Func<Exception, bool>> handledErrorFilter) => new NonEmptyCatchBlockFilter().IncludeError(handledErrorFilter);

		public static NonEmptyCatchBlockFilter CreateByExcluding<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception => CreateByExcluding<TException>(null, errorType);

		public static NonEmptyCatchBlockFilter CreateByExcluding<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception => new NonEmptyCatchBlockFilter().ExcludeError(func, errorType);

		public static NonEmptyCatchBlockFilter CreateByExcluding(Expression<Func<Exception, bool>> handledErrorFilter) => new NonEmptyCatchBlockFilter().ExcludeError(handledErrorFilter);

		public new NonEmptyCatchBlockFilter ExcludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			return ExcludeError<TException>(null, errorType);
		}

		public new NonEmptyCatchBlockFilter ExcludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			switch (errorType)
			{
				case ErrorType.Error:
					return this.ExcludeError<NonEmptyCatchBlockFilter, TException>(func);
				case ErrorType.InnerError:
					return this.ExcludeInnerError(func);
				default:
					throw new NotImplementedException();
			}
		}

		public new NonEmptyCatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.ExcludeError<NonEmptyCatchBlockFilter>(expression);
		}

		public new NonEmptyCatchBlockFilter IncludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			return IncludeError<TException>(null, errorType);
		}

		public new NonEmptyCatchBlockFilter IncludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			switch (errorType)
			{
				case ErrorType.Error:
					return this.IncludeError<NonEmptyCatchBlockFilter, TException>(func);
				case ErrorType.InnerError:
					return this.IncludeInnerError(func);
				default:
					throw new NotImplementedException();
			}
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
