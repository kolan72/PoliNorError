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

		private static readonly Func<RetryCountInfo, ErrorContext<RetryContext>, CancellationToken, bool> _policyRuleFunc = (retryCountInfo, exCtx, _) => retryCountInfo.CanRetry(exCtx.Context.CurrentRetryCount);

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

		public PolicyResult<T> Retry<T>(Func<T> func, RetryCountInfo retryCountInfo, CancellationToken token = default)
		{
			return RetryInternal(func, retryCountInfo, null, _retryErrorContextCreator, token);
		}

		public Task<PolicyResult> RetryAsync(Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, _retryErrorContextCreator, configureAwait, token);
		}

		public Task<PolicyResult<T>> RetryAsync<T>(Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, bool configureAwait = false, CancellationToken token = default)
		{
			return RetryInternalAsync(func, retryCountInfo, null, _retryErrorContextCreator, configureAwait, token);
		}

		internal PolicyResult RetryInternal(Action action, RetryCountInfo retryCountInfo, RetryDelay retryDelay, Func<int, RetryErrorContext> retryErrorContextCreator, CancellationToken token)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			bool PolicyRule(ErrorContext<RetryContext> ctx, CancellationToken ct) => _policyRuleFunc(retryCountInfo, ctx, ct);

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
					result.SetFailedAndCanceled(ae.GetCancellationException(token));
				}
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && HandleException(ex, result, retryContext, PolicyRule, retryDelay, token))
					{
						retryContext.IncrementCount();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal PolicyResult RetryInternal<TParam>(Action<TParam> action, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			bool PolicyRule(ErrorContext<RetryContext> ctx, CancellationToken ct) => _policyRuleFunc(retryCountInfo, ctx, ct);

			var retryContext = new RetryErrorContext<TParam>(param, retryCountInfo.StartTryCount);

			do
			{
				try
				{
					action(param);
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
					result.SetFailedAndCanceled(ae.GetCancellationException(token));
				}
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && HandleException(ex, result, retryContext, PolicyRule, retryDelay, token))
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
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			bool PolicyRule(ErrorContext<RetryContext> ctx, CancellationToken ct) => _policyRuleFunc(retryCountInfo, ctx, ct);

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
					result.SetFailedAndCanceled(ae.GetCancellationException(token));
				}
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && HandleException(ex, result, retryContext, PolicyRule, retryDelay, token))
					{
						retryContext.IncrementCount();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal PolicyResult<T> RetryInternal<TParam, T>(Func<TParam, T> func, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, CancellationToken token)
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
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			bool PolicyRule(ErrorContext<RetryContext> ctx, CancellationToken ct) => _policyRuleFunc(retryCountInfo, ctx, ct);

			var retryContext = new RetryErrorContext<TParam>(param, retryCountInfo.StartTryCount);
			do
			{
				try
				{
					var res = func(param);
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
					result.SetFailedAndCanceled(ae.GetCancellationException(token));
				}
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && HandleException(ex, result, retryContext, PolicyRule, retryDelay, token))
					{
						retryContext.IncrementCount();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal async Task<PolicyResult> RetryInternalAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			Task<bool> PolicyRuleAsync(ErrorContext<RetryContext> context, CancellationToken ct) => Task.FromResult(_policyRuleFunc(retryCountInfo, context, ct));

			var retryContext = new RetryErrorContext<TParam>(param, retryCountInfo.StartTryCount);

			do
			{
				try
				{
					await func(param, token).ConfigureAwait(configureAwait);
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
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && await HandleExceptionAsync(ex, result, retryContext, PolicyRuleAsync, retryDelay, configureAwait, token).ConfigureAwait(configureAwait))
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
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			Task<bool> PolicyRuleAsync(ErrorContext<RetryContext> context, CancellationToken ct) => Task.FromResult(_policyRuleFunc(retryCountInfo, context, ct));

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
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && await HandleExceptionAsync(ex, result, retryContext, PolicyRuleAsync, retryDelay, configureAwait, token).ConfigureAwait(configureAwait))
					{
						retryContext.IncrementCount();
					}
				}
			}
			while (!result.IsFailed);
			return result;
		}

		internal async Task<PolicyResult<T>> RetryInternalAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, RetryCountInfo retryCountInfo, RetryDelay retryDelay, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);
			if (token.IsCancellationRequested)
			{
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			Task<bool> PolicyRuleAsync(ErrorContext<RetryContext> context, CancellationToken ct) => Task.FromResult(_policyRuleFunc(retryCountInfo, context, ct));

			var retryContext = new RetryErrorContext<TParam>(param, retryCountInfo.StartTryCount);

			do
			{
				try
				{
					var res = await func(param, token).ConfigureAwait(configureAwait);
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
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && await HandleExceptionAsync(ex, result, retryContext, PolicyRuleAsync, retryDelay, configureAwait, token).ConfigureAwait(configureAwait))
					{
						retryContext.IncrementCount();
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
				result.SetCanceledEarly();
				return result;
			}

			result.SetExecuted();

			result.ErrorsNotUsed = ErrorsNotUsed;

			Task<bool> PolicyRuleAsync(ErrorContext<RetryContext> context, CancellationToken ct) => Task.FromResult(_policyRuleFunc(retryCountInfo, context, ct));

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
				catch (Exception ex) when (!ShouldPropagateFilterUnsatisfied(ex, false, result, out bool filterAccepted))
				{
					if (filterAccepted && await HandleExceptionAsync(ex, result, retryContext, PolicyRuleAsync, retryDelay, configureAwait, token).ConfigureAwait(configureAwait))
					{
						retryContext.IncrementCount();
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


		private async Task<bool> HandleExceptionAsync(
							Exception ex,
							PolicyResult policyResult,
							ErrorContext<RetryContext> errorContext,
							Func<ErrorContext<RetryContext>, CancellationToken, Task<bool>> policyRuleFunc,
							RetryDelay retryDelay,
							bool configureAwait,
							CancellationToken token)
		{
			await HandleExceptionAsync(
				ex,
				policyResult,
				errorContext,
				SaveErrorAsync,
				policyRuleFunc,
				ExceptionHandlingBehavior.Handle,
				ProcessingOrder.EvaluateThenProcess,
				ErrorProcessingCancellationEffect.Propagate,
				configureAwait,
				token).ConfigureAwait(configureAwait);

			if (policyResult.IsFailed)
				return false;

			await DelayProvider.DelayAndCheckIfResultFailedAsync(retryDelay?.GetDelay(errorContext.Context.CurrentRetryCount), policyResult, ex, configureAwait, token).ConfigureAwait(configureAwait);
			return !policyResult.IsFailed;
		}

		private bool HandleException(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<RetryContext> errorContext,
			Func<ErrorContext<RetryContext>, CancellationToken, bool> policyRuleFunc,
			RetryDelay retryDelay,
			CancellationToken token)
		{
			HandleException(
				ex,
				policyResult,
				errorContext,
				SaveError,
				policyRuleFunc,
				ExceptionHandlingBehavior.Handle,
				ProcessingOrder.EvaluateThenProcess,
				ErrorProcessingCancellationEffect.Propagate,
				token);

			if (policyResult.IsFailed)
				return false;

			DelayProvider.DelayAndCheckIfResultFailed(retryDelay?.GetDelay(errorContext.Context.CurrentRetryCount), policyResult, ex, token);
			return !policyResult.IsFailed;
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
