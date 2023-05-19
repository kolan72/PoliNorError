using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegate : PolicyDelegateBase
	{
		private SingleDelegateContainer _delegateContainer;

		internal PolicyDelegate(IPolicyBase policy) : base(policy){}

		internal void SetDelegate(Func<CancellationToken, Task> executeAsync)
		{
			_delegateContainer = SingleDelegateContainer.FromNotSync(executeAsync);
			DelegateContainer = _delegateContainer;
		}

		internal void SetDelegate(Action execute)
		{
			_delegateContainer = SingleDelegateContainer.FromSync(execute);
			DelegateContainer = _delegateContainer;
		}

		internal MethodInfo GetMethodInfo()
		{
			if (_delegateContainer?.UseSync == SyncPolicyDelegateType.None)
				return null;
			return _delegateContainer?.UseSync == SyncPolicyDelegateType.Sync ? Execute?.Method : ExecuteAsync?.Method;
		}

		public void ClearDelegate()
		{
			_delegateContainer?.ClearDelegate();
		}

		internal Func<CancellationToken, Task> ExecuteAsync => _delegateContainer?.ExecuteAsync;
		internal Action Execute =>  _delegateContainer?.Execute;

		protected override SyncPolicyDelegateType GetSyncType() => (_delegateContainer?.UseSync) ?? SyncPolicyDelegateType.None;
	}

	public sealed class PolicyDelegate<T> : PolicyDelegateBase
	{
		private SingleDelegateContainer<T> _delegateContainer;
		internal PolicyDelegate(IPolicyBase policy) : base(policy){}

		internal void SetDelegate(Func<CancellationToken, Task<T>> executeAsync)
		{
			_delegateContainer = SingleDelegateContainer<T>.FromNotSync(executeAsync);
			DelegateContainer = _delegateContainer;
		}

		internal void SetDelegate(Func<T> execute)
		{
			_delegateContainer = SingleDelegateContainer<T>.FromSync(execute);
			DelegateContainer = _delegateContainer;
		}

		internal MethodInfo GetMethodInfo()
		{
			if (_delegateContainer?.UseSync == SyncPolicyDelegateType.None)
				return null;
			return _delegateContainer?.UseSync == SyncPolicyDelegateType.Sync ? Execute?.Method : ExecuteAsync?.Method;
		}

		public void ClearDelegate()
		{
			_delegateContainer?.ClearDelegate();
		}

		internal Func<CancellationToken, Task<T>> ExecuteAsync => _delegateContainer?.ExecuteAsync;
		internal Func<T> Execute => _delegateContainer?.Execute;

		protected override SyncPolicyDelegateType GetSyncType() => (_delegateContainer?.UseSync) ?? SyncPolicyDelegateType.None;
	}

	public enum SyncPolicyDelegateType
	{
		None = 0,
		Sync,
		Async
	}
}
