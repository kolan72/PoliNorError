using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class PolicyResultExtensions
	{
		internal static void AddBulkProcessorErrors(this PolicyResult policyResult, BulkErrorProcessor.BulkProcessResult bulkProcessResult)
		{
			policyResult.AddCatchBlockErrors(bulkProcessResult.ToCatchBlockExceptions());
		}

		internal static void ChangeByHandleCatchBlockResult(this PolicyResult policyResult, HandleCatchBlockResult canHandleResult)
		{
			switch (canHandleResult)
			{
				case HandleCatchBlockResult.FailedByPolicyRules:
					policyResult.SetFailedInner();
					break;
				case HandleCatchBlockResult.FailedByErrorFilter:
					policyResult.SetFailedAndFilterUnsatisfied();
					break;
				case HandleCatchBlockResult.Canceled:
					policyResult.SetFailedAndCanceled();
					break;
			}
		}

		internal static bool NotFailedOrCanceled(this PolicyResult prevResult)
		{
			return prevResult != null && (prevResult.IsCanceled || !prevResult.IsFailed);
		}

		internal static T SetFailedWithError<T>(this T retryResult, Exception exception, PolicyResultFailedReason failedReason = PolicyResultFailedReason.PolicyHandleGuardsFailed) where T : PolicyResult
		{
			retryResult.AddError(exception);
			retryResult.SetFailedInner(failedReason);
			return retryResult;
		}

		internal static void SetFailedWithCatchBlockError(this PolicyResult result, Exception processException, Exception handlingException, bool isCritical = false)
		{
			result.AddCatchBlockError(new CatchBlockException(processException, handlingException, isCritical));
			result.SetFailedInner();
		}

		internal async static Task<PolicyResult> HandleResultMisc(this PolicyResult policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, bool configureAwait, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					if (handler.UseSync)
					{
						handler.Run(curRes, token);
					}
					else
					{
					   await handler.RunAsync(curRes, token).ConfigureAwait(configureAwait);
					}
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex, handler);
				}
			}
			return curRes;
		}

		internal async static Task<PolicyResult<T>> HandleResultMisc<T>(this PolicyResult<T> policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, bool configureAwait, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					if (handler.UseSync)
					{
						handler.Run(curRes, token);
					}
					else
					{
						await handler.RunAsync(curRes, token).ConfigureAwait(configureAwait);
					}
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex, handler);
				}
			}
			return curRes;
		}

		internal async static Task<PolicyResult> HandleResultAsync(this PolicyResult policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, bool configureAwait, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					await handler.RunAsync(curRes, token).ConfigureAwait(configureAwait);
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex, handler);
				}
			}
			return curRes;
		}

		internal static PolicyResult HandleResultSync(this PolicyResult policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					handler.Run(curRes, token);
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex, handler);
				}
			}
			return curRes;
		}

		internal static PolicyResult<T> HandleResultSync<T>(this PolicyResult<T> policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					handler.Run(curRes, token);
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex, handler);
				}
			}
			return curRes;
		}

		internal static PolicyResult HandleResultForceSync(this PolicyResult policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					if (handler.UseSync)
					{
					   handler.Run(curRes, token);
					}
					else
					{
						Task.Run(() => handler.RunAsync(curRes, token), token).Wait(token);
					}
				}
				catch (AggregateException ae)
				{
					if (ae.HasCanceledException(token))
					{
						curRes.AddWrappedHandleResultError(new OperationCanceledException(token), handler);
					}
					else
					{
						curRes.AddWrappedHandleResultErrors(ae.InnerExceptions, handler);
					}
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex, handler);
				}
			}
			return curRes;
		}

		internal static void AddWrappedHandleResultError(this PolicyResult policyResult, Exception ex, IHandlerRunner handler)
		{
			policyResult.AddHandleResultError(new HandlePolicyResultException(ex, handler));
		}

		internal static void AddWrappedHandleResultErrors(this PolicyResult policyResult, IEnumerable<Exception> exs, IHandlerRunner handler)
		{
			var handlePolicyResultErrors = exs.Select((ex) => new HandlePolicyResultException(ex, handler));
			policyResult.AddHandleResultErrors(handlePolicyResultErrors);
		}
	}
}
