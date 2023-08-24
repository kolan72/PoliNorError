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

		public PolicyResult Handle(CancellationToken cancellationToken = default) => Policy.Handle(Execute, cancellationToken);

		public Task<PolicyResult> HandleAsync(CancellationToken cancellationToken) => HandleAsync(false, cancellationToken);

		public Task<PolicyResult> HandleAsync(bool configureAwait, CancellationToken cancellationToken) => Policy.HandleAsync(ExecuteAsync, configureAwait, cancellationToken);

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

		internal Func<CancellationToken, Task> ExecuteAsync => _delegateContainer?.ExecuteAsync;
		internal Action Execute =>  _delegateContainer?.Execute;

		protected override SyncPolicyDelegateType GetSyncType() => (_delegateContainer?.UseSync) ?? SyncPolicyDelegateType.None;
	}
}
