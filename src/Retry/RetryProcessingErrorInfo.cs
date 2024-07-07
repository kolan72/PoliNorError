namespace PoliNorError
{
	public class RetryProcessingErrorInfo : ProcessingErrorInfo
	{
		internal RetryProcessingErrorInfo(int retryAttempt) : base(PolicyAlias.Retry, new RetryProcessingErrorContext(retryAttempt))
		{
#pragma warning disable CS0618 // Type or member is obsolete
			CurrentRetryCount = retryAttempt;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public int RetryCount => ((RetryProcessingErrorContext)CurrentContext).RetryCount;
	}
}
