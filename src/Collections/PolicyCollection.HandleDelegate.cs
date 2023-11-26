using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class PolicyCollection
	{
		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler"/> handler from a  <see cref="IPolicyDelegateCollection"/> with a common delegate <paramref name="action"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler.Handle"/> method.
		/// </summary>
		/// <param name="action">The delegate to handle</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public PolicyDelegateCollectionResult HandleDelegate(Action action, CancellationToken token = default)
		{
			if (action == null)
			{
				return GetEmptyFailedPolicyDelegateCollectionResult(action);
			}
			return BuildCollectionHandlerFor(action).Handle(token);
		}
		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler"/> handler from a  <see cref="IPolicyDelegateCollection"/> with a common delegate <paramref name="func"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler.Handle"/> method.
		/// </summary>
		/// <param name="func">The delegate to handle</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public PolicyDelegateCollectionResult HandleDelegate(Func<CancellationToken, Task> func, CancellationToken token = default)
		{
			if (func == null)
			{
				return GetEmptyFailedPolicyDelegateCollectionResult(func);
			}
			return  BuildCollectionHandlerFor(func).Handle(token);
		}

		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler{T}"/> handler from a  <see cref="IPolicyDelegateCollection{T}"/> with a common delegate <paramref name="func"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler{T}.Handle"/> method.
		/// </summary>
		/// <typeparam name="T">The type of the result</typeparam>
		/// <param name="func">The delegate to handle</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public PolicyDelegateCollectionResult<T> HandleDelegate<T>(Func<T> func, CancellationToken token = default)
		{
			if (func == null)
			{
				return GetEmptyFailedPolicyDelegateCollectionResult(func);
			}
			return BuildCollectionHandlerFor(func).Handle(token);
		}
		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler{T}"/> handler from a  <see cref="IPolicyDelegateCollection{T}"/> with a common delegate <paramref name="func"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler{T}.Handle"/> method.
		///</summary>
		/// <typeparam name="T">The type of the result</typeparam>
		/// <param name="func">The delegate to handle</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public PolicyDelegateCollectionResult<T> HandleDelegate<T>(Func<CancellationToken, Task<T>> func, CancellationToken token = default)
		{
			if (func == null)
			{
				return GetEmptyFailedPolicyDelegateCollectionResult(func);
			}
			return BuildCollectionHandlerFor(func).Handle(token);
		}

		/// <inheritdoc 
		/// cref="HandleDelegateAsync(Action, bool, CancellationToken)"/>
		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Action action, CancellationToken token = default) => HandleDelegateAsync(action, false, token);

		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler"/> handler from a  <see cref="IPolicyDelegateCollection"/> with a common delegate <paramref name="action"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler.HandleAsync"/> method.
		/// </summary>
		/// <param name="action">The delegate to handle async</param>
		/// <param name="configAwait">A continueOnCapturedContext parameter for awaiter</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Action action, bool configAwait, CancellationToken token = default)
		{
			if (action == null)
			{
				return Task.FromResult(GetEmptyFailedPolicyDelegateCollectionResult(action));
			}
			return BuildCollectionHandlerFor(action).HandleAsync(configAwait, token);
		}

		/// <inheritdoc 
		/// cref="HandleDelegateAsync(Func{CancellationToken, Task} , bool, CancellationToken)"/>
		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Func<CancellationToken, Task> func, CancellationToken token = default) => HandleDelegateAsync(func, false, token);

		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler"/> handler from a  <see cref="IPolicyDelegateCollection"/> with a common delegate <paramref name="func"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler.HandleAsync"/> method.
		/// </summary>
		/// <param name="func">The delegate to handle async</param>
		/// <param name="configAwait">A continueOnCapturedContext parameter for awaiter</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Func<CancellationToken, Task> func, bool configAwait, CancellationToken token = default)
		{
			if (func == null)
			{
				return Task.FromResult(GetEmptyFailedPolicyDelegateCollectionResult(func));
			}
			return BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		/// <inheritdoc 
		/// cref="HandleDelegateAsync{T}(Func{T}, bool, CancellationToken)"/>
		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<T> func, CancellationToken token = default) => HandleDelegateAsync(func, false, token);

		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler{T}"/> handler from a  <see cref="IPolicyDelegateCollection{T}"/> with a common delegate <paramref name="func"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler{T}.HandleAsync"/> method.
		/// </summary>
		/// <typeparam name="T">The type of the result</typeparam>
		/// <param name="func">The delegate to handle async</param>
		/// <param name="configAwait">A continueOnCapturedContext parameter for awaiter</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<T> func, bool configAwait, CancellationToken token = default)
		{
			if (func == null)
			{
				return Task.FromResult(GetEmptyFailedPolicyDelegateCollectionResult(func));
			}
			return BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		/// <inheritdoc 
		/// cref="HandleDelegateAsync{T}(Func{CancellationToken, Task{T}}, bool, CancellationToken)"/>
		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken token = default) => HandleDelegateAsync(func, false, token);

		/// <summary>
		/// Handles a delegate by creating a <see cref ="IPolicyDelegateCollectionHandler{T}"/> handler from a  <see cref="IPolicyDelegateCollection{T}"/> with a common delegate <paramref name="func"/>
		/// and calling  the <see cref ="IPolicyDelegateCollectionHandler{T}.HandleAsync"/> method.
		/// </summary>
		/// <typeparam name="T">The type of the result</typeparam>
		/// <param name="func">The delegate to handle async</param>
		/// <param name="configAwait">A continueOnCapturedContext parameter for awaiter</param>
		/// <param name="token">A cancellation token to cancel handling</param>
		/// <returns></returns>
		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<CancellationToken, Task<T>> func, bool configAwait, CancellationToken token = default)
		{
			if (func == null)
			{
				return Task.FromResult(GetEmptyFailedPolicyDelegateCollectionResult(func));
			}
			return BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		private PolicyDelegateCollectionResult GetEmptyFailedPolicyDelegateCollectionResult(Action action) => new PolicyDelegateCollectionResult(new List<PolicyDelegateResult>(), ToPolicyDelegateCollection(action), LastPolicyResultState.FromFailed(), PolicyResultFailedReason.DelegateIsNull);

		private PolicyDelegateCollectionResult GetEmptyFailedPolicyDelegateCollectionResult(Func<CancellationToken, Task> func) => new PolicyDelegateCollectionResult(new List<PolicyDelegateResult>(), ToPolicyDelegateCollection(func), LastPolicyResultState.FromFailed(), PolicyResultFailedReason.DelegateIsNull);

		private PolicyDelegateCollectionResult<T> GetEmptyFailedPolicyDelegateCollectionResult<T>(Func<T> func) => new PolicyDelegateCollectionResult<T>(new List<PolicyDelegateResult<T>>(), ToPolicyDelegateCollection(func), LastPolicyResultState.FromFailed(), PolicyResultFailedReason.DelegateIsNull);

		private PolicyDelegateCollectionResult<T> GetEmptyFailedPolicyDelegateCollectionResult<T>(Func<CancellationToken, Task<T>> func) => new PolicyDelegateCollectionResult<T>(new List<PolicyDelegateResult<T>>(), ToPolicyDelegateCollection(func), LastPolicyResultState.FromFailed(), PolicyResultFailedReason.DelegateIsNull);
	}
}
