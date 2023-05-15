using System.Collections;
using System.Collections.Generic;

namespace PoliNorError
{
	internal class FlexSyncEnumerable<T> : FlexSyncEnumerable<T>.IWrapper
	{
		private readonly IWrapper _inner;

		public FlexSyncEnumerable(bool forAsync = false)
		{
			_inner = !forAsync ? new ListWrapper() : (IWrapper)new SynchronizedListWrapper();
		}

		public void Add(T t)
		{
			_inner.Add(t);
		}

		public void AddRange(IEnumerable<T> collection)
		{
			_inner.AddRange(collection);
		}

		public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private sealed class ListWrapper : IWrapper
		{
			private readonly List<T> _inner;
			public ListWrapper()
			{
				_inner = new List<T>();
			}

			public void Add(T t) => _inner.Add(t);
			public void AddRange(IEnumerable<T> collection) => _inner.AddRange(collection);
			public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private sealed class SynchronizedListWrapper : IWrapper
		{
			private readonly SynchronizedList<T> _inner;
			public SynchronizedListWrapper()
			{
				_inner = new SynchronizedList<T>();
			}

			public void Add(T t) => _inner.Add(t);
			public void AddRange(IEnumerable<T> collection) => _inner.AddRange(collection);
			public IEnumerator<T> GetEnumerator() => ((IList<T>)_inner).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		internal interface IWrapper : ICanAddOneAndRange, IEnumerable<T> { }

		internal interface ICanAddOneAndRange
		{
			void AddRange(IEnumerable<T> collection);
			void Add(T t);
		}
	}
}
