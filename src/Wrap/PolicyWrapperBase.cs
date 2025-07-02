using System;
using System.Threading;

namespace PoliNorError
{
	internal abstract class PolicyWrapperBase
	{
		protected CancellationToken _token;
		protected bool _configureAwait;

		protected PolicyWrapperBase(CancellationToken token, bool configureAwait = false)
		{
			_token = token;
			_configureAwait = configureAwait;
		}

		protected void ThrowIfFailed(PolicyResult res)
		{
			if (res?.IsFailed == true)
			{
				if (IsNaturalFailed(res))
					throw res.UnprocessedError ?? res.PolicyCanceledError ?? GetCanceledOrApplicationException(res);
				else
					throw new PolicyResultHandlerFailedException(res);
			}
		}

		protected void ThrowIfFailed<T>(PolicyResult<T> res)
		{
			if (res?.IsFailed == true)
			{
				if (IsNaturalFailed(res))
					throw res.UnprocessedError;
				else
					throw new PolicyResultHandlerFailedException<T>(res);
			}
		}

		private static bool IsNaturalFailed(PolicyResult res)
		{
			return res.FailedReason != PolicyResultFailedReason.PolicyResultHandlerFailed;
		}

		private static Exception GetCanceledOrApplicationException(PolicyResult pr)
		{
			return pr.IsCanceled ? new OperationFailedAndCanceledException() : (Exception)new ApplicationException();
		}
	}
}
