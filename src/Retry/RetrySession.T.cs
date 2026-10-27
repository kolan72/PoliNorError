using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class RetrySession<T> : RetrySessionBase
	{
		private readonly Func<CancellationToken, Task<T>> _func;

		private RetrySession(
			Func<CancellationToken, Task<T>> func,
			DefaultRetryProcessor processor,
			PolicyResult<T> result,
			RetryCountInfo retryCountInfo,
			RetryDelay retryDelay,
			Func<int, RetryErrorContext> retryErrorContextCreator,
			bool configureAwait,
			CancellationToken token,
			bool linkCancellationToken)
			: base(processor, result, retryCountInfo, retryDelay, retryErrorContextCreator, configureAwait, token, linkCancellationToken)
		{
			_func = func;
		}

		internal static RetrySession<T> Create(
			Func<CancellationToken, Task<T>> func,
			DefaultRetryProcessor processor,
			RetryCountInfo retryCountInfo,
			RetryDelay retryDelay,
			Func<int, RetryErrorContext> retryErrorContextCreator,
			bool configureAwait,
			CancellationToken token,
			bool linkCancellationToken = false)
		{
			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait, DefaultRetryProcessor.GetErrorsCapacity(retryCountInfo));
			return new RetrySession<T>(func, processor, result, retryCountInfo, retryDelay, retryErrorContextCreator, configureAwait, token, linkCancellationToken);
		}

		internal new PolicyResult<T> Result => (PolicyResult<T>)base.Result;

		internal new async Task<PolicyResult<T>> RunUntilTerminalAsync(bool configureAwait, CancellationToken token)
			=> (PolicyResult<T>)await base.RunUntilTerminalAsync(configureAwait, token).ConfigureAwait(configureAwait);

		internal override async Task<RetryStepOutcome> StepAsync(bool configureAwait, CancellationToken token)
		{
			if (IsFinished)
			{
				return RetryStepOutcome.Terminal;
			}

			try
			{
				var res = await _func(token).ConfigureAwait(configureAwait);
				if (RetryContext.Context.IsZeroRetry)
				{
					Result.SetOk();
				}
				Result.SetResult(res);
				if (Result.UnprocessedError != null)
				{
					Result.UnprocessedError = null;
				}
				IsFinished = true;
				return RetryStepOutcome.Terminal;
			}
			catch (OperationCanceledException oe) when (token.IsCancellationRequested)
			{
				Result.SetFailedAndCanceled(oe);
				IsFinished = true;
				return RetryStepOutcome.Terminal;
			}
			catch (Exception ex) when (!Processor.ShouldPropagateFilterUnsatisfied(ErrorFilterPredicate, ex, false, Result, out bool filterAccepted))
			{
				if (filterAccepted &&
					await Processor.TryHandleExceptionCoreAsync(ex, Result, RetryContext, PolicyRuleAsync, configureAwait, token).ConfigureAwait(configureAwait))
				{
					return RetryStepOutcome.WaitAndContinue(GetNextDelay(), ex);
				}
				IsFinished = true;
				return RetryStepOutcome.Terminal;
			}
		}
	}
}