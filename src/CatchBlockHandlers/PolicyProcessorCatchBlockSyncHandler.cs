using System;
using System.Threading;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	internal abstract class PolicyProcessorCatchBlockSyncHandler<T>
	{
		private readonly PolicyResult _policyResult;
		private readonly CancellationToken _cancellationToken;
		private readonly IBulkErrorProcessor _bulkErrorProcessor;
		private readonly ICanHandleChecker<T> _canHandleChecker;

		protected PolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, ICanHandleChecker<T> canHandleChecker, CancellationToken cancellationToken)
		{
			_policyResult = policyResult;
			_bulkErrorProcessor = bulkErrorProcessor;
			_canHandleChecker = canHandleChecker;
			_cancellationToken = cancellationToken;
		}

		public HandleCatchBlockResult Handle(Exception ex, ErrorContext<T> errorContext = null)
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				return HandleCatchBlockResult.Canceled;
			}
			var checkFallbackResult = _canHandleChecker.CanHandle(ex, errorContext);
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

	internal class DefaultPolicyProcessorCatchBlockSyncHandler : PolicyProcessorCatchBlockSyncHandler<Unit>
	{
		public DefaultPolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, ExceptionFilter exceptionFilter, CancellationToken cancellationToken)
														  : base(policyResult, bulkErrorProcessor, new DefalutCanHandleChecker(exceptionFilter), cancellationToken)
		{}
	}
}
