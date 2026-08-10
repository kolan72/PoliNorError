using System;
using System.Threading;

namespace PoliNorError
{
	internal static class BasicHandler
	{
		public static void EvaluateRuleThenProcessException<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc,
			IBulkErrorProcessor bulkErrorProcessor,
			ErrorProcessingCancellationEffect cancellationEffect,
			CancellationToken token)
		{
			EvaluateRuleThenProcessException(
				ex,
				policyResult,
				errorContext,
				PolicyProcessor.ErrorSaver<T>.Default,
				policyRuleFunc,
				bulkErrorProcessor,
				cancellationEffect,
				token);
		}

		public static void EvaluateRuleThenProcessException<T>(
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
				return;
			}

			var ruleResult = EvaluatePolicyRule(ex, policyResult, errorContext, policyRuleFunc, token);
			if (ruleResult != ExceptionHandlingResult.Accepted)
			{
				return;
			}

			var bulkProcessResult = bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
			policyResult.AddBulkProcessorErrors(bulkProcessResult);
			if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
			{
				policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
			}
		}

		/// <summary>
		/// Evaluates a policy rule synchronously to determine if an exception should be handled.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <param name="ex">The exception to evaluate.</param>
		/// <param name="policyResult">The policy result to update based on the rule evaluation.</param>
		/// <param name="errorContext">The error context containing additional information about the exception.</param>
		/// <param name="policyRuleFunc">The policy rule function to evaluate.</param>
		/// <param name="token">The cancellation token.</param>
		/// <returns>The exception handling result based on the policy rule evaluation.</returns>
		private static ExceptionHandlingResult EvaluatePolicyRule<T>(Exception ex, PolicyResult policyResult, ErrorContext<T> errorContext, Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc, CancellationToken token)
		{
			return HandlePolicyRuleFuncResult(RunPolicyRuleFunc(), ex, policyResult);

			(bool Result, bool IsCanceled, Exception error) RunPolicyRuleFunc()
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

		/// <summary>
		/// Handles the result of a policy rule function evaluation and updates the policy result accordingly.
		/// </summary>
		/// <param name="result">The result from the policy rule function evaluation.</param>
		/// <param name="ex">The original exception that was evaluated.</param>
		/// <param name="policyResult">The policy result to update based on the rule evaluation.</param>
		/// <returns>The exception handling result based on the policy rule function evaluation.</returns>
		private static ExceptionHandlingResult HandlePolicyRuleFuncResult((bool accepted, bool canceled, Exception error) result, Exception ex, PolicyResult policyResult)
		{
			if (result.accepted)
			{
				return ExceptionHandlingResult.Accepted;
			}
			else
			{
				if (!(result.error is null))
				{
					if (result.canceled)
					{
						policyResult.SetFailedAndCanceled((OperationCanceledException)result.error);
						policyResult.AddCatchBlockError(new CatchBlockException(result.error, ex, CatchBlockExceptionSource.PolicyRule));
					}
					else
					{
						policyResult.SetFailedWithCatchBlockError(result.error, ex, CatchBlockExceptionSource.PolicyRule);
					}
				}
				else
				{
					policyResult.SetFailedInner();
				}

				return ExceptionHandlingResult.Handled;
			}
		}
	}
}
