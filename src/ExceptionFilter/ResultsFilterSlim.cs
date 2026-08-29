using System;

namespace PoliNorError
{
	/// <summary>
	/// A compiled, not-unchangeable view of a <see cref="ResultsFilter"/> that exposes a per-type
	/// success predicate without exposing the underlying filter expressions.
	/// </summary>
	internal class ResultsFilterSlim
	{
		private readonly ResultsFilter _resultsFilter;

		internal ResultsFilterSlim(ResultsFilter resultsFilter)
		{
			_resultsFilter = resultsFilter;
		}

		/// <summary>
		/// Reports whether the given <paramref name="value"/> of type <typeparamref name="T"/> is considered
		/// successful (i.e. not excluded by any filter registered for that type).
		/// </summary>
		public bool IsSuccessful<T>(T value)
		{
			return _resultsFilter.GetIsSuccessful<T>()(value);
		}
	}
}