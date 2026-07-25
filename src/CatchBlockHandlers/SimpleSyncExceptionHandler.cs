using System;
using System.Threading;

namespace PoliNorError
{
#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This class is obsolete")]
#pragma warning restore S1133 // Deprecated code should be removed
	internal class SimpleSyncExceptionHandler
	{
		private readonly PolicyResult _policyResult;
		private readonly IBulkErrorProcessor _bulkErrorProcessor;
		private readonly Func<Exception, bool> _errorFilterFunc;
		private readonly CancellationToken _token;

		public SimpleSyncExceptionHandler(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, Func<Exception, bool> errorFilterFunc, CancellationToken token)
		{
			_policyResult = policyResult;
			_bulkErrorProcessor = bulkErrorProcessor;
			_errorFilterFunc = errorFilterFunc;
			_token = token;
		}

		public bool Handle(Exception ex, EmptyErrorContext emptyErrorContext)
		{
			_policyResult.AddError(ex);

			var handler = new PolicyProcessorCatchBlockSyncHandler<Unit>(_policyResult,
																_bulkErrorProcessor,
																_token,
																_errorFilterFunc);
			return _policyResult.WasResultSetToFailureByCatchBlock(
										handler.Handle(ex, emptyErrorContext));
		}
	}
}
