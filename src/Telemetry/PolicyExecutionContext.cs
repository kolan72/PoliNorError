using System;
using System.Threading;

namespace PoliNorError
{
	public sealed class PolicyExecutionContext
	{
		public string OperationKey { get; set; }

		public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

		public int AttemptNumber { get; set; }

		public CancellationToken CancellationToken { get; set; }
	}
}
