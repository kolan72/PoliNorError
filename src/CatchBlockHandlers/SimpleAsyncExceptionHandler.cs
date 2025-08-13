using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class SimpleAsyncExceptionHandler
	{
		private readonly PolicyResult _policyResult;
		private readonly IBulkErrorProcessor _bulkErrorProcessor;
		private readonly Func<Exception, bool> _errorFilterFunc;
		private readonly bool _configAwait;
		private readonly CancellationToken _token;

		public SimpleAsyncExceptionHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, Func<Exception, bool> errorFilterFunc, bool configAwait, CancellationToken token)
		{
			_policyResult = policyResult;
			_bulkErrorProcessor = bulkErrorProcessor;
			_errorFilterFunc = errorFilterFunc;
			_configAwait = configAwait;
			_token = token;
		}

		public async Task<bool> HandleAsync(Exception ex, EmptyErrorContext emptyErrorContext)
		{
			_policyResult.AddError(ex);

			var handler = new PolicyProcessorCatchBlockAsyncHandler<Unit>(_policyResult,
																_bulkErrorProcessor,
																_configAwait,
																_token,
																_errorFilterFunc);

			return _policyResult.ChangeByHandleCatchBlockResult(
										await handler.HandleAsync(ex, emptyErrorContext).ConfigureAwait(_configAwait));
		}
	}
}
