using System;

namespace PoliNorError
{
	public class RetryCountInfoOptions
	{
		public Func<int, bool> CanRetryInner { get; set; }
		public int StartTryCount { get; set; }
	}
}
