using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
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
}
