using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IBulkErrorProcessor : IEnumerable<IErrorProcessor>
	{
		void AddProcessor(IErrorProcessor errorProcessor);
		Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default);
		BulkErrorProcessor.BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default);
	}

	public static class IBulkErrorProcessorExtensions
	{
		public static Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(this IBulkErrorProcessor bulkErrorProcessor, Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
			=> bulkErrorProcessor.ProcessAsync(handlingError, errorContext, false, token);
	}
}