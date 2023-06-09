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

		private readonly FlexSyncEnumerable<PolicyHandledResult<T>> _policyHandledResults;

		internal PolicyWrapper(IPolicyBase policyBase, Func<CancellationToken, Task<T>> funcAsync, CancellationToken token, bool configureAwait) : base(policyBase, token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyHandledResult<T>>(!configureAwait);
			_funcAsync = funcAsync;
		}

		internal PolicyWrapper(IPolicyBase policyBase, Func<T> func, CancellationToken token) : base(policyBase, token)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyHandledResult<T>>();
			_func = func;
		}

		internal T Handle()
		{
			var res = _policyBase.Handle(_func, _token);

			_policyHandledResults.Add(new PolicyHandledResult<T>(new PolicyDelegateInfo(_policyBase, _func.Method), res));

			if (res.IsFailed)
				throw res.Errors.LastOrDefault();

			return res.Result;
		}

		internal async Task<T> HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_funcAsync, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyHandledResult<T>(new PolicyDelegateInfo(_policyBase, _funcAsync.Method), res));

			if (res.IsFailed)
				throw res.Errors.LastOrDefault();

			return res.Result;
		}

		internal IEnumerable<PolicyHandledResult<T>> PolicyResults
		{
			get { return _policyHandledResults; }
		}
	}

	internal sealed class PolicyWrapper : PolicyWrapperBase
	{
		private readonly Func<CancellationToken, Task> _func;
		private readonly Action _action;
		private readonly FlexSyncEnumerable<PolicyHandledResult> _policyHandledResults;

		internal PolicyWrapper(IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait) : base(policyBase, token, configureAwait)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyHandledResult>(!configureAwait);
			_func = func;
		}

		internal PolicyWrapper(IPolicyBase policyBase, Action action, CancellationToken token) : base(policyBase, token)
		{
			_policyHandledResults = new FlexSyncEnumerable<PolicyHandledResult>();
			_action = action;
		}

		internal async Task HandleAsync(CancellationToken token)
		{
			var res = await _policyBase.HandleAsync(_func, _configureAwait, token).ConfigureAwait(_configureAwait);

			_policyHandledResults.Add(new PolicyHandledResult(new PolicyDelegateInfo(_policyBase, _func.Method), res));

			if (res.IsFailed)
				throw res.Errors.LastOrDefault();
		}

		internal void Handle()
		{
			var res = _policyBase.Handle(_action, _token);

			_policyHandledResults.Add(new PolicyHandledResult(new PolicyDelegateInfo(_policyBase, _action.Method), res));

			if (res.IsFailed)
			{
				throw res.Errors.LastOrDefault();
			}
		}

		internal IEnumerable<PolicyHandledResult> PolicyResults
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
