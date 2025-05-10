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
}
