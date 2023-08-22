using System;

namespace PoliNorError
{
	public static class RetryProcessor
	{
		public static IRetryProcessor CreateDefault(bool failedIfSaveErrorThrows = false) => new DefaultRetryProcessor(failedIfSaveErrorThrows);
		public static IRetryProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false) => new DefaultRetryProcessor(bulkErrorProcessor, failedIfSaveErrorThrows);
	}
}
