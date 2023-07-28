using System;

namespace PoliNorError
{
	public static class RetryProcessor
	{
		public static IRetryProcessor CreateDefault(Action<PolicyResult, Exception> errorSaverFunc = null, bool setFailedIfInvocationError = false) => new DefaultRetryProcessor(errorSaverFunc, setFailedIfInvocationError);
		public static IRetryProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, Action<PolicyResult, Exception> errorSaverFunc = null, bool setFailedIfInvocationError = false) => new DefaultRetryProcessor(bulkErrorProcessor, errorSaverFunc, setFailedIfInvocationError);
	}
}
