using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// A non-generic result filter that can hold result filters for more than one type.
	/// Each distinct result type owns its own <see cref="ResultFilter{T}"/> stored in an inner
	/// store keyed by <see cref="Type"/>.
	/// </summary>
	public class ResultsFilter
	{
		private readonly Dictionary<Type, IResultFilterStoreEntry> _store = new Dictionary<Type, IResultFilterStoreEntry>();

		/// <summary>
		/// Adds (or appends to) an excluded-result filter for the result type <typeparamref name="T"/>.
		/// A per-type <see cref="ResultFilter{T}"/> is created lazily on first use.
		/// </summary>
		public ResultsFilter ExcludeResult<T>(Expression<Func<T, bool>> expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			GetOrCreateEntry<T>().ExcludeResult(expression);
			return this;
		}

		/// <summary>
		/// Returns a predicate that reports whether a result of type <typeparamref name="T"/> is considered
		/// successful (i.e. not excluded by any registered filter for that type). If no filter has been
		/// registered for <typeparamref name="T"/>, an always-<c>true</c> predicate is returned.
		/// </summary>
		public Func<T, bool> GetIsSuccessful<T>()
		{
			return GetOrCreateEntry<T>().GetIsSuccessful();
		}

		/// <summary>
		/// Merges the per-type filters from <paramref name="other"/> into this filter.
		/// </summary>
		public void AppendFilter(ResultsFilter other)
		{
			if (other == null)
			{
				throw new ArgumentNullException(nameof(other));
			}

			foreach (var pair in other._store)
			{
				GetOrCreateEntry(pair.Key).AppendFilter(pair.Value);
			}
		}

		internal ResultsFilterSlim GetSlim()
		{
			return new ResultsFilterSlim(this);
		}

		internal Dictionary<Type, IResultFilterStoreEntry> Store => _store;

		private ResultFilterStoreEntry<T> GetOrCreateEntry<T>()
		{
			return (ResultFilterStoreEntry<T>)GetOrCreateEntry(typeof(T));
		}

		private IResultFilterStoreEntry GetOrCreateEntry(Type type)
		{
			if (!_store.TryGetValue(type, out var entry))
			{
				entry = CreateEntry(type);
				_store[type] = entry;
			}

			return entry;
		}

		private static IResultFilterStoreEntry CreateEntry(Type type)
		{
			var entryType = typeof(ResultFilterStoreEntry<>).MakeGenericType(type);
			return (IResultFilterStoreEntry)Activator.CreateInstance(entryType);
		}

		internal interface IResultFilterStoreEntry
		{
			void AppendFilter(IResultFilterStoreEntry other);
		}

		private sealed class ResultFilterStoreEntry<T> : IResultFilterStoreEntry
		{
			private readonly ResultFilter<T> _filter = new ResultFilter<T>();

			public void ExcludeResult(Expression<Func<T, bool>> expression)
			{
				_filter.ExcludeResult(expression);
			}

			public Func<T, bool> GetIsSuccessful()
			{
				return _filter.GetIsSuccessful();
			}

			public void AppendFilter(IResultFilterStoreEntry other)
			{
				if (other is ResultFilterStoreEntry<T> typed)
				{
					_filter.AppendFilter(typed._filter);
				}
			}
		}
	}
}