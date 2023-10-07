using System;
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

		internal override async Task HandleAsync(CancellationToken token)
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
}
