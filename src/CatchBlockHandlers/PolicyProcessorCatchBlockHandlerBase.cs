using System;
using System.Threading;
using static PoliNorError.BulkErrorProcessor;

namespace PoliNorError
{
	internal abstract class PolicyProcessorCatchBlockHandlerBase<T>
	{
		protected readonly PolicyResult _policyResult;
		protected readonly CancellationToken _cancellationToken;
		protected readonly IBulkErrorProcessor _bulkErrorProcessor;

		private readonly Func<Exception, bool> _errorFilterFunc;
		private readonly Func<ErrorContext<T>, bool> _policyRuleFunc;

		protected PolicyProcessorCatchBlockHandlerBase(PolicyResult policyResult, IBulkErrorProcessor bulkErrorProcessor, CancellationToken cancellationToken, Func<Exception, bool> errorFilterFunc, Func<ErrorContext<T>, bool> policyRuleFunc = null)
		{
			_policyResult = policyResult;
			_cancellationToken = cancellationToken;
			_bulkErrorProcessor = bulkErrorProcessor;
			_errorFilterFunc = errorFilterFunc;
			_policyRuleFunc = policyRuleFunc ?? ((_) => true);
		}

		protected (HandleCatchBlockResult Result, bool CanProcess) PreHandle(Exception ex, ErrorContext<T> errorContext)
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				return (HandleCatchBlockResult.Canceled, false);
			}
			var checkFallbackResult = CanHandle(ex, errorContext);
			if (checkFallbackResult != HandleCatchBlockResult.Success)
				return (checkFallbackResult, false);
			else
				return (HandleCatchBlockResult.Success, true);
		}

		private HandleCatchBlockResult CanHandle(Exception ex, ErrorContext<T> errorContext)
		{
			if (!RunErrorFilterFunc(ex))
				return HandleCatchBlockResult.FailedByErrorFilter;
			else if (!_policyRuleFunc(errorContext))
				return HandleCatchBlockResult.FailedByPolicyRules;
			else
				return HandleCatchBlockResult.Success;
		}

		protected HandleCatchBlockResult PostHandle(BulkProcessResult bulkProcessResult, HandleCatchBlockResult resultIfNotCanceled)
		{
			_policyResult.AddBulkProcessorErrors(bulkProcessResult);
			return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : resultIfNotCanceled;
		}

		private bool RunErrorFilterFunc(Exception ex)
		{
			try
			{
				return _errorFilterFunc(ex);
			}
			catch (Exception exIn)
			{
				_policyResult.SetFailedWithCatchBlockError(exIn, ex, CatchBlockExceptionSource.ErrorFilter);
				return false;
			}
		}
	}
}