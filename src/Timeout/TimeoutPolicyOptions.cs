using System;

namespace PoliNorError
{
	public sealed class TimeoutPolicyOptions
	{
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

		public TimeoutStrategy Strategy { get; set; } = TimeoutStrategy.Optimistic;

		public bool ThrowTimeoutException { get; set; }
	}
}
