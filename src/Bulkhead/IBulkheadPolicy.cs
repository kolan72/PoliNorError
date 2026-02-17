using System;

namespace PoliNorError
{
	public interface IBulkheadPolicy : IPolicyBase
	{
		IBulkheadPolicy WithLimits(int maxParallelization, int maxQueueSize = 0);

		IBulkheadPolicy WithQueueTimeout(TimeSpan timeout);

		IBulkheadPolicy OnRejected(Action<PolicyResult> onRejected);
	}
}
