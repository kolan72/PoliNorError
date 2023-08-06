using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IBulkErrorProcessor : IEnumerable<IErrorProcessor>
	{
		void AddProcessor(IErrorProcessor errorProcessor);
		Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(ProcessErrorInfo catchBlockProcessErrorInfo, Exception handlingError, bool configAwait = false, CancellationToken token = default);
		BulkErrorProcessor.BulkProcessResult Process(ProcessErrorInfo catchBlockProcessErrorInfo, Exception handlingError, CancellationToken token = default);
	}

	public static class IBulkErrorProcessorExtensions
	{
		public static Task<BulkErrorProcessor.BulkProcessResult> ProcessAsync(this IBulkErrorProcessor bulkErrorProcessor, ProcessErrorInfo catchBlockProcessErrorInfo, Exception handlingError, CancellationToken token)
			=> bulkErrorProcessor.ProcessAsync(catchBlockProcessErrorInfo, handlingError, false, token);
	}
}