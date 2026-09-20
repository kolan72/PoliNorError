using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Packs a Func&lt;CancellationToken, Task&lt;T&gt;&gt; or <see cref="Func{T}"/> delegate with a policy into a single class.
	/// </summary>
	/// <typeparam name="T">Type that the delegate returns.</typeparam>
	public sealed class PolicyDelegate<T> : PolicyDelegateBase
	{
		internal PolicyDelegate(IPolicyBase policy) : base(policy) { }

		/// <summary>
		/// Calls the <see cref="IPolicyBase.Handle{T}"/> method of the policy this <see cref="PolicyDelegate{T}"/> packs.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to cancel handling.</param>
		/// <returns></returns>
		public PolicyResult<T> Handle(CancellationToken cancellationToken = default) => Policy.Handle(Execute, cancellationToken);

		/// <summary>
		/// Calls the <see cref="IPolicyBase.HandleAsync{T}"/> method with the configureAwait parameter set to false for the policy that this <see cref="PolicyDelegate{T}"/> packs.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to cancel handling.</param>
		/// <returns></returns>
		public Task<PolicyResult<T>> HandleAsync(CancellationToken cancellationToken = default) => HandleAsync(false, cancellationToken);

		/// <summary>
		/// Calls the <see cref="IPolicyBase.HandleAsync{T}"/> method for the policy this <see cref="PolicyDelegate{T}"/> packs.
		/// </summary>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="cancellationToken">A cancellation token to cancel handling.</param>
		/// <returns></returns>
		public Task<PolicyResult<T>> HandleAsync(bool configureAwait, CancellationToken cancellationToken = default) => Policy.HandleAsync(ExecuteAsync, configureAwait, cancellationToken);

		internal void SetDelegate(Func<CancellationToken, Task<T>> executeAsync)
		{
			DelegateContainer = SingleDelegateContainer<T>.FromNotSync(executeAsync);
		}

		internal void SetDelegate(Func<T> execute)
		{
			DelegateContainer = SingleDelegateContainer<T>.FromSync(execute);
		}

		internal MethodInfo GetMethodInfo()
		{
			var container = TypedContainer;
			if (container?.UseSync == SyncPolicyDelegateType.None)
				return null;
			return container.UseSync == SyncPolicyDelegateType.Sync
				? container.Execute?.Method
				: container.ExecuteAsync?.Method;
		}

		private SingleDelegateContainer<T> TypedContainer => (SingleDelegateContainer<T>)DelegateContainer;

		internal Func<CancellationToken, Task<T>> ExecuteAsync => TypedContainer?.ExecuteAsync;
		internal Func<T> Execute => TypedContainer?.Execute;

		protected override SyncPolicyDelegateType GetSyncType() => (TypedContainer?.UseSync) ?? SyncPolicyDelegateType.None;
	}
}
