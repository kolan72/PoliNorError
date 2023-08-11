namespace PoliNorError
{
	public sealed class ProcessingErrorInfo
	{
		private ProcessingErrorInfo(){}

		public ProcessingErrorInfo(PolicyAlias policyKind,  ProcessingErrorContext currentContext = null)
		{
			PolicyKind = policyKind;
			if (currentContext != null)
			{
				PopulateContextProperties(currentContext, policyKind);
				HasContext = true;
			}
		}

		public int CurrentRetryCount { get; private set; } = -1;

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
			return new ProcessingErrorInfo() { CurrentRetryCount = retryAttempt, PolicyKind = PolicyAlias.Retry };
		}

		public PolicyAlias PolicyKind { get; private set; }

		public bool HasContext { get; }
	}

	public class ProcessingErrorContext
	{
		public int CurrentRetryCount { get; private set; } = -1;

		public static ProcessingErrorContext FromRetry(int currentRetryCount)
		{
			return new ProcessingErrorContext() { CurrentRetryCount = currentRetryCount };
		}
	}
}
