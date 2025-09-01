using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class DefaultRetryProcessor : PolicyProcessor, IRetryProcessor, ICanAddErrorFilter<DefaultRetryProcessor>
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

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult Retry<TParam>(Action<TParam> action, TParam param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryWithErrorContext(action.Apply(param), param, retryCountInfo, token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, int retryCount, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult<T> Retry<TParam, T>(Func<TParam, T> func, TParam param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryWithErrorContext(func.Apply(param), param, retryCountInfo, token);
		}

		public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(func, retryCountInfo, null, _retryErrorContextCreator, token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult RetryWithErrorContext<TErrorContext>(Action action, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(action, retryCountInfo, null, retryErrorContextCreator, token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, int retryCount, CancellationToken token = default)
		{
			return RetryWithErrorContext(func, param, RetryCountInfo.Limited(retryCount), token);
		}

		public PolicyResult<T> RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return RetryInternal(func, retryCountInfo, null, retryErrorContextCreator, token);
		}

		public PolicyResult RetryInfiniteWithErrorContext<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult<T> RetryInfiniteWithErrorContext<TErrorContext, T>(Func<T> action, TErrorContext param, CancellationToken token = default)
		{
			return RetryWithErrorContext(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult RetryInfinite<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			return Retry(action, param, RetryCountInfo.Infinite(), token);
		}

		public PolicyResult<T> RetryInfinite<TParam, T>(Func<TParam, T> func, TParam param, CancellationToken token = default)
		{
			return Retry(func, param, RetryCountInfo.Infinite(), token);
		}

		public Task<PolicyResult> RetryInfiniteWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}

		public Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public async Task<PolicyResult> RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return await RetryInternalAsync(func, retryCountInfo, null, retryErrorContextCreator, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public async Task<PolicyResult<T>> RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			var retryErrorContextCreator = GetRetryErrorContextCreator<TErrorContext>().Apply(param);
			return await RetryInternalAsync(func, retryCountInfo, null, retryErrorContextCreator, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func.Apply(param), param, retryCountInfo, configureAwait, token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, _retryErrorContextCreator, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, int retryCount, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Limited(retryCount), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryWithErrorContextAsync(func.Apply(param), param, retryCountInfo, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, _retryErrorContextCreator, configureAwait, token);
		}

		public Task<PolicyResult> RetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param,  bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryAsync(func, param, RetryCountInfo.Infinite(), configureAwait, token);
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

			result.SetExecuted();

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
					if (result.UnprocessedError != null)
						result.UnprocessedError = null;
					break;
				}
				catch (OperationCanceledException oe) when (token.IsCancellationRequested)
				{
					result.SetFailedAndCanceled(oe);
				}
				catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
				{
					result.SetFailedAndCanceled(ae.GetCancellationException());
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

		internal PolicyResult<T> RetryInternal<T>(Func<T> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, Func<int, RetryErrorContext> retryErrorContextCreator, CancellationToken token)
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

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockSyncHandler(result, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = retryErrorContextCreator(retryCountInfo.StartTryCount);
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
				catch (OperationCanceledException oe) when (token.IsCancellationRequested)
				{
					result.SetFailedAndCanceled(oe);
				}
				catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
				{
					result.SetFailedAndCanceled(ae.GetCancellationException());
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

		internal async Task<PolicyResult> RetryInternalAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, Func<int, RetryErrorContext> retryErrorContextCreator, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockAsyncHandler(result, configureAwait, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = retryErrorContextCreator(retryCountInfo.StartTryCount);
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
				catch (OperationCanceledException oe) when (token.IsCancellationRequested)
				{
					result.SetFailedAndCanceled(oe);
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

		internal async Task<PolicyResult<T>> RetryInternalAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, RetryDelay retryDelay, Func<int, RetryErrorContext> retryErrorContextCreator, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);
			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			var handler = GetCatchBlockAsyncHandler(result, configureAwait, token, _policyRuleFunc.Apply(retryCountInfo));

			var retryContext = retryErrorContextCreator(retryCountInfo.StartTryCount);
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
				catch (OperationCanceledException oe) when (token.IsCancellationRequested)
				{
					result.SetFailedAndCanceled(oe);
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

		public DefaultRetryProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultRetryProcessor, TErrorContext>(actionProcessor);
		}

		public DefaultRetryProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultRetryProcessor, TErrorContext>(funcProcessor);
		}

		public DefaultRetryProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			return this.WithErrorContextProcessorOf<DefaultRetryProcessor, TErrorContext>(funcProcessor, cancellationType);
		}

		public DefaultRetryProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultRetryProcessor, TErrorContext>(funcProcessor);
		}

		public DefaultRetryProcessor WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			return this.WithErrorContextProcessor<DefaultRetryProcessor, TErrorContext>(errorProcessor);
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
						&& !result.WasResultSetToFailureByCatchBlock(handler
																.Handle(ex, retryContext))
						&& !DelayProvider.DelayAndCheckIfResultFailed(retryDelay?.GetDelay(retryContext.Context.CurrentRetryCount), result, ex, token);
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
						&& !result.WasResultSetToFailureByCatchBlock(await handler
																.HandleAsync(ex, retryContext).ConfigureAwait(configureAwait))
						&& !await DelayProvider.DelayAndCheckIfResultFailedAsync(retryDelay?.GetDelay(retryContext.Context.CurrentRetryCount), result, ex, configureAwait, token).ConfigureAwait(configureAwait);
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

		///<inheritdoc cref = "ICanAddErrorFilter{DefaultRetryProcessor}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public DefaultRetryProcessor AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			this.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		///<inheritdoc cref = "ICanAddErrorFilter{DefaultRetryProcessor}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public DefaultRetryProcessor AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			this.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}
	}
}
