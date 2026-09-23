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
			DelegateContainer = SingleDelegateContainer.FromNotSync(executeAsync);
		}

		internal void SetDelegate(Action execute)
		{
			DelegateContainer = SingleDelegateContainer.FromSync(execute);
		}

		internal MethodInfo GetMethodInfo()
		{
			var container = TypedContainer;
			if (container?.UseSync == SyncPolicyDelegateType.None)
				return null;
			return container?.UseSync == SyncPolicyDelegateType.Sync
				? container.Execute?.Method
				: container?.ExecuteAsync?.Method;
		}

		private SingleDelegateContainer TypedContainer => (SingleDelegateContainer)DelegateContainer;

		internal Func<CancellationToken, Task> ExecuteAsync => TypedContainer?.ExecuteAsync;
		internal Action Execute => TypedContainer?.Execute;

		protected override SyncPolicyDelegateType GetSyncType() => (TypedContainer?.UseSync) ?? SyncPolicyDelegateType.None;
	}
}
