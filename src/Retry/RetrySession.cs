using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal abstract class RetrySessionBase
	{
		private readonly DefaultRetryProcessor _processor;
		private readonly Func<Exception, bool> _errorFilterPredicate;
		private readonly Func<ErrorContext<RetryContext>, CancellationToken, Task<bool>> _policyRuleAsync;
		private readonly RetryDelay _retryDelay;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private int _terminalHandlerState;
		private Task _terminalHandlerTask;

		protected RetrySessionBase(
			DefaultRetryProcessor processor,
			PolicyResult result,
			RetryCountInfo retryCountInfo,
			RetryDelay retryDelay,
			Func<int, RetryErrorContext> retryErrorContextCreator,
			bool configureAwait,
			CancellationToken token,
			bool linkCancellationToken)
		{
			_processor = processor ?? throw new ArgumentNullException(nameof(processor));
			Result = result ?? throw new ArgumentNullException(nameof(result));
			_retryDelay = retryDelay;
			ConfigureAwait = configureAwait;

			if (linkCancellationToken)
			{
				_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
				Token = _cancellationTokenSource.Token;
			}
			else
			{
				Token = token;
			}

			if (Token.IsCancellationRequested)
			{
				Result.SetCanceledEarly();
				IsFinished = true;
				return;
			}

			Result.SetExecuted();
			Result.ErrorsNotUsed = processor.ErrorsNotUsed;
			RetryContext = retryErrorContextCreator(retryCountInfo.StartTryCount);
			_policyRuleAsync = DefaultRetryProcessor.GetAsyncPolicyRule(retryCountInfo);
			_errorFilterPredicate = processor.ErrorFilter.GetCanHandle();
		}

		internal PolicyResult Result { get; }

		internal CancellationToken Token { get; }

		internal bool ConfigureAwait { get; }

		internal RetryErrorContext RetryContext { get; private set; }

		internal bool IsFinished { get; set; }

		internal Func<Task> TerminalHandler { get; set; }

		protected DefaultRetryProcessor Processor => _processor;

		protected Func<ErrorContext<RetryContext>, CancellationToken, Task<bool>> PolicyRuleAsync => _policyRuleAsync;

		protected Func<Exception, bool> ErrorFilterPredicate => _errorFilterPredicate;

		internal abstract Task<RetryStepOutcome> StepAsync(bool configureAwait, CancellationToken token);

		internal async Task<PolicyResult> RunUntilTerminalAsync(bool configureAwait, CancellationToken token)
		{
			while (true)
			{
				var outcome = await StepAsync(configureAwait, token).ConfigureAwait(configureAwait);
				if (outcome.IsTerminal)
				{
					return Result;
				}
				if (await _processor.DelayProvider
						.DelayAndCheckIfResultFailedAsync(outcome.Delay, Result, outcome.LastError, configureAwait, token)
						.ConfigureAwait(configureAwait))
				{
					return Result;
				}
				IncrementCount();
			}
		}

		internal TimeSpan? GetNextDelay() => _retryDelay?.GetDelay(RetryContext.Context.CurrentRetryCount);

		internal void IncrementCount() => RetryContext.IncrementCount();

		internal void ApplyCancellation()
		{
			Result.SetFailedAndCanceled(new OperationCanceledException(Token));
			IsFinished = true;
		}

		internal void OnWorkerFault(Exception ex)
		{
			Result.SetFailedWithCatchBlockError(ex, null, CatchBlockExceptionSource.RetryQueueWorker);
			IsFinished = true;
		}

		internal void CancelSession()
		{
			try
			{
				_cancellationTokenSource?.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (AggregateException)
			{
			}
		}

		internal Task RunTerminalHandlerAsync()
		{
			if (Interlocked.CompareExchange(ref _terminalHandlerState, 1, 0) == 0 && TerminalHandler != null)
			{
				_terminalHandlerTask = TerminalHandler();
			}
			return _terminalHandlerTask ?? Task.CompletedTask;
		}

		internal void DisposeCancellation()
		{
			_cancellationTokenSource?.Dispose();
		}
	}

	internal sealed class RetrySession : RetrySessionBase
	{
		private readonly Func<CancellationToken, Task> _func;

		private RetrySession(
			Func<CancellationToken, Task> func,
			DefaultRetryProcessor processor,
			PolicyResult result,
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

		internal static RetrySession Create(
			Func<CancellationToken, Task> func,
			DefaultRetryProcessor processor,
			RetryCountInfo retryCountInfo,
			RetryDelay retryDelay,
			Func<int, RetryErrorContext> retryErrorContextCreator,
			bool configureAwait,
			CancellationToken token,
			bool linkCancellationToken = false)
		{
			var result = PolicyResult.InitByConfigureAwait(configureAwait, DefaultRetryProcessor.GetErrorsCapacity(retryCountInfo));
			return new RetrySession(func, processor, result, retryCountInfo, retryDelay, retryErrorContextCreator, configureAwait, token, linkCancellationToken);
		}

		internal override async Task<RetryStepOutcome> StepAsync(bool configureAwait, CancellationToken token)
		{
			if (IsFinished)
			{
				return RetryStepOutcome.Terminal;
			}

			try
			{
				await _func(token).ConfigureAwait(configureAwait);
				if (RetryContext.Context.IsZeroRetry)
				{
					Result.SetOk();
				}
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