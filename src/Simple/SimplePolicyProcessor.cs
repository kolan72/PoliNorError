using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// A SimplePolicy processor that can handle delegates.
	/// </summary>
	public sealed class SimplePolicyProcessor : PolicyProcessor, ISimplePolicyProcessor, ICanAddErrorFilter<SimplePolicyProcessor>
	{
		private readonly EmptyErrorContext _emptyErrorContext;

		private readonly bool _rethrowIfErrorFilterUnsatisfied;

		///<inheritdoc cref = "SimplePolicyProcessor(IBulkErrorProcessor, bool)"/>
		public SimplePolicyProcessor(bool rethrowIfErrorFilterUnsatisfied = false) : this(null, rethrowIfErrorFilterUnsatisfied) { }

		/// <summary>
		/// Initializes a new instance of the SimplePolicyProcessor
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="rethrowIfErrorFilterUnsatisfied">Specifies whether an exception is rethrown if the error filter is unsatisfied.</param>
		public SimplePolicyProcessor(IBulkErrorProcessor bulkErrorProcessor, bool rethrowIfErrorFilterUnsatisfied = false) : this(bulkErrorProcessor, null, rethrowIfErrorFilterUnsatisfied)
		{}

		internal SimplePolicyProcessor(CatchBlockFilter catchBlockFilter, IBulkErrorProcessor bulkErrorProcessor = null, bool rethrowIfErrorFilterUnsatisfied = false) : this(bulkErrorProcessor, (catchBlockFilter ?? new CatchBlockFilter()).ErrorFilter, rethrowIfErrorFilterUnsatisfied)
		{}

		private SimplePolicyProcessor(IBulkErrorProcessor bulkErrorProcessor, ExceptionFilter exceptionFilter, bool rethrowIfErrorFilterUnsatisfied) : base(exceptionFilter ?? new ExceptionFilter(), bulkErrorProcessor)
		{
			_emptyErrorContext = EmptyErrorContext.DefaultSimple;
			_rethrowIfErrorFilterUnsatisfied = rethrowIfErrorFilterUnsatisfied;
		}

		///<inheritdoc cref = "CreateDefault(IBulkErrorProcessor, bool)"/>
		public static ISimplePolicyProcessor CreateDefault(bool rethrowIfErrorFilterUnsatisfied = false) => new SimplePolicyProcessor(rethrowIfErrorFilterUnsatisfied);

		/// <summary>
		/// Creates and returns the <see cref="SimplePolicyProcessor"/> class, which represents the default implementation of the <see cref="ISimplePolicyProcessor"/> interface.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="rethrowIfErrorFilterUnsatisfied">Specifies whether an exception is rethrown if the error filter is unsatisfied.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, bool rethrowIfErrorFilterUnsatisfied = false) => new SimplePolicyProcessor(bulkErrorProcessor, rethrowIfErrorFilterUnsatisfied);

		///<inheritdoc cref = "ISimplePolicyProcessor.Execute"/>
		public PolicyResult Execute(Action action, CancellationToken token = default)
		{
			return Execute(action, _emptyErrorContext, token);
		}

		public PolicyResult Execute<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			return Execute(action.Apply(param), param, token);
		}

		public PolicyResult Execute<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return Execute(action, (EmptyErrorContext)emptyContext, token);
		}

		private PolicyResult Execute(Action action, EmptyErrorContext emptyErrorContext, CancellationToken token = default)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.ForSync();

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
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					var (filterUnsatisfied, filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						ex.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY] = true;
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(GetCatchBlockSyncHandler<Unit>(result, token)
													 .Handle(ex, emptyErrorContext));
			}
			return result;
		}

		///<inheritdoc cref = "ISimplePolicyProcessor.Execute{T}"/>
		public PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
		{
			return Execute(func, _emptyErrorContext, token);
		}

		public PolicyResult<T> Execute<TParam, T>(Func<TParam, T> func, TParam param, CancellationToken token = default)
		{
			return Execute(func.Apply(param), param, token);
		}

		public PolicyResult<T> Execute<TErrorContext, T>(Func<T> func, TErrorContext param, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return Execute(func, (EmptyErrorContext)emptyContext, token);
		}

		private PolicyResult<T> Execute<T>(Func<T> func, EmptyErrorContext emptyErrorContext, CancellationToken token = default)
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
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					var (filterUnsatisfied, filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						ex.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY] = true;
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(GetCatchBlockSyncHandler<Unit>(result, token)
													 .Handle(ex, emptyErrorContext));
			}
			return result;
		}

		///<inheritdoc cref = "ISimplePolicyProcessor.ExecuteAsync"/>
		public async Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			return await ExecuteAsync(func, _emptyErrorContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult> ExecuteAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, bool configureAwait = false, CancellationToken token = default)
		{
			return await ExecuteAsync(func.Apply(param), param, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult> ExecuteAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, bool configureAwait = false, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return await ExecuteAsync(func, (EmptyErrorContext)emptyContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		private async Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, EmptyErrorContext emptyErrorContext, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult().WithNoDelegateException();

			var result = PolicyResult.InitByConfigureAwait(configureAwait);

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
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					var (filterUnsatisfied, filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						ex.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY] = true;
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(await GetCatchBlockAsyncHandler<Unit>(result, configureAwait, token)
															.HandleAsync(ex, emptyErrorContext).ConfigureAwait(configureAwait));
			}
			return result;
		}

		///<inheritdoc cref = "ISimplePolicyProcessor.ExecuteAsync{T}"/>
		public async Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			return await ExecuteAsync(func, _emptyErrorContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult<T>> ExecuteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, bool configureAwait = false, CancellationToken token = default)
		{
			return await ExecuteAsync(func.Apply(param), param, configureAwait, token).ConfigureAwait(configureAwait);
		}

		public async Task<PolicyResult<T>> ExecuteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, bool configureAwait = false, CancellationToken token = default)
		{
			var emptyContext = new EmptyErrorContext<TErrorContext>(param);
			return await ExecuteAsync(func, (EmptyErrorContext)emptyContext, configureAwait, token).ConfigureAwait(configureAwait);
		}

		private async Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, EmptyErrorContext emptyErrorContext, bool configureAwait = false, CancellationToken token = default)
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
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					var (filterUnsatisfied, filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						ex.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY] = true;
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(await GetCatchBlockAsyncHandler<Unit>(result, configureAwait, token)
															.HandleAsync(ex, emptyErrorContext).ConfigureAwait(configureAwait));
			}
			return result;
		}

		public SimplePolicyProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			return this.WithErrorContextProcessorOf<SimplePolicyProcessor, TErrorContext>(actionProcessor);
		}

		public SimplePolicyProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			return this.WithErrorContextProcessorOf<SimplePolicyProcessor, TErrorContext>(actionProcessor, cancellationType);
		}

		public SimplePolicyProcessor WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			return this.WithErrorContextProcessorOf<SimplePolicyProcessor, TErrorContext>(actionProcessor);
		}

		public SimplePolicyProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			return this.WithErrorContextProcessorOf<SimplePolicyProcessor, TErrorContext>(funcProcessor);
		}

		public SimplePolicyProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			return this.WithErrorContextProcessorOf<SimplePolicyProcessor, TErrorContext>(funcProcessor, cancellationType);
		}

		public SimplePolicyProcessor WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			return this.WithErrorContextProcessorOf<SimplePolicyProcessor, TErrorContext>(funcProcessor);
		}

		public SimplePolicyProcessor WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			return this.WithErrorContextProcessor<SimplePolicyProcessor, TErrorContext>(errorProcessor);
		}

		private (bool? FilterUnsatisfied, Exception exception) GetFilterUnsatisfiedOrFilterException(Exception ex)
		{
			try
			{
				return (!ErrorFilter.GetCanHandle()(ex), null);
			}
			catch (Exception filterEx)
			{
				return (null, filterEx);
			}
		}

		private static void AddErrorAndCatchBlockFilterError(PolicyResult result, Exception ex, Exception filterException)
		{
			result.AddError(ex);
			result.SetFailedWithCatchBlockError(filterException, ex, CatchBlockExceptionSource.ErrorFilter);
		}

		///<inheritdoc cref = "ICanAddErrorFilter{SimplePolicyProcessor}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public SimplePolicyProcessor AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			this.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		///<inheritdoc cref = "ICanAddErrorFilter{SimplePolicyProcessor}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public SimplePolicyProcessor AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			this.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}
	}
}
