using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegate<T> : PolicyDelegateBase
	{
		private SingleDelegateContainer<T> _delegateContainer;
		internal PolicyDelegate(IPolicyBase policy) : base(policy) { }

		public PolicyResult<T> Handle(CancellationToken cancellationToken = default) => Policy.Handle(Execute, cancellationToken);

		public Task<PolicyResult<T>> HandleAsync(CancellationToken cancellationToken) => HandleAsync(false, cancellationToken);

		public Task<PolicyResult<T>> HandleAsync(bool configureAwait, CancellationToken cancellationToken) => Policy.HandleAsync(ExecuteAsync, configureAwait, cancellationToken);

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

		internal Func<CancellationToken, Task<T>> ExecuteAsync => _delegateContainer?.ExecuteAsync;
		internal Func<T> Execute => _delegateContainer?.Execute;

		protected override SyncPolicyDelegateType GetSyncType() => (_delegateContainer?.UseSync) ?? SyncPolicyDelegateType.None;
	}
}
