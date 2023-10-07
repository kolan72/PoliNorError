using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
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
