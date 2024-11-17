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
		private SingleDelegateContainer<T> _delegateContainer;
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
