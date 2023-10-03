using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyWrapperSingle : PolicyWrapper
	{
		protected IPolicyBase _policyBase;
		internal PolicyWrapperSingle(IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait)
			: base(func, token, configureAwait)
		{
			SetPolicyParam(policyBase);
		}

		internal PolicyWrapperSingle(IPolicyBase policyBase, Action action, CancellationToken token)
			: base(action, token)
		{
			SetPolicyParam(policyBase);
		}

		private void SetPolicyParam(IPolicyBase policyBase)
		{
			_policyBase = policyBase;
		}

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
		protected IEnumerable<IPolicyBase> _polices;

		private ThrowOnWrappedCollectionFailed _throwOnWrappedCollectionFailed = ThrowOnWrappedCollectionFailed.None;

		public PolicyWrapperCollection(IEnumerable<IPolicyBase> policies, Func<CancellationToken, Task> func, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed, bool configureAwait)
			: base(func, token, configureAwait)
		{
			SetPoliciesParams(policies, throwOnWrappedCollectionFailed);
		}

		public PolicyWrapperCollection(IEnumerable<IPolicyBase> policies, Action action, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
			: base(action, token)
		{
			SetPoliciesParams(policies, throwOnWrappedCollectionFailed);
		}

		private void SetPoliciesParams(IEnumerable<IPolicyBase> policies, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			_polices = policies;
			_throwOnWrappedCollectionFailed = throwOnWrappedCollectionFailed;
		}

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
			var polDelegates = _polices.Select(pi => pi.ToPolicyDelegate(_func));
			PolicyDelegateHandleType handleType = polDelegates.GetHandleType();
			var (HandleResults, lastPolicyResultState) = await PolicyDelegatesHandler.HandleAllBySyncType(polDelegates, handleType, token, _configureAwait).ConfigureAwait(_configureAwait);
			if (lastPolicyResultState.IsFailed == true)
			{
				ThrowIfCollectionFailed(HandleResults);
			}
		}

		protected void ThrowIfCollectionFailed(IEnumerable<PolicyDelegateResult> results)
		{
			if (_throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.LastError)
			{
				ThrowIfFailed(results.LastOrDefault()?.Result);
			}
			else if (results.LastOrDefault()?.Result.FailedReason != PolicyResultFailedReason.PolicyResultHandlerFailed)
			{
				throw new PolicyDelegateCollectionException(results);
			}
			else
			{
				throw new PolicyResultHandlerFailedException();
			}
		}
	}

	internal  abstract class PolicyWrapper : PolicyWrapperBase
	{
		protected readonly Func<CancellationToken, Task> _func;
		protected readonly Action _action;
		protected readonly FlexSyncEnumerable<PolicyDelegateResult>.IWrapper _policyHandledResults;

		private protected PolicyWrapper(Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait) : base(token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>(!_configureAwait);
			_func = func;
		}

		private protected PolicyWrapper(Action action, CancellationToken token) : base(token)
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
