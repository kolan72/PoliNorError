using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	///  Packs a <see cref="Func{CancellationToken, Task}"/> or <see cref="Action"/> delegate with a policy into a single class.
	/// </summary>
	public sealed class PolicyDelegate : PolicyDelegateBase
	{
		private SingleDelegateContainer _delegateContainer;

		internal PolicyDelegate(IPolicyBase policy) : base(policy){}

		/// <summary>
		/// Calls the <see cref="IPolicyBase.Handle"/> method of the policy this <see cref="PolicyDelegate"/> packs.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to cancel handling.</param>
		/// <returns></returns>
		public PolicyResult Handle(CancellationToken cancellationToken = default) => Policy.Handle(Execute, cancellationToken);

		/// <summary>
		/// Calls the <see cref="IPolicyBase.HandleAsync"/> method with configureAwait parameter equal to false of the policy that this <see cref="PolicyDelegate"/> packs.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to cancel handling.</param>
		/// <returns></returns>
		public Task<PolicyResult> HandleAsync(CancellationToken cancellationToken = default) => HandleAsync(false, cancellationToken);

		/// <summary>
		/// Calls the <see cref="IPolicyBase.HandleAsync"/> method for the policy that this <see cref="PolicyDelegate"/> packs.
		/// </summary>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="cancellationToken">A cancellation token to cancel handling.</param>
		/// <returns></returns>
		public Task<PolicyResult> HandleAsync(bool configureAwait, CancellationToken cancellationToken = default) => Policy.HandleAsync(ExecuteAsync, configureAwait, cancellationToken);

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
