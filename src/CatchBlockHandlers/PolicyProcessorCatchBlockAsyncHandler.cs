using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyProcessorCatchBlockAsyncHandler<T> : PolicyProcessorCatchBlockHandlerBase<T>
	{
		private readonly bool _configAwait;

		public PolicyProcessorCatchBlockAsyncHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, bool configAwait, CancellationToken cancellationToken, Func<Exception, bool> errorFilterFunc, Func<ErrorContext<T>, bool> policyRuleFunc = null)
				: base(policyResult, bulkErrorProcessor, cancellationToken, errorFilterFunc, policyRuleFunc)
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
