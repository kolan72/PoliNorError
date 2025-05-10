namespace PoliNorError
{
	public class RetryProcessingErrorInfo : ProcessingErrorInfo, IRetryExecutionInfo
	{
		internal RetryProcessingErrorInfo(int retryAttempt) : base(new RetryProcessingErrorContext(retryAttempt))
		{
#pragma warning disable CS0618 // Type or member is obsolete
			CurrentRetryCount = retryAttempt;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public int RetryCount => ((RetryProcessingErrorContext)CurrentContext).RetryCount;
	}
}
