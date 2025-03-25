using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal class EmptyCatchBlockFilter : IEmptyCatchBlockFilter
	{
		private readonly NonEmptyCatchBlockFilter _innerFilter;
		internal EmptyCatchBlockFilter()
		{
			_innerFilter = new NonEmptyCatchBlockFilter();
		}

		public NonEmptyCatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression) => _innerFilter.ExcludeError(expression);

		public NonEmptyCatchBlockFilter ExcludeError<TException>(CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception
		{
			return _innerFilter.ExcludeError<TException>(errorType);
		}

		public NonEmptyCatchBlockFilter ExcludeError<TException>(Func<TException, bool> func, CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception
		{
			return _innerFilter.ExcludeError(func, errorType);
		}

		public NonEmptyCatchBlockFilter ExcludeErrorSet(IErrorSet errorSet) => _innerFilter.ExcludeErrorSet(errorSet);

		public NonEmptyCatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression) => _innerFilter.IncludeError(expression);

		public NonEmptyCatchBlockFilter IncludeError<TException>(CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception
		{
			return _innerFilter.IncludeError<TException>(errorType);
		}

		public NonEmptyCatchBlockFilter IncludeError<TException>(Func<TException, bool> func, CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception
		{
			return _innerFilter.IncludeError(func, errorType);
		}

		public NonEmptyCatchBlockFilter IncludeErrorSet(IErrorSet errorSet) => _innerFilter.IncludeErrorSet(errorSet);
	}

	public interface IEmptyCatchBlockFilter
	{
		NonEmptyCatchBlockFilter ExcludeError(Expression<Func<Exception, bool>> expression);
		NonEmptyCatchBlockFilter ExcludeError<TException>(CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception;
		NonEmptyCatchBlockFilter ExcludeError<TException>(Func<TException, bool> func, CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception;
		NonEmptyCatchBlockFilter ExcludeErrorSet(IErrorSet errorSet);
		NonEmptyCatchBlockFilter IncludeError(Expression<Func<Exception, bool>> expression);
		NonEmptyCatchBlockFilter IncludeError<TException>(CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception;
		NonEmptyCatchBlockFilter IncludeError<TException>(Func<TException, bool> func, CatchBlockFilter.ErrorType errorType = CatchBlockFilter.ErrorType.Error) where TException : Exception;
		NonEmptyCatchBlockFilter IncludeErrorSet(IErrorSet errorSet);
	}
}
