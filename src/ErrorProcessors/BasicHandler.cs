using System;
using System.Threading;

namespace PoliNorError
{
	internal static class BasicHandler
	{
		public static bool TryEvaluateRuleThenProcessException<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			CancellationToken token)
		{
			return TryEvaluateRuleThenProcessException(
				ex,
				policyResult,
				errorContext,
				PolicyProcessor.ErrorSaver<T>.Default,
				policyRuleFunc,
				bulkErrorProcessor,
				cancellationEffect,
				token);
		}

		public static bool TryEvaluateRuleThenProcessException<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> errorSaver,
			Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			CancellationToken token)
		{
			errorSaver(policyResult, ex, errorContext, token);
			if (token.IsCancellationRequested)
			{
				policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
				return false;
			}

			var(accepted, canceled, error) = RunPolicyRuleFunc(errorContext, policyRuleFunc, token);
			if (!accepted)
			{
				policyResult.HandlePolicyRuleFailure(error, canceled, ex);
				return false;
			}

			var bulkProcessResult = bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
			policyResult.AddBulkProcessorErrors(bulkProcessResult);
			if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
			{
				policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
				return false;
			}
			return true;
		}

		private static (bool Result, bool IsCanceled, Exception error) RunPolicyRuleFunc<T>(ErrorContext<T> errorContext, Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc, CancellationToken token)
		{
			try
			{
				var result = policyRuleFunc?.Invoke(errorContext, token);
				return (result != false, false, null);
			}
			catch (OperationCanceledException tce) when (token.IsCancellationRequested)
			{
				return (false, true, tce);
			}
			catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
			{
				return (false, true, ae.GetCancellationException());
			}
			catch (Exception cex)
			{
				return (false, false, cex);
			}
		}

	}
}
