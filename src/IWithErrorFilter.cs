using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Defines methods to include or exclude specific exception types in an error filter.
	/// </summary>
	/// <typeparam name="T">The implementing type, used for fluent chaining.</typeparam>
	public interface IWithErrorFilter<T> where T : IWithErrorFilter<T>
	{
		/// <summary>
		/// Includes the specified exception type in the error filter.
		/// </summary>
		/// <typeparam name="TException">The type of exception to include.</typeparam>
		/// <param name="func">An optional predicate to further filter the exception.</param>
		/// <returns>The current instance for fluent chaining.</returns>
		T IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception;

		/// <summary>
		/// Includes exceptions matching the specified expression in the error filter.
		/// </summary>
		/// <param name="expression">An expression that defines which exceptions to include.</param>
		/// <returns>The current instance for fluent chaining.</returns>
		T IncludeError(Expression<Func<Exception, bool>> expression);

		/// <summary>
		/// Excludes the specified exception type from the error filter.
		/// </summary>
		/// <typeparam name="TException">The type of exception to exclude.</typeparam>
		/// <param name="func">An optional predicate to further filter the exception.</param>
		/// <returns>The current instance for fluent chaining.</returns>
		T ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception;

		/// <summary>
		/// Excludes exceptions matching the specified expression from the error filter.
		/// </summary>
		/// <param name="expression">An expression that defines which exceptions to exclude.</param>
		/// <returns>The current instance for fluent chaining.</returns>
		T ExcludeError(Expression<Func<Exception, bool>> expression);
	}
}
