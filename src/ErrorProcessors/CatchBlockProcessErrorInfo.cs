namespace PoliNorError
{
	public sealed class CatchBlockProcessErrorInfo
	{
		private CatchBlockProcessErrorInfo(){}

		public int CurrentRetryCount { get; private set; } = -1;

		public static CatchBlockProcessErrorInfo FromRetry(int retryAttempt)
		{
			return new CatchBlockProcessErrorInfo() { CurrentRetryCount = retryAttempt, PolicyKind = PolicyAlias.Retry };
		}

		public static CatchBlockProcessErrorInfo FromFallback()
		{
			return new CatchBlockProcessErrorInfo() { PolicyKind = PolicyAlias.Fallback };
		}

		public static CatchBlockProcessErrorInfo FromSimple()
		{
			return new CatchBlockProcessErrorInfo() { PolicyKind = PolicyAlias.Simple };
		}

		public PolicyAlias PolicyKind { get; private set; }
	}
}
