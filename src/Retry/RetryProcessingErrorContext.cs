namespace PoliNorError
{
	internal class RetryProcessingErrorContext : ProcessingErrorContext
	{
		public RetryProcessingErrorContext(int retryCount) : base(PolicyAlias.Retry)
		{
			RetryCount = retryCount;
		}

		internal override ProcessingErrorInfo ToProcessingErrorInfo(PolicyAlias policyAlias)
		{
			return new RetryProcessingErrorInfo(RetryCount);
		}

		public int RetryCount { get; }
	}
}
