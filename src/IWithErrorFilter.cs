using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	public interface IWithErrorFilter<T> where T : IWithErrorFilter<T>
	{
		T IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception;
		T IncludeError(Expression<Func<Exception, bool>> expression);
		T ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception;
		T ExcludeError(Expression<Func<Exception, bool>> expression);
	}
}
