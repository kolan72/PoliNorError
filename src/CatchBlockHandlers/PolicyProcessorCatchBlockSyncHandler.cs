using System;
using System.Threading;

namespace PoliNorError
{
#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This class is obsolete")]
#pragma warning restore S1133 // Deprecated code should be removed
	internal class PolicyProcessorCatchBlockSyncHandler<T> : PolicyProcessorCatchBlockHandlerBase<T>
	{
		public PolicyProcessorCatchBlockSyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, CancellationToken cancellationToken, Func<Exception, bool> errorFilterFunc, Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc = null)
			:base(policyResult, bulkErrorProcessor, cancellationToken, errorFilterFunc, policyRuleFunc)
		{
		}

		public HandleCatchBlockResult Handle(Exception ex, ErrorContext<T> errorContext = null)
		{
			var shouldHandleResult = ShouldHandleException(ex, errorContext);
			if (shouldHandleResult != HandleCatchBlockResult.Success)
				return shouldHandleResult;

			var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), _cancellationToken);

			_policyResult.AddBulkProcessorErrors(bulkProcessResult);
			return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : shouldHandleResult;
		}
	}
}
