using System;

namespace PoliNorError
{
	/// <summary>
	///  Defines an interface for fluently adding a <see cref="NonEmptyCatchBlockFilter"/>.
	/// </summary>
	/// <typeparam name="T">A type implementing <see cref="ICanAddErrorFilter{T}"/>.</typeparam>
	public interface ICanAddErrorFilter<T> where T : ICanAddErrorFilter<T>
	{
		/// <summary>
		/// Adds a <see cref="NonEmptyCatchBlockFilter"/> filter.
		/// </summary>
		/// <param name="filter"><see cref="NonEmptyCatchBlockFilter"/></param>
		/// <returns></returns>
		T AddErrorFilter(NonEmptyCatchBlockFilter filter);

		/// <summary>
		/// Adds a <see cref="NonEmptyCatchBlockFilter"/> filter constructed using <paramref name="filterFactory"/>.
		/// </summary>
		/// <param name="filterFactory">Factory to create the <see cref="NonEmptyCatchBlockFilter"/>.</param>
		/// <returns></returns>
		T AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory);
	}

	/// <summary>
	/// Provides extension methods for the <see cref="ICanAddErrorFilter{T}"/> interface.
	/// </summary>
	public static class CanAddErrorFilterExtensions
	{
		/// <summary>
		/// Wraps a <see cref="PolicyProcessor.ExceptionFilter"/> into a <see cref="NonEmptyCatchBlockFilter"/>
		/// and adds it to the <see cref="ICanAddErrorFilter{T}"/>.
		/// </summary>
		/// <typeparam name="T">A type implementing <see cref="ICanAddErrorFilter{T}"/>.</typeparam>
		/// <param name="filter">The instance this method extends.</param>
		/// <param name="errorFilter">The exception filter logic to be applied.</param>
		public static void AddExceptionFilter<T>(this ICanAddErrorFilter<T> filter, PolicyProcessor.ExceptionFilter errorFilter) where T : ICanAddErrorFilter<T>
		{
			var neFilter = new NonEmptyCatchBlockFilter() { ErrorFilter = errorFilter };
			filter.AddErrorFilter(neFilter);
		}
	}
}
