using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal abstract class PolicyProcessorCatchBlockAsyncHandler<T> : PolicyProcessorCatchBlockHandlerBase<T>
	{
		private readonly bool _configAwait;

		protected PolicyProcessorCatchBlockAsyncHandler(PolicyResult policyResult, ICanHandleChecker<T> canHandleChecker, IBulkErrorProcessor bulkErrorProcessor, bool configAwait, CancellationToken cancellationToken) : base(policyResult, canHandleChecker, bulkErrorProcessor, cancellationToken)
		{
			_configAwait = configAwait;
		}

		public async Task<HandleCatchBlockResult> HandleAsync(Exception ex, ErrorContext<T> errorContext = null)
		{
			var (Result, CanProcess) = PreHandle(ex, errorContext);
			if (!CanProcess)
				return Result;

			var bulkProcessResult = await _bulkErrorProcessor.ProcessAsync(ex, errorContext.ToProcessingErrorContext(), _configAwait, _cancellationToken).ConfigureAwait(_configAwait);

			return PostHandle(bulkProcessResult, Result);
		}
	}
}
