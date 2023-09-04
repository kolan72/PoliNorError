using System;
using System.Threading;

namespace PoliNorError
{
	internal class PolicyProcessorCatchBlockSyncHandler<T> : PolicyProcessorCatchBlockHandlerBase<T>
	{
		public PolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, CancellationToken cancellationToken, Func<Exception, bool> errorFilterFunc, Func<ErrorContext<T>, bool> policyRuleFunc = null)
			:base(policyResult, bulkErrorProcessor, cancellationToken, errorFilterFunc, policyRuleFunc)
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
}
