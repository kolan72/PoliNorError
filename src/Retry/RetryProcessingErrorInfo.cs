namespace PoliNorError
{
	internal class RetryProcessingErrorInfo : ProcessingErrorInfo
	{
		public RetryProcessingErrorInfo(int retryAttempt)
		{
			CurrentRetryCount = retryAttempt;
			HasContext = true;
			PolicyKind = PolicyAlias.Retry;
		}
	}
}
