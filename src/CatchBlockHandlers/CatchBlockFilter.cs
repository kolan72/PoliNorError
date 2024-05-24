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

		public CatchBlockFilter ExcludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			return ExcludeError<TException>(null, errorType);
		}

		public CatchBlockFilter ExcludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			switch (errorType)
			{
				case ErrorType.Error:
					return this.ExcludeError<CatchBlockFilter, TException>(func);
				case ErrorType.InnerError:
					return this.ExcludeInnerError(func);
				default:
					throw new NotImplementedException();
			}
		}

		public CatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.ExcludeError<CatchBlockFilter>(expression);
		}

		public CatchBlockFilter IncludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			return IncludeError<TException>(null, errorType);
		}

		public CatchBlockFilter IncludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
		{
			switch (errorType)
			{
				case ErrorType.Error:
					return this.IncludeError<CatchBlockFilter, TException>(func);
				case ErrorType.InnerError:
					return this.IncludeInnerError(func);
				default:
					throw new NotImplementedException();
			}
		}

		public CatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression)
		{
			return this.IncludeError<CatchBlockFilter>(expression);
		}

		public enum ErrorType
		{
			Error,
			InnerError
		}
	}
}
