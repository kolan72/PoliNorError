﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class DefaultFallbackProcessor : PolicyProcessor, IFallbackProcessor, ICanAddErrorFilter<DefaultFallbackProcessor>
	{
		private readonly EmptyErrorContext _emptyErrorContext;
		public DefaultFallbackProcessor(IBulkErrorProcessor bulkErrorProcessor = null) : base(bulkErrorProcessor)
		{
			_emptyErrorContext = EmptyErrorContext.DefaultFallback;
		}

		public PolicyResult Fallback(Action action, Action<CancellationToken> fallback, CancellationToken token = default)
		{
			return Fallback(action, fallback, _emptyErrorContext, token);
		}

		public PolicyResult Fallback<TParam>(Action<TParam> action, TParam param, Action<CancellationToken> fallback, CancellationToken token = default)
		{
			return Fallback(action.Apply(param), param, fallback, token);
		}

		public PolicyResult Fallback<TErrorContext>(Action action, TErrorContext param, Action<CancellationToken> fallback, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return Fallback(action, fallback, emptyContext, token);
		}

		private PolicyResult Fallback(Action action, Action<CancellationToken> fallback, EmptyErrorContext emptyErrorContext, CancellationToken token = default)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			PolicyResult result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				action();
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled(oe);
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled(ae.GetCancellationException());
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(GetCatchBlockSyncHandler<Unit>(result, token)
													.Handle(ex, emptyErrorContext));
				if (!result.IsFailed)
				{
					fallback.HandleAsFallback(token).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public PolicyResult<T> Fallback<T>(Func<T> func, Func<CancellationToken, T> fallback, CancellationToken token = default)
		{
			return Fallback(func, fallback, _emptyErrorContext, token);
		}

		public PolicyResult<T> Fallback<TParam, T>(Func<TParam, T> func, TParam param, Func<CancellationToken, T> fallback, CancellationToken token = default)
		{
			return Fallback(func.Apply(param), param, fallback, token);
		}

		public PolicyResult<T> Fallback<TErrorContext, T>(Func<T> func, TErrorContext param, Func<CancellationToken, T> fallback, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return Fallback(func, fallback, emptyContext, token);
		}

		private PolicyResult<T> Fallback<T>(Func<T> func, Func<CancellationToken, T> fallback, EmptyErrorContext emptyErrorContext, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = func();
				result.SetOk();
				result.SetResult(resAction);
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled(oe);
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled(ae.GetCancellationException());
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(GetCatchBlockSyncHandler<Unit>(result, token)
													.Handle(ex, emptyErrorContext));
				if (!result.IsFailed)
				{
					fallback.HandleAsFallback(token).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public async Task<PolicyResult> FallbackAsync(Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			return await FallbackAsync(func, fallback, _emptyErrorContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult> FallbackAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, Func<CancellationToken, Task> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			return await FallbackAsync(func.Apply(param), param, fallback, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult> FallbackAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, Func<CancellationToken, Task> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return await FallbackAsync(func, fallback, emptyContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		private async Task<PolicyResult> FallbackAsync(Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallback, EmptyErrorContext emptyErrorContext, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult().WithNoDelegateException();

			PolicyResult result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				await func(token).ConfigureAwait(configureAwait);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled(oe);
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(await GetCatchBlockAsyncHandler<Unit>(result, configureAwait, token)
													.HandleAsync(ex, emptyErrorContext).ConfigureAwait(configureAwait));
				if (!result.IsFailed)
				{
					(await fallback.HandleAsFallbackAsync(configureAwait, token).ConfigureAwait(configureAwait)).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public async Task<PolicyResult<T>> FallbackAsync<T>(Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			return await FallbackAsync(func, fallback, _emptyErrorContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult<T>> FallbackAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, Func<CancellationToken, Task<T>> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			return await FallbackAsync(func.Apply(param), param, fallback, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult<T>> FallbackAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, Func<CancellationToken, Task<T>> fallback, bool configureAwait = false, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return await FallbackAsync(func, fallback, emptyContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		private async Task<PolicyResult<T>> FallbackAsync<T>(Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallback, EmptyErrorContext emptyErrorContext, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = await func(token).ConfigureAwait(configureAwait);
				result.SetResult(resAction);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled(oe);
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(await GetCatchBlockAsyncHandler<Unit>(result, configureAwait, token)
													.HandleAsync(ex, emptyErrorContext).ConfigureAwait(configureAwait));
				if (!result.IsFailed)
				{
					(await fallback.HandleAsFallbackAsync(configureAwait, token).ConfigureAwait(configureAwait)).ChangePolicyResult(result, ex);
				}
			}
			return result;
		}

		public DefaultFallbackProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultFallbackProcessor, TErrorContext>(actionProcessor);
		}

		public DefaultFallbackProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			return this.WithErrorContextProcessorOf<DefaultFallbackProcessor, TErrorContext>(actionProcessor, cancellationType);
		}

		public DefaultFallbackProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultFallbackProcessor, TErrorContext>(actionProcessor);
		}

		public DefaultFallbackProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultFallbackProcessor, TErrorContext>(funcProcessor);
		}

		public DefaultFallbackProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			return this.WithErrorContextProcessorOf<DefaultFallbackProcessor, TErrorContext>(funcProcessor, cancellationType);
		}

		public DefaultFallbackProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			return this.WithErrorContextProcessorOf<DefaultFallbackProcessor, TErrorContext>(funcProcessor);
		}

		public DefaultFallbackProcessor WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			return this.WithErrorContextProcessor<DefaultFallbackProcessor, TErrorContext>(errorProcessor);
		}

		///<inheritdoc cref = "ICanAddErrorFilter{DefaultFallbackProcessor}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public DefaultFallbackProcessor AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			this.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		///<inheritdoc cref = "ICanAddErrorFilter{DefaultFallbackProcessor}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public DefaultFallbackProcessor AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			this.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}
	}
}
