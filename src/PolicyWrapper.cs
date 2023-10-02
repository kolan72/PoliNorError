using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyWrapperSingle : PolicyWrapper
	{
		internal PolicyWrapperSingle(IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait): base(policyBase, func, token, configureAwait)
		{}

		internal PolicyWrapperSingle(IPolicyBase policyBase, Action action, CancellationToken token) : base(policyBase, action, token)
		{}

		internal override async Task  HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_func, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyDelegateResult(res, _policyBase.PolicyName, _func.Method));

			ThrowIfFailed(res);
		}

		internal override void Handle()
		{
			var res = _policyBase.Handle(_action, _token);

			_policyHandledResults.Add(new PolicyDelegateResult(res, _policyBase.PolicyName, _action.Method));

			ThrowIfFailed(res);
		}
	}

	internal class PolicyWrapperCollection : PolicyWrapper
	{
		public PolicyWrapperCollection(IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait) : base(policyBase, func, token, configureAwait)
		{}

		public PolicyWrapperCollection(IPolicyBase policyBase, Action action, CancellationToken token) : base(policyBase, action, token)
		{ }

		internal override void Handle()
		{
			var (HandleResults, lastPolicyResultState) = PolicyDelegatesHandler.HandleWhenAllReallySync(_polices.Select(pi => pi.ToPolicyDelegate(_action)), _token);

			_policyHandledResults.AddRange(HandleResults);

			if (lastPolicyResultState.IsFailed == true)
			{
				ThrowIfCollectionFailed(HandleResults);
			}
		}

		internal override async Task HandleAsync(CancellationToken token)
		{
			var polDelegates = _polices.Select(pi => pi.ToPolicyDelegate(_action));
			PolicyDelegateHandleType handleType = polDelegates.GetHandleType();
			var (HandleResults, lastPolicyResultState) = await PolicyDelegatesHandler.HandleAllBySyncType(polDelegates, handleType, token, _configureAwait).ConfigureAwait(_configureAwait);
			if (lastPolicyResultState.IsFailed == true)
			{
				ThrowIfCollectionFailed(HandleResults);
			}
		}
	}

	internal  abstract class PolicyWrapper : PolicyWrapperBase
	{
		protected readonly Func<CancellationToken, Task> _func;
		protected readonly Action _action;
		protected readonly FlexSyncEnumerable<PolicyDelegateResult>.IWrapper _policyHandledResults;

		private protected PolicyWrapper(IEnumerable<IPolicyBase> policies, Func<CancellationToken, Task> func, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed, bool configureAwait) : base(policies, token, throwOnWrappedCollectionFailed, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>(!_configureAwait);
			_func = func;
		}

		private protected PolicyWrapper(IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait) : base(policyBase, token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>(!_configureAwait);
			_func = func;
		}

		private protected PolicyWrapper(IEnumerable<IPolicyBase> policies, Action action, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed) : base(policies, token, throwOnWrappedCollectionFailed)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			_action = action;
		}

		private protected PolicyWrapper(IPolicyBase policyBase, Action action, CancellationToken token) : base(policyBase, token)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			_action = action;
		}

		internal abstract Task HandleAsync(CancellationToken token);

		internal abstract void Handle();

		internal IEnumerable<PolicyDelegateResult> PolicyDelegateResults
		{
			get { return _policyHandledResults; }
		}
	}
}
