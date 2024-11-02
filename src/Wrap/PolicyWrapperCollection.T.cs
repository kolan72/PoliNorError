using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyWrapperCollection<T> : PolicyWrapper<T>
	{
		protected IEnumerable<IPolicyBase> _polices;

		private ThrowOnWrappedCollectionFailed _throwOnWrappedCollectionFailed = ThrowOnWrappedCollectionFailed.None;

		public PolicyWrapperCollection(IEnumerable<IPolicyBase> policies, Func<CancellationToken, Task<T>> func, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed, bool configureAwait)
			: base(func, token, configureAwait)
		{
			SetPoliciesParams(policies, throwOnWrappedCollectionFailed);
		}

		public PolicyWrapperCollection(IEnumerable<IPolicyBase> policies, Func<T> func, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
			: base(func, token)
		{
			SetPoliciesParams(policies, throwOnWrappedCollectionFailed);
		}

		private void SetPoliciesParams(IEnumerable<IPolicyBase> policies, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			_polices = policies;
			_throwOnWrappedCollectionFailed = throwOnWrappedCollectionFailed;
		}

		internal override T Handle()
		{
			var (HandleResults, lastPolicyResultState) = PolicyDelegatesHandler.HandleWhenAllReallySync(_polices.Select(pi => pi.ToPolicyDelegate(_func)), _token);

			_policyHandledResults.AddRange(HandleResults);

			if (lastPolicyResultState.IsFailed == true)
			{
				ThrowIfCollectionFailed(HandleResults);
			}
			return (HandleResults.LastOrDefault()?.Result).GetResultOrDefault();
		}

		internal override async Task<T> HandleAsync(CancellationToken token)
		{
			var polDelegates = _polices.Select(pi => pi.ToPolicyDelegate(_funcAsync));
			PolicyDelegateHandleType handleType = polDelegates.GetHandleType();
			var (HandleResults, lastPolicyResultState) = await PolicyDelegatesHandler.HandleAllBySyncType(polDelegates, handleType, token, _configureAwait).ConfigureAwait(_configureAwait);

			_policyHandledResults.AddRange(HandleResults);

			if (lastPolicyResultState.IsFailed == true)
			{
				ThrowIfCollectionFailed(HandleResults);
			}
			return (HandleResults.LastOrDefault()?.Result).GetResultOrDefault();
		}

		protected void ThrowIfCollectionFailed(IEnumerable<PolicyDelegateResult<T>> results)
		{
			if (_throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.LastError)
			{
				ThrowIfFailed(results.LastOrDefault()?.Result);
			}
			else if (results.LastOrDefault()?.Result.FailedReason != PolicyResultFailedReason.PolicyResultHandlerFailed)
			{
				throw new PolicyDelegateCollectionException<T>(results);
			}
			else
			{
				throw new PolicyResultHandlerFailedException<T>(results.LastOrDefault()?.Result);
			}
		}
	}
}
