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

		internal static T SetFailedWithError<T>(this T retryResult, Exception exception, PolicyResultFailedReason failedReason = PolicyResultFailedReason.DelegateIsNull) where T : PolicyResult
		{
			retryResult.AddError(exception);
			retryResult.SetFailedInner(failedReason);
			return retryResult;
		}

		internal static T SetPolicyName<T>(this T retryResult, string policyName) where T : PolicyResult
		{
			retryResult.PolicyName = policyName;
			return retryResult;
		}

		internal static void SetFailedWithCatchBlockError(this PolicyResult result, Exception processException, Exception handlingException, CatchBlockExceptionSource errorSource = CatchBlockExceptionSource.Unknown)
		{
			result.AddCatchBlockError(new CatchBlockException(processException, handlingException, errorSource, true));
			result.SetFailedInner();
		}

		internal static T WithNoDelegateException<T>(this T retryResult) where T : PolicyResult
		{
			return retryResult.SetFailedWithError(new NoDelegateException());
		}

		internal async static Task<PolicyResult> HandleResultMisc(this PolicyResult policyRetryResult, IEnumerable<IHandlerRunner> handlerRunners, bool configureAwait, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					if (handler.SyncRun)
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
					curRes.AddWrappedHandleResultError(ex);
				}
			}
			return curRes;
		}

		internal async static Task<PolicyResult<T>> HandleResultMisc<T>(this PolicyResult<T> policyRetryResult, IEnumerable<IHandlerRunnerT> handlerRunners, bool configureAwait, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					if (handler.SyncRun)
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
					curRes.AddWrappedHandleResultError(ex);
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
					curRes.AddWrappedHandleResultError(ex);
				}
			}
			return curRes;
		}

		internal async static Task<PolicyResult<T>> HandleResultAsync<T>(this PolicyResult<T> policyRetryResult, IEnumerable<IHandlerRunnerT> handlerRunners, bool configureAwait, CancellationToken token)
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
					curRes.AddWrappedHandleResultError(ex);
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
					curRes.AddWrappedHandleResultError(ex);
				}
			}
			return curRes;
		}

		internal static PolicyResult<T> HandleResultSync<T>(this PolicyResult<T> policyRetryResult, IEnumerable<IHandlerRunnerT> handlerRunners, CancellationToken token)
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
					curRes.AddWrappedHandleResultError(ex);
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
					if (handler.SyncRun)
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
						curRes.AddWrappedHandleResultError(new OperationCanceledException(token));
					}
					else
					{
						curRes.AddWrappedHandleResultErrors(ae.InnerExceptions);
					}
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex);
				}
			}
			return curRes;
		}

		internal static PolicyResult<T> HandleResultForceSync<T>(this PolicyResult<T> policyRetryResult, IEnumerable<IHandlerRunnerT> handlerRunners, CancellationToken token)
		{
			var curRes = policyRetryResult;
			foreach (var handler in handlerRunners)
			{
				try
				{
					if (handler.SyncRun)
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
						curRes.AddWrappedHandleResultError(new OperationCanceledException(token));
					}
					else
					{
						curRes.AddWrappedHandleResultErrors(ae.InnerExceptions);
					}
				}
				catch (Exception ex)
				{
					curRes.AddWrappedHandleResultError(ex);
				}
			}
			return curRes;
		}

		internal static void AddWrappedHandleResultError(this PolicyResult policyResult, Exception ex)
		{
			policyResult.AddHandleResultError(new PolicyResultHandlingException(ex));
		}

		internal static void AddWrappedHandleResultErrors(this PolicyResult policyResult, IEnumerable<Exception> exs)
		{
			var handlePolicyResultErrors = exs.Select((ex) => new PolicyResultHandlingException(ex));
			policyResult.AddHandleResultErrors(handlePolicyResultErrors);
		}

		internal static PolicyResult SetWrappedPolicyResults(this PolicyResult policyResult, PolicyWrapper wrapper)
		{
			if (wrapper != null)
			{
				policyResult.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}
			return policyResult;
		}

		internal static PolicyResult<T> SetWrappedPolicyResults<T>(this PolicyResult<T> policyResult, PolicyWrapper<T> wrapper)
		{
			if (wrapper != null)
			{
				policyResult.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}
			return policyResult;
		}
	}
}
