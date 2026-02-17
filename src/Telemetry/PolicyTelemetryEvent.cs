using System;

namespace PoliNorError
{
	public sealed class PolicyTelemetryEvent
	{
		public string PolicyName { get; set; }

		public string EventName { get; set; }

		public Exception Exception { get; set; }

		public PolicyExecutionContext Context { get; set; }
	}
}
