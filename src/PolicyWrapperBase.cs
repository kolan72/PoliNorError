using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PoliNorError
{
	internal abstract class PolicyWrapperBase
	{
		protected CancellationToken _token;
		protected bool _configureAwait;

		protected IPolicyBase _policyBase;
		protected readonly IEnumerable<IPolicyBase> _polices;

		private readonly ThrowOnWrappedCollectionFailed _throwOnWrappedCollectionFailed = ThrowOnWrappedCollectionFailed.None;

		protected PolicyWrapperBase(IEnumerable<IPolicyBase> polices, CancellationToken token, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed, bool configureAwait = false) : this(token, configureAwait)
		{
			_polices = polices;
			WrapSinglePolicy = false;
			_throwOnWrappedCollectionFailed = throwOnWrappedCollectionFailed;
		}

		protected PolicyWrapperBase(IPolicyBase policyBase, CancellationToken token, bool configureAwait = false) : this(token, configureAwait)
		{
			_policyBase = policyBase;
			WrapSinglePolicy = true;
		}

		private PolicyWrapperBase(CancellationToken token, bool configureAwait = false)
		{
			_token = token;
			_configureAwait = configureAwait;
		}

		protected string GetExceptionMessage(IEnumerable<Exception> exceptions)
		{
			return $"Wrapped policy {_policyBase.PolicyName} excepions: {string.Join(";", exceptions.Select(exc => exc.Message))}";
		}

		protected void ThrowIfFailed(PolicyResult res)
		{
			if (res?.IsFailed == true)
			{
				if (res.FailedReason != PolicyResultFailedReason.PolicyResultHandlerFailed)
					throw res.UnprocessedError;
				else
					throw new PolicyResultHandlerFailedException();
			}
		}

		protected void ThrowIfCollectionFailed(IEnumerable<PolicyDelegateResult> results)
		{
			if (_throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.LastError)
			{
				ThrowIfFailed(results.LastOrDefault()?.Result);
			}
			else
			{
				if (results.LastOrDefault()?.Result.FailedReason != PolicyResultFailedReason.PolicyResultHandlerFailed)
					throw new PolicyDelegateCollectionException(results);
				else
					throw new PolicyResultHandlerFailedException();
			}
		}

		protected bool WrapSinglePolicy { get; }
	}

	public enum ThrowOnWrappedCollectionFailed
	{
		None,
		LastError,
		CollectionError
	}
}
