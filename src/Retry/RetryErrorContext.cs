namespace PoliNorError
{
	internal class RetryErrorContext : ErrorContext<RetryContext>
	{
		private readonly bool _isPolicyAliasSet;

		public RetryErrorContext(int tryCount, bool isPolicyAliasSet = true) : this(new RetryContext(tryCount), isPolicyAliasSet){}

		public RetryErrorContext(RetryContext retryContext, bool isPolicyAliasSet = true) : base(retryContext) => _isPolicyAliasSet = isPolicyAliasSet;

		public override ProcessingErrorContext ToProcessingErrorContext()
		{
			var res = ProcessingErrorContext.FromRetry(Context.CurrentRetryCount);
			if (!_isPolicyAliasSet)
			{
				res.PolicyKind = PolicyAlias.Retry;
			}
			return res;
		}
	}

	internal class RetryContext
	{
		public RetryContext(int currentRetryCount) => CurrentRetryCount = currentRetryCount;

		public int CurrentRetryCount { get; }
	}
}
