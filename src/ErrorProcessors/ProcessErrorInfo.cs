namespace PoliNorError
{
	public sealed class ProcessErrorInfo
	{
		private ProcessErrorInfo(){}

		public int CurrentRetryCount { get; private set; } = -1;

		public static ProcessErrorInfo FromRetry(int retryAttempt)
		{
			return new ProcessErrorInfo() { CurrentRetryCount = retryAttempt, PolicyKind = PolicyAlias.Retry };
		}

		public static ProcessErrorInfo FromFallback()
		{
			return new ProcessErrorInfo() { PolicyKind = PolicyAlias.Fallback };
		}

		public static ProcessErrorInfo FromSimple()
		{
			return new ProcessErrorInfo() { PolicyKind = PolicyAlias.Simple };
		}

		public PolicyAlias PolicyKind { get; private set; }
	}
}
