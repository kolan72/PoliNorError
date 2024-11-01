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
				if (res.FailedReason != PolicyResultFailedReason.PolicyResultHandlerFailed)
					throw res.UnprocessedError;
				else
					throw new PolicyResultHandlerFailedException(res);
			}
		}
	}
}
