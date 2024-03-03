namespace PoliNorError
{
	public class ProcessingErrorInfo
	{
		protected ProcessingErrorInfo(){}

		internal ProcessingErrorInfo(ProcessingErrorContext currentContext) : this(currentContext.PolicyKind, currentContext){}

		public ProcessingErrorInfo(PolicyAlias policyKind,  ProcessingErrorContext currentContext = null)
		{
			PolicyKind = policyKind;
			if (currentContext != null)
			{
				PopulateContextProperties(currentContext, policyKind);
				HasContext = true;
			}
		}

		public int CurrentRetryCount { get; protected set; } = -1;

		private void PopulateContextProperties(ProcessingErrorContext currentContext, PolicyAlias policyKind)
		{
			switch (policyKind)
			{
				case PolicyAlias.Retry:
					CurrentRetryCount = currentContext.CurrentRetryCount;
					break;
				default:
					return;
			}
		}

		public static ProcessingErrorInfo FromRetry(int retryAttempt)
		{
			return new RetryProcessingErrorInfo(retryAttempt);
		}

		public PolicyAlias PolicyKind { get; protected set; }

		public bool HasContext { get; protected set; }
	}
}
