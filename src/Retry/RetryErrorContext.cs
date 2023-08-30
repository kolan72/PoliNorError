namespace PoliNorError
{
	internal class RetryErrorContext : ErrorContext<RetryContext>
	{
		public RetryErrorContext(RetryContext retryContext):base(retryContext){}

		public override ProcessingErrorContext ToProcessingErrorContext()
		{
			return ProcessingErrorContext.FromRetry(Context.CurrentRetryCount);
		}
	}

	internal class RetryContext
	{
		public RetryContext(int currentRetryCount) => CurrentRetryCount = currentRetryCount;

		public int CurrentRetryCount { get; }
	}
}
