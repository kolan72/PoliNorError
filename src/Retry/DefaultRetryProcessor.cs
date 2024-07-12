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

		private readonly Func<int, RetryErrorContext> _retryErrorContextCreator;

		public DefaultRetryProcessor(bool failedIfSaveErrorThrows = false) : this(null, failedIfSaveErrorThrows) { }

		public DefaultRetryProcessor(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows = false)
			: this(bulkErrorProcessor, failedIfSaveErrorThrows, null)
		{}

		internal DefaultRetryProcessor(IDelayProvider delayProvider): this(null, false, delayProvider) {}

		internal DefaultRetryProcessor(IBulkErrorProcessor bulkErrorProcessor, bool failedIfSaveErrorThrows, IDelayProvider delayProvider = null)
			: base(PolicyAlias.Retry, bulkErrorProcessor)
		{
			_failedIfSaveErrorThrows = failedIfSaveErrorThrows;
			_retryErrorContextCreator = CreateRetryErrorContext;
			_delayProvider = delayProvider;
		}

		public PolicyResult Retry(Action action, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(action, retryCountInfo, null, token);
		}

		public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(func, retryCountInfo, null, token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, configureAwait, token);
		}

		internal PolicyResult RetryInternal(Action action, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			if (!(retryDelay is null) && _delayProvider is null)
			{
				_delayProvider = new DelayProvider();
			}

			var result = PolicyResult.ForSync();
			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockSyncHandler<RetryContext>(result, token, (exCtx) => retryCountInfo.CanRetry(exCtx.Context.CurrentRetryCount));

			int tryCount = retryCountInfo.StartTryCount;
			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					action();
					if (tryCount == retryCountInfo.StartTryCount)
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
					SaveError(result, ex, tryCount, token);
					if (result.IsFailed)
					{
						break;
					}

					result.ChangeByHandleCatchBlockResult(handler
														.Handle(ex, retryContext));
					if (!result.IsFailed)
					{
						if (!(retryDelay is null))
						{
							_delayProvider.Backoff(retryDelay.GetDelay(tryCount), token);
						}
						tryCount++;
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

			if (typeof(T).Equals(typeof(Task)) || typeof(T).IsSubclassOf(typeof(Task)))
			{
				throw new ArgumentException("Do not use this method for task return type!");
			}

			if (!(retryDelay is null) && _delayProvider is null)
			{
				_delayProvider = new DelayProvider();
			}

			var result = PolicyResult<T>.ForSync();
			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockSyncHandler<RetryContext>(result, token, (exCtx) => retryCountInfo.CanRetry(exCtx.Context.CurrentRetryCount));

			int tryCount = retryCountInfo.StartTryCount;
			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					var res = func();
					if (tryCount == retryCountInfo.StartTryCount)
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
					SaveError(result, ex, tryCount, token);
					if (result.IsFailed)
					{
						break;
					}
					result.ChangeByHandleCatchBlockResult(handler
														.Handle(ex, retryContext));
					if (!result.IsFailed)
					{
						if (!(retryDelay is null))
						{
							_delayProvider.Backoff(retryDelay.GetDelay(tryCount), token);
						}
						tryCount++;
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

			if (!(retryDelay is null) && _delayProvider is null)
			{
				_delayProvider = new DelayProvider();
			}

			var result = PolicyResult.InitByConfigureAwait(configureAwait);
			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockAsyncHandler<RetryContext>(result, configureAwait, token, (exCtx) => retryCountInfo.CanRetry(exCtx.Context.CurrentRetryCount));

			int tryCount = retryCountInfo.StartTryCount;
			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					await func(token).ConfigureAwait(configureAwait);
					if (tryCount == retryCountInfo.StartTryCount)
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
					await SaveErrorAsync(result, ex, tryCount, configureAwait, token).ConfigureAwait(configureAwait);
					if (result.IsFailed)
					{
						break;
					}
					result.ChangeByHandleCatchBlockResult(await handler.HandleAsync(ex, retryContext).ConfigureAwait(configureAwait));
					if (!result.IsFailed)
					{
						if (!(retryDelay is null))
						{
							await _delayProvider.BackoffAsync(retryDelay.GetDelay(tryCount), configureAwait, token).ConfigureAwait(configureAwait);
						}
						Interlocked.Increment(ref tryCount);
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

			if (!(retryDelay is null) && _delayProvider is null)
			{
				_delayProvider = new DelayProvider();
			}

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);
			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockAsyncHandler<RetryContext>(result, configureAwait, token, (exCtx) => retryCountInfo.CanRetry(exCtx.Context.CurrentRetryCount));

			int tryCount = retryCountInfo.StartTryCount;
			var retryContext = _retryErrorContextCreator(retryCountInfo.StartTryCount);
			do
			{
				if (token.IsCancellationRequested)
				{
					if (tryCount == retryCountInfo.StartTryCount)
						result.SetCanceled();
					else
						result.SetFailedAndCanceled();
					break;
				}
				try
				{
					var res = await func(token).ConfigureAwait(configureAwait);
					if (tryCount == retryCountInfo.StartTryCount)
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
					await SaveErrorAsync(result, ex, tryCount, configureAwait, token).ConfigureAwait(configureAwait);
					if (result.IsFailed)
					{
						break;
					}
					result.ChangeByHandleCatchBlockResult(await handler.HandleAsync(ex, retryContext).ConfigureAwait(configureAwait));
					if (!result.IsFailed)
					{
						if (!(retryDelay is null))
						{
							await _delayProvider.BackoffAsync(retryDelay.GetDelay(tryCount), configureAwait, token).ConfigureAwait(configureAwait);
						}
						Interlocked.Increment(ref tryCount);
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

		private RetryErrorContext CreateRetryErrorContext(int tryCount)
		{
			return new RetryErrorContext(tryCount);
		}

		private bool ErrorsNotUsed => _saveErrorProcessor != null;

		private void SaveError(PolicyResult result, Exception ex, int tryErrorCount, CancellationToken token)
		{
			try
			{
				if (_saveErrorProcessor == null)
				{
					result.AddError(ex);
				}
				else
				{
					_saveErrorProcessor.Process(ex, new RetryProcessingErrorInfo(tryErrorCount), token);
					//We set it here to keep UnprocessedError from being lost.
					result.UnprocessedError = ex;
				}
			}
			catch (Exception exIn)
			{
				HandleSaveErrorProcessorException(result, exIn, ex);
			}
		}

		private async Task SaveErrorAsync(PolicyResult result, Exception ex, int tryErrorCount, bool configureAwait, CancellationToken token)
		{
			try
			{
				if (_saveErrorProcessor == null)
				{
					result.AddError(ex);
				}
				else
				{
					await _saveErrorProcessor.ProcessAsync(ex, new RetryProcessingErrorInfo(tryErrorCount), configureAwait, token).ConfigureAwait(configureAwait);
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
	}
}
