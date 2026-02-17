using System;

namespace PoliNorError
{
	public sealed class CircuitBreakerOptions
	{
		public int FailureThreshold { get; set; } = 5;

		public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

		public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(15);

		public int HalfOpenMaxCalls { get; set; } = 1;

		public bool BreakOnHandledExceptionsOnly { get; set; } = true;
	}
}
