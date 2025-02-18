using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class DefaultRetryProcessor : PolicyProcessor, IRetryProcessor
	{
		private IErrorProcessor _saveErrorProcessor;
		private readonly bool _failedIfSaveErrorThrows;
		private IDelayProvider _delayProvider;

		private static readonly Func<int, RetryErrorContext> _retryErrorContextCreator = (tryCount) => new RetryErrorContext(tryCount);

		private static readonly Func<RetryCountInfo, ErrorContext<RetryContext>, bool> _policyRuleFunc = (retryCountInfo, exCtx) => retryCountInfo.CanRetry(exCtx.Context.CurrentRetryCount);

		public DefaultRetryProcessor(bool failedIfSaveErrorThrows = false) : this(null, failedIfSaveErrorThrows) { }

		public DefaultRetryProcessor(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false)
			: this(bulkErrorProcessor, failedIfSaveErrorThrows, null)
		{}

		internal DefaultRetryProcessor(IDelayProvider delayProvider): this(null, false, delayProvider) {}

		internal DefaultRetryProcessor(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows, IDelayProvider delayProvider = null)
			: base(bulkErrorProcessor)
		{
			_failedIfSaveErrorThrows = failedIfSaveErrorThrows;
			_delayProvider = delayProvider;
		}

		public PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(action, retryCountInfo, null, _retryErrorContextCreator, token);
		}

		public PolicyResult Retry<TErrorContext>(Action action, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return Retry(action.Apply(param), param, retryCountInfo, token);
		}

		public PolicyResult Retry<TErrorContext>(Action action, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(action, retryCountInfo, null, retryErrorContextCreator, token);
		}

		public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(func, retryCountInfo, null, token);
		}

		public PolicyResult RetryInfiniteWithErrorContext<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult RetryInfinite<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Infinite(), token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, configureAwait, token);
		}

		internal PolicyResult RetryInternal(Action action, RetryCountInfo retryCountInfo, RetryDelay retryDelay, Func<int, RetryErrorContext> retryErrorContextCreator, CancellationToken token)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockSyncHandler(result, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				try
				{
					action();
					if (retryContext.Context.IsZeroRetry)
					{
						result.SetOk();
					}
					if(result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (AggregateException ae) when (ae.HasCanceledException(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					SaveError(result, ex, retryContext, token);
					if (HandleError(ex, result, retryDelay, handler, retryContext, token))
					{
						retryContext.IncrementCount();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal PolicyResult<T> RetryInternal<T>(Func<T> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			if (typeof(T) == typeof(Task) || typeof(T).IsSubclassOf(typeof(Task)))
			{
				throw new ArgumentException("Do not use this method for task return type!");
			}

			var result = PolicyResult<T>.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockSyncHandler(result, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				try
				{
					var res = func();
					if (retryContext.Context.IsZeroRetry)
					{
						result.SetOk();
					}
					result.SetResult(res);
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (AggregateException ae) when (ae.HasCanceledException(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					SaveError(result, ex, retryContext, token);
					if (HandleError(ex, result, retryDelay, handler, retryContext, token))
					{
						retryContext.IncrementCount();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal async Task<PolicyResult> RetryInternalAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockAsyncHandler(result, configureAwait, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				try
				{
					await func(token).ConfigureAwait(configureAwait);
					if (retryContext.Context.IsZeroRetry)
					{
						result.SetOk();
					}
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					await SaveErrorAsync(result, ex, retryContext, configureAwait, token).ConfigureAwait(configureAwait);

					if (await HandleErrorAsync(ex, result, retryDelay, handler, retryContext, configureAwait, token).ConfigureAwait(configureAwait))
					{
						retryContext.IncrementCountAtomic();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal async Task<PolicyResult<T>> RetryInternalAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);
			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockAsyncHandler(result, configureAwait, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				try
				{
					var res = await func(token).ConfigureAwait(configureAwait);
					if (retryContext.Context.IsZeroRetry)
					{
						result.SetOk();
					}
					result.SetResult(res);
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
				{
					result.SetFailedAndCanceled();
				}
				catch (Exception ex)
				{
					await SaveErrorAsync(result, ex, retryContext, configureAwait, token).ConfigureAwait(configureAwait);
					if (await HandleErrorAsync(ex, result, retryDelay, handler, retryContext, configureAwait, token).ConfigureAwait(configureAwait))
					{
						retryContext.IncrementCountAtomic();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		public IRetryProcessor UseCustomErrorSaver(IErrorProcessor saveErrorProcessor)
		{
			_saveErrorProcessor = saveErrorProcessor ?? throw new ArgumentNullException(nameof(saveErrorProcessor), "Custom error saver cannot be null.");
			return this;
		}

		public DefaultRetryProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultRetryProcessor, TErrorContext>(actionProcessor);
		}

		public DefaultRetryProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			return this.WithErrorContextProcessorOf<DefaultRetryProcessor, TErrorContext>(actionProcessor, cancellationType);
		}

		private IDelayProvider DelayProvider => _delayProvider ?? (_delayProvider = new DelayProvider());

		private bool HandleError(Exception ex,
							PolicyResult result,
							RetryDelay retryDelay,
							PolicyProcessorCatchBlockSyncHandler<RetryContext> handler,
							RetryErrorContext retryContext,
							CancellationToken token)
		{
			return !result.IsFailed
						&& result.ChangeByHandleCatchBlockResult(handler
																.Handle(ex, retryContext))
						&& !result.IsFailed
						&& result.ChangeByRetryDelayResult(DelayIfNeed(retryDelay, retryContext, token), ex)
						&& !result.IsFailed;
		}

		private async Task<bool> HandleErrorAsync(Exception ex,
							PolicyResult result,
							RetryDelay retryDelay,
							PolicyProcessorCatchBlockAsyncHandler<RetryContext> handler,
							RetryErrorContext retryContext,
							bool configureAwait,
							CancellationToken token)
		{
			return !result.IsFailed
						&& result.ChangeByHandleCatchBlockResult(await handler
																.HandleAsync(ex, retryContext).ConfigureAwait(configureAwait))
						&& !result.IsFailed
						&& result.ChangeByRetryDelayResult(await DelayIfNeedAsync(retryDelay, retryContext, configureAwait, token).ConfigureAwait(configureAwait), ex)
						&& !result.IsFailed;
		}

		private BasicResult DelayIfNeed(RetryDelay retryDelay, RetryErrorContext retryContext, CancellationToken token)
		{
			BasicResult res = null;
			var delay = retryDelay?.GetDelay(retryContext.Context.CurrentRetryCount);
			if (delay > TimeSpan.Zero)
			{
				res = DelayProvider.BackoffSafely(delay.Value, token);
			}
			return res;
		}

		private async Task<BasicResult> DelayIfNeedAsync(RetryDelay retryDelay, RetryErrorContext retryContext, bool configureAwait, CancellationToken token)
		{
			BasicResult res = null;
			var delay = retryDelay?.GetDelay(retryContext.Context.CurrentRetryCount);
			if (delay > TimeSpan.Zero)
			{
				res = await DelayProvider.BackoffSafelyAsync(delay.Value, configureAwait, token).ConfigureAwait(configureAwait);
			}
			return res;
		}

		private bool ErrorsNotUsed => _saveErrorProcessor != null;

		private void SaveError(PolicyResult result, Exception ex, ErrorContext<RetryContext> retryContext, CancellationToken token)
		{
			try
			{
				if (_saveErrorProcessor == null)
				{
					result.AddError(ex);
				}
				else
				{
					_saveErrorProcessor.Process(ex, new RetryProcessingErrorInfo(retryContext.Context.CurrentRetryCount), token);
					//We set it here to keep UnprocessedError from being lost.
					result.UnprocessedError = ex;
				}
			}
			catch (Exception exIn)
			{
				HandleSaveErrorProcessorException(result, exIn, ex);
			}
		}

		private async Task SaveErrorAsync(PolicyResult result, Exception ex, ErrorContext<RetryContext> retryContext, bool configureAwait, CancellationToken token)
		{
			try
			{
				if (_saveErrorProcessor == null)
				{
					result.AddError(ex);
				}
				else
				{
					await _saveErrorProcessor.ProcessAsync(ex, new RetryProcessingErrorInfo(retryContext.Context.CurrentRetryCount), configureAwait, token).ConfigureAwait(configureAwait);
					//We set it here to keep UnprocessedError from being lost.
					result.UnprocessedError = ex;
				}
			}
			catch (Exception exIn)
			{
				HandleSaveErrorProcessorException(result, exIn, ex);
			}
		}

		private void HandleSaveErrorProcessorException(PolicyResult result, Exception errorProcessorEx, Exception ex)
		{
			if (_failedIfSaveErrorThrows)
			{
				result.SetFailedWithCatchBlockError(errorProcessorEx, ex, CatchBlockExceptionSource.ErrorSaver);
			}
			else
			{
				result.AddCatchBlockError(new CatchBlockException(errorProcessorEx, ex, CatchBlockExceptionSource.ErrorSaver));
			}
			result.UnprocessedError = ex;
		}

		private static Func<TErrorContext, int, RetryErrorContext<TErrorContext>> GetRetryErrorContextCreator<TErrorContext>()
					=> (context, tryCount) => new RetryErrorContext<TErrorContext>(context, tryCount);
	}
}
