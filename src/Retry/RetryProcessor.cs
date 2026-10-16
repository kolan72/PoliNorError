using System;

namespace PoliNorError
{
	/// <summary>
	/// A retry processor that can create default retry processor instances.
	/// </summary>
	public static class RetryProcessor
	{
		/// <inheritdoc cref="CreateDefault(IBulkErrorProcessor, bool)"/>
		public static IRetryProcessor CreateDefault(bool failedIfSaveErrorThrows = false) => new DefaultRetryProcessor(failedIfSaveErrorThrows);

		/// <summary>
		/// Creates and returns the <see cref="DefaultRetryProcessor"/> class, which represents the default implementation of the <see cref="IRetryProcessor"/> interface.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="failedIfSaveErrorThrows">Specifies whether an exception is thrown if saving the error fails.</param>
		/// <returns></returns>
		public static IRetryProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false) => new DefaultRetryProcessor(bulkErrorProcessor, failedIfSaveErrorThrows);
	}
}
