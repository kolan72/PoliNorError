using System;

namespace PoliNorError
{
	public sealed class BulkheadRejectedException : InvalidOperationException
	{
		public BulkheadRejectedException()
			: base("The bulkhead rejected the execution because all slots and queue entries are occupied.")
		{
		}
	}
}
