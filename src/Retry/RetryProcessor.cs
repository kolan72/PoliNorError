using System;

namespace PoliNorError
{
	public static class RetryProcessor
	{
		public static IRetryProcessor CreateDefault(bool setFailedIfInvocationError = false) => new DefaultRetryProcessor(setFailedIfInvocationError);
		public static IRetryProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, bool setFailedIfInvocationError = false) => new DefaultRetryProcessor(bulkErrorProcessor, setFailedIfInvocationError);
	}
}
