using System.Collections;
using System.Collections.Generic;

namespace PoliNorError
{
	public abstract class PolicyDelegateCollectionBase<T> : IEnumerable<T> where T:  PolicyDelegateBase
	{
		protected readonly List<T> _syncInfos = new List<T>();
		protected bool _terminated;

		internal void AddPolicyDelegate(T errorPolicy)
		{
			this.ThrowIfInconsistency(errorPolicy);
			_syncInfos.Add(errorPolicy);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<T> GetEnumerator() => _syncInfos.GetEnumerator();

		public bool ThrowOnLastFailed => _terminated;

		public T LastPolicyDelegate => this.LastOrDefaultIfEmpty();

		public IEnumerable<IPolicyBase> Policies => _syncInfos.GetPolicies();
	}
}
