using System;

namespace PoliNorError
{
	public class TimeoutRejectedException : TimeoutException
	{
		public TimeoutRejectedException(TimeSpan timeout)
			: base($"The operation timed out after {timeout}.")
		{
			Timeout = timeout;
		}

		public TimeSpan Timeout { get; }
	}
}
