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

		public IEnumerable<IPolicyBase> Policies => _syncInfos.GetPolicies();

		internal void ClearDelegates()
		{
			if (this.IsEmpty()) return;
			foreach (var polInfo in this)
			{
				polInfo.ClearDelegate();
			}
		}
	}
}
