using System;
using System.Threading;

namespace PoliNorError
{
	public sealed class BulkheadOptions
	{
		public int MaxParallelization { get; set; } = 1;

		public int MaxQueueSize { get; set; }

		public TimeSpan QueueTimeout { get; set; } = Timeout.InfiniteTimeSpan;
	}
}
