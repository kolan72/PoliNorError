using System;
using System.Threading;

namespace PoliNorError
{
	internal abstract class PolicyProcessorCatchBlockSyncHandler<T>
	{
		private readonly PolicyResult _policyResult;
		private readonly CancellationToken _cancellationToken;
		private readonly IBulkErrorProcessor _bulkErrorProcessor;

		protected PolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, CancellationToken cancellationToken)
		{
			_policyResult = policyResult;
			_bulkErrorProcessor = bulkErrorProcessor;
			_cancellationToken = cancellationToken;
		}

		public void Handle(Exception ex, ErrorContext<T> errorContext = null)
		{
			_policyResult.ChangeByHandleCatchBlockResult(CanHandleCatchBlock());

			HandleCatchBlockResult CanHandleCatchBlock()
			{
				if (_cancellationToken.IsCancellationRequested)
				{
					return HandleCatchBlockResult.Canceled;
				}
				var checkFallbackResult = CanHandle(ex, errorContext);
				if (checkFallbackResult == HandleCatchBlockResult.Success)
				{
					var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), _cancellationToken);
					_policyResult.AddBulkProcessorErrors(bulkProcessResult);
					return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : checkFallbackResult;
				}
				else
				{
					return checkFallbackResult;
				}
			}
		}

		internal abstract HandleCatchBlockResult CanHandle(Exception exception, ErrorContext<T> errorContext);
	}
}
