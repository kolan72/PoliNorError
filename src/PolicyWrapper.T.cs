using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class PolicyWrapper<T> : PolicyWrapperBase
	{
		private readonly Func<CancellationToken, Task<T>> _funcAsync;
		private readonly Func<T> _func;

		private readonly FlexSyncEnumerable<PolicyDelegateResult<T>> _policyHandledResults;

		internal PolicyWrapper(IPolicyBase policyBase, Func<CancellationToken, Task<T>> funcAsync, CancellationToken token, bool configureAwait) : base(policyBase, token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>(!configureAwait);
			_funcAsync = funcAsync;
		}

		internal PolicyWrapper(IPolicyBase policyBase, Func<T> func, CancellationToken token) : base(policyBase, token)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			_func = func;
		}

		internal T Handle()
		{
			var res = _policyBase.Handle(_func, _token);

			_policyHandledResults.Add(new PolicyDelegateResult<T>(res, _policyBase.PolicyName, _func.Method));

			ThrowIfFailed(res);

			return res.Result;
		}

		internal async Task<T> HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_funcAsync, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyDelegateResult<T>(res, _policyBase.PolicyName, _funcAsync.Method));

			ThrowIfFailed(res);

			return res.Result;
		}

		internal IEnumerable<PolicyDelegateResult<T>> PolicyDelegateResults
		{
			get { return _policyHandledResults; }
		}
	}
}
