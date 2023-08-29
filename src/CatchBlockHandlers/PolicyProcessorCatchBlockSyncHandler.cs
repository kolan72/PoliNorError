using System;
using System.Threading;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	internal abstract class PolicyProcessorCatchBlockSyncHandler<T> : PolicyProcessorCatchBlockHandlerBase<T>
	{
		protected PolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, ICanHandleChecker<T> canHandleChecker, IBulkErrorProcessor bulkErrorProcessor, CancellationToken cancellationToken)
			:base(policyResult, canHandleChecker, bulkErrorProcessor, cancellationToken)
		{
		}

		public HandleCatchBlockResult Handle(Exception ex, ErrorContext<T> errorContext = null)
		{
			var (Result, CanProcess) = PreHandle(ex, errorContext);
			if (!CanProcess)
				return Result;

			var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), _cancellationToken);

			return PostHandle(bulkProcessResult, Result);
		}
	}

	internal class DefaultPolicyProcessorCatchBlockSyncHandler : PolicyProcessorCatchBlockSyncHandler<Unit>
	{
		public DefaultPolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, ExceptionFilter exceptionFilter, CancellationToken cancellationToken)
														  : base(policyResult, new DefalutCanHandleChecker(exceptionFilter), bulkErrorProcessor, cancellationToken)
		{}
	}
}
