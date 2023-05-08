using System;

namespace PoliNorError
{
	public static class RetryProcessor
	{
		public static IRetryProcessor CreateDefault(Action<PolicyResult, Exception> errorSaverFunc = null) => new DefaultRetryProcessor(errorSaverFunc);
		public static IRetryProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, Action<PolicyResult, Exception> errorSaverFunc = null) => new DefaultRetryProcessor(bulkErrorProcessor, errorSaverFunc);
	}
}
