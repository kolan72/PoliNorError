using System;
using System.Threading;
using static PoliNorError.BulkErrorProcessor;

namespace PoliNorError
{
	internal abstract class PolicyProcessorCatchBlockHandlerBase<T>
	{
		protected readonly PolicyResult _policyResult;
		protected readonly CancellationToken _cancellationToken;
		protected readonly ICanHandleChecker<T> _canHandleChecker;
		protected readonly IBulkErrorProcessor _bulkErrorProcessor;

		protected PolicyProcessorCatchBlockHandlerBase(PolicyResult policyResult, ICanHandleChecker<T> canHandleChecker, IBulkErrorProcessor bulkErrorProcessor, CancellationToken cancellationToken)
		{
			_policyResult = policyResult;
			_canHandleChecker = canHandleChecker;
			_cancellationToken = cancellationToken;
			_bulkErrorProcessor = bulkErrorProcessor;
		}

		protected (HandleCatchBlockResult Result, bool CanProcess) PreHandle(Exception ex, ErrorContext<T> errorContext)
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				return (HandleCatchBlockResult.Canceled, false);
			}
			var checkFallbackResult = _canHandleChecker.CanHandle(ex, errorContext);
			if (checkFallbackResult != HandleCatchBlockResult.Success)
				return (checkFallbackResult, false);
			else
				return (HandleCatchBlockResult.Success, true);
		}

		protected HandleCatchBlockResult PostHandle(BulkProcessResult bulkProcessResult, HandleCatchBlockResult resultIfNotCanceled)
		{
			_policyResult.AddBulkProcessorErrors(bulkProcessResult);
			return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : resultIfNotCanceled;
		}
	}
}