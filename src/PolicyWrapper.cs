using System;
using System.Collections.Generic;
using System.Linq;
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

			if (res.IsFailed)
				throw res.Errors.LastOrDefault();

			return res.Result;
		}

		internal async Task<T> HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_funcAsync, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyDelegateResult<T>(res, _policyBase.PolicyName, _funcAsync.Method));

			if (res.IsFailed)
				throw res.Errors.LastOrDefault();

			return res.Result;
		}

		internal IEnumerable<PolicyDelegateResult<T>> PolicyResults
		{
			get { return _policyHandledResults; }
		}
	}

	internal sealed class PolicyWrapper : PolicyWrapperBase
	{
		private readonly Func<CancellationToken, Task> _func;
		private readonly Action _action;
		private readonly FlexSyncEnumerable<PolicyDelegateResult> _policyHandledResults;

		internal PolicyWrapper(IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait) : base(policyBase, token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>(!configureAwait);
			_func = func;
		}

		internal PolicyWrapper(IPolicyBase policyBase, Action action, CancellationToken token) : base(policyBase, token)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			_action = action;
		}

		internal async Task HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_func, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyDelegateResult(res, _policyBase.PolicyName, _func.Method));

			if (res.IsFailed)
				throw res.Errors.LastOrDefault();
		}

		internal void Handle()
		{
			var res = _policyBase.Handle(_action, _token);

			_policyHandledResults.Add(new PolicyDelegateResult(res, _policyBase.PolicyName, _action.Method));

			if (res.IsFailed)
			{
				throw res.Errors.LastOrDefault();
			}
		}

		internal IEnumerable<PolicyDelegateResult> PolicyDelegateResults
		{
			get { return _policyHandledResults; }
		}
	}

	internal class PolicyWrapperBase
	{
		protected CancellationToken _token;
		protected IPolicyBase _policyBase;
		protected bool _configureAwait;

		protected PolicyWrapperBase(IPolicyBase policyBase, CancellationToken token, bool configureAwait = false)
		{
			_token = token;
			_policyBase = policyBase;
			_configureAwait = configureAwait;
		}

		protected string GetExceptionMessage(IEnumerable<Exception> exceptions)
		{
			return $"Wrapped policy {_policyBase.PolicyName} excepions: {string.Join(";", exceptions.Select(exc => exc.Message))}";
		}
	}
}
