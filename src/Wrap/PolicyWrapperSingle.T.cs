using System;
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
}
