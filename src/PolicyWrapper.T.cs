using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyWrapperSingle<T> : PolicyWrapper<T>
	{
		protected IPolicyBase _policyBase;
		internal PolicyWrapperSingle(IPolicyBase policyBase, Func<CancellationToken, Task<T>> func, CancellationToken token, bool configureAwait)
			: base(func, token, configureAwait)
		{
			SetPolicyParam(policyBase);
		}

		internal PolicyWrapperSingle(IPolicyBase policyBase, Func<T> func, CancellationToken token)
			: base(func, token)
		{
			SetPolicyParam(policyBase);
		}

		private void SetPolicyParam(IPolicyBase policyBase)
		{
			_policyBase = policyBase;
		}

		internal override async Task<T> HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_funcAsync, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyDelegateResult<T>(res, _policyBase.PolicyName, _funcAsync.Method));

			ThrowIfFailed(res);

			return res.Result;
		}

		internal override T Handle()
		{
			var res = _policyBase.Handle(_func, _token);

			_policyHandledResults.Add(new PolicyDelegateResult<T>(res, _policyBase.PolicyName, _func.Method));

			ThrowIfFailed(res);

			return res.Result;
		}
	}

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
				throw new PolicyResultHandlerFailedException();
			}
		}
	}

	internal abstract class PolicyWrapper<T> : PolicyWrapperBase
	{
		protected readonly Func<CancellationToken, Task<T>> _funcAsync;
		protected readonly Func<T> _func;

		protected readonly FlexSyncEnumerable<PolicyDelegateResult<T>> _policyHandledResults;

		private protected PolicyWrapper(Func<CancellationToken, Task<T>> funcAsync, CancellationToken token, bool configureAwait) : base(token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>(!configureAwait);
			_funcAsync = funcAsync;
		}

		private protected PolicyWrapper(Func<T> func, CancellationToken token) : base(token)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			_func = func;
		}

		internal abstract T Handle();

		internal abstract Task<T> HandleAsync(CancellationToken token);

		internal IEnumerable<PolicyDelegateResult<T>> PolicyDelegateResults
		{
			get { return _policyHandledResults; }
		}
	}
}
