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
					throw new PolicyResultHandlerFailedException();
			}
		}
	}

	/// <summary>
	/// The way in which an exception will be generated if the last policy in the PolicyCollection fails.
	/// </summary>
	public enum ThrowOnWrappedCollectionFailed
	{
		None,
		/// <summary>
		/// The last <see cref="PolicyResult.UnprocessedError"/>  exception will be thrown.
		/// </summary>
		LastError,
		/// <summary>
		/// The <see cref="PolicyDelegateCollectionException"/>  exception will be thrown.
		/// </summary>
		CollectionError
	}
}
