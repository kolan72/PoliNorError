using System;

namespace PoliNorError
{
	public interface IWithInnerErrorFilter<T> where T : IWithInnerErrorFilter<T>
	{
		T IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception;
		T ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception;
	}
}
