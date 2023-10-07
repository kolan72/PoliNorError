using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
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
