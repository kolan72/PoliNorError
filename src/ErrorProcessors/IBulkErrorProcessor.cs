using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Defines methods for bulk error processing
	/// </summary>
	public interface IBulkErrorProcessor : IEnumerable<IErrorProcessor>, ICanAddErrorProcessor
	{
		/// <summary>
		/// Adds an error processor to the collection.
		/// </summary>
		/// <param name="errorProcessor">The error processor to add.</param>
		void AddProcessor(IErrorProcessor errorProcessor);

		/// <summary>
		/// Processes an exception asynchronously through the collection of error processors.
		/// </summary>
		/// <param name="handlingError">The exception to handle.</param>
		/// <param name="errorContext">The context of the processing error.</param>
		/// <param name="configAwait">A value indicating whether to configure await.</param>
		/// <param name="token">A cancellation token that can be used to cancel the work.</param>
		/// <returns>A task that represents the asynchronous operation, containing the result of the bulk processing.</returns>
		Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default);

		/// <summary>
		/// Processes an exception synchronously through the collection of error processors.
		/// </summary>
		/// <param name="handlingError">The exception to handle.</param>
		/// <param name="errorContext">The context of the processing error.</param>
		/// <param name="token">A cancellation token that can be used to cancel the work.</param>
		/// <returns>The result of the bulk processing.</returns>
		BulkErrorProcessor.BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default);
	}

	/// <summary>
	/// Provides extension methods for the <see cref="IBulkErrorProcessor"/> interface.
	/// </summary>
	public static class IBulkErrorProcessorExtensions
	{
		/// <summary>
		/// Processes an exception asynchronously through the collection of error processors with `ConfigureAwait(false)`.
		/// </summary>
		/// <param name="bulkErrorProcessor">The bulk error processor instance.</param>
		/// <param name="handlingError">The exception to handle.</param>
		/// <param name="errorContext">The context of the processing error.</param>
		/// <param name="token">A cancellation token that can be used to cancel the work.</param>
		/// <returns>A task that represents the asynchronous operation, containing the result of the bulk processing.</returns>
		public static Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(this IBulkErrorProcessor bulkErrorProcessor, Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
			=> bulkErrorProcessor.ProcessAsync(handlingError, errorContext, false, token);
	}
}