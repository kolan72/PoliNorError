namespace PoliNorError
{
	public sealed class CatchBlockProcessErrorInfo
	{
		private CatchBlockProcessErrorInfo(){}

		public int CurRetryErrorCount { get; private set; } = -1;

		public string Info { get; private set; }

		public static CatchBlockProcessErrorInfo FromRetry(int retryAttempt, string info = nameof(FromRetry))
		{
			return new CatchBlockProcessErrorInfo() { CurRetryErrorCount = retryAttempt, Info = info };
		}

		public static CatchBlockProcessErrorInfo FromFallback(string info = nameof(FromFallback))
		{
			return new CatchBlockProcessErrorInfo() { Info = info };
		}
	}
}
