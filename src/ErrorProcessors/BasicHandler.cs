using System;
using System.Threading;
using System.Threading.Tasks;

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

		public static bool TryProcessExceptionThenEvaluateRule<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			CancellationToken token)
		{
			return TryProcessExceptionThenEvaluateRule(
				ex,
				policyResult,
				errorContext,
				PolicyProcessor.ErrorSaver<T>.Default,
				policyRuleFunc,
				bulkErrorProcessor,
				cancellationEffect,
				token);
		}

		public static bool TryProcessExceptionThenEvaluateRule<T>(
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

			var bulkProcessResult = bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
			policyResult.AddBulkProcessorErrors(bulkProcessResult);
			if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
			{
				policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
				return false;
			}

			var (accepted, canceled, error) = RunPolicyRuleFunc(errorContext, policyRuleFunc, token);
			if (!accepted)
			{
				policyResult.HandlePolicyRuleFailure(error, canceled, ex);
				return false;
			}
			return true;
		}

		public static Task<bool> TryProcessExceptionThenEvaluateRuleAsync<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			bool configureAwait,
			CancellationToken token)
		{
			return TryProcessExceptionThenEvaluateRuleAsync(
				ex,
				policyResult,
				errorContext,
				PolicyProcessor.AsyncErrorSaver<T>.Default,
				policyRuleFunc,
				bulkErrorProcessor,
				cancellationEffect,
				configureAwait,
				token);
		}

		public static async Task<bool> TryProcessExceptionThenEvaluateRuleAsync<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> errorSaver,
			Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			bool configureAwait,
			CancellationToken token)
		{
			await errorSaver(policyResult, ex, errorContext, configureAwait, token).ConfigureAwait(configureAwait);
			if (token.IsCancellationRequested)
			{
				policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
				return false;
			}

			var bulkProcessResult = await bulkErrorProcessor.ProcessAsync(ex, errorContext.ToProcessingErrorContext(), configureAwait, token).ConfigureAwait(configureAwait);
			policyResult.AddBulkProcessorErrors(bulkProcessResult);
			if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
			{
				policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
				return false;
			}

			var (accepted, canceled, error) = await RunPolicyRuleFuncAsync(errorContext, policyRuleFunc, configureAwait, token).ConfigureAwait(configureAwait);
			if (!accepted)
			{
				policyResult.HandlePolicyRuleFailure(error, canceled, ex);
				return false;
			}
			return true;
		}

		public static Task<bool> TryEvaluateRuleThenProcessExceptionAsync<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			bool configureAwait,
			CancellationToken token)
		{
			return TryEvaluateRuleThenProcessExceptionAsync(
				ex,
				policyResult,
				errorContext,
				PolicyProcessor.AsyncErrorSaver<T>.Default,
				policyRuleFunc,
				bulkErrorProcessor,
				cancellationEffect,
				configureAwait,
				token);
		}

		public static async Task<bool> TryEvaluateRuleThenProcessExceptionAsync<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> errorSaver,
			Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			bool configureAwait,
			CancellationToken token)
		{
			await errorSaver(policyResult, ex, errorContext, configureAwait, token).ConfigureAwait(configureAwait);
			if (token.IsCancellationRequested)
			{
				policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
				return false;
			}

			var (accepted, canceled, error) = await RunPolicyRuleFuncAsync(errorContext, policyRuleFunc, configureAwait, token).ConfigureAwait(configureAwait);
			if (!accepted)
			{
				policyResult.HandlePolicyRuleFailure(error, canceled, ex);
				return false;
			}

			var bulkProcessResult = await bulkErrorProcessor.ProcessAsync(ex, errorContext.ToProcessingErrorContext(), configureAwait, token).ConfigureAwait(configureAwait);
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

		private static async Task<(bool Result, bool IsCanceled, Exception error)> RunPolicyRuleFuncAsync<T>(ErrorContext<T> errorContext, Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc, bool configureAwait, CancellationToken token)
		{
			try
			{
				if (policyRuleFunc is null)
					return (true, false, null);
				var result = await policyRuleFunc(errorContext, token).ConfigureAwait(configureAwait);
				return (result, false, null);
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
