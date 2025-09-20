using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;
using PoliNorError.Extensions.PolicyResultHandling;

namespace PoliNorError
{
	public abstract partial class FallbackPolicyBase : Policy, IFallbackPolicy, IWithErrorFilter<FallbackPolicyBase>, IWithInnerErrorFilter<FallbackPolicyBase>, ICanAddErrorFilter<FallbackPolicyBase>
	{
		internal IFallbackProcessor _fallbackProcessor;

		internal FallbackFuncsProvider _fallbackFuncsProvider;

		internal Action<CancellationToken> _fallback;
		internal Func<CancellationToken, Task> _fallbackAsync;

		protected FallbackPolicyBase(IFallbackProcessor processor, bool onlyGenericFallbackForGenericDelegate) : this(processor, new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate))
		{}

		protected FallbackPolicyBase(FallbackFuncsProvider fallbackFuncsProvider) : this(new DefaultFallbackProcessor(), fallbackFuncsProvider)
		{}

		private protected FallbackPolicyBase(IFallbackProcessor processor, FallbackFuncsProvider fallbackFuncsProvider) : base(processor)
		{
			_fallbackProcessor = processor;
			_fallbackFuncsProvider = fallbackFuncsProvider;
		}

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Action<CancellationToken> curFallback = _fallbackFuncsProvider.GetFallbackAction();

			var fallBackRes = _fallbackProcessor.Fallback(Act, curFallback, token)
								.SetWrappedPolicyResults(Wrapper)
								.SetPolicyName(PolicyName);

			HandlePolicyResult(fallBackRes, token);
			return fallBackRes;
		}

		public PolicyResult Handle<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

			Action<CancellationToken> curFallback = _fallbackFuncsProvider.GetFallbackAction();

			var fallBackRes = processor.Fallback(Act, param, curFallback, token)
								.SetWrappedPolicyResults(Wrapper)
								.SetPolicyName(PolicyName);

			HandlePolicyResult(fallBackRes, token);
			return fallBackRes;
		}

		public PolicyResult Handle<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			if (HasPolicyWrapperFactory)
			{
				return Handle(action.Apply(param), token);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

				Action<CancellationToken> curFallback = _fallbackFuncsProvider.GetFallbackAction();

				var result = processor.Fallback(action, param, curFallback, token)
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Func<CancellationToken, T> fallBackFunc = _fallbackFuncsProvider.GetFallbackFunc<T>();

			var fallbackResult = _fallbackProcessor.Fallback(Fn, fallBackFunc, token)
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			HandlePolicyResult(fallbackResult, token);
			return fallbackResult;
		}

		public PolicyResult<T> Handle<TParam, T>(Func<TParam, T> func, TParam param, CancellationToken token = default)
		{
			if (HasPolicyWrapperFactory)
			{
				return Handle(func.Apply(param), token);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

				Func<CancellationToken, T> fallBackFunc = _fallbackFuncsProvider.GetFallbackFunc<T>();

				var result = processor.Fallback(func, param, fallBackFunc, token)
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		public PolicyResult<T> Handle<TErrorContext, T>(Func<T> func, TErrorContext param, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

			Func<CancellationToken, T> fallBackFunc = _fallbackFuncsProvider.GetFallbackFunc<T>();

			var retryResult = processor.Fallback(Fn, param, fallBackFunc, token)
							  .SetWrappedPolicyResults(Wrapper)
							  .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Func<CancellationToken, Task> curFallbackAsync = _fallbackFuncsProvider.GetAsyncFallbackFunc();

			var fallBackRes = (await _fallbackProcessor.FallbackAsync(Fn, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait, token).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, bool configureAwait, CancellationToken token)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

			Func<CancellationToken, Task> curFallbackAsync = _fallbackFuncsProvider.GetAsyncFallbackFunc();

			var fallBackRes = (await processor.FallbackAsync(Fn, param, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait, token).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, bool configureAwait, CancellationToken token)
		{
			if (HasPolicyWrapperFactory)
			{
				return await HandleAsync(func.Apply(param), configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

				Func<CancellationToken, Task> curFallbackAsync = _fallbackFuncsProvider.GetAsyncFallbackFunc();

				var result = (await processor.FallbackAsync(func, param, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait))
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Func<CancellationToken, Task<T>> fallBackAsyncFunc = _fallbackFuncsProvider.GetAsyncFallbackFunc<T>(configureAwait);

			var fallBackRes = (await _fallbackProcessor.FallbackAsync(Fn, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public Task<PolicyResult<T>> HandleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult<T>> HandleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, bool configureAwait, CancellationToken token)
		{
			if (HasPolicyWrapperFactory)
			{
				return await HandleAsync(func.Apply(param), configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

				Func<CancellationToken, Task<T>> fallBackAsyncFunc = _fallbackFuncsProvider.GetAsyncFallbackFunc<T>(configureAwait);

				var result = (await processor.FallbackAsync(func, param, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait))
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		public Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		public async Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, bool configureAwait, CancellationToken token)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor processor);

			Func<CancellationToken, Task<T>> fallBackAsyncFunc = _fallbackFuncsProvider.GetAsyncFallbackFunc<T>(configureAwait);

			var retryResult = (await processor.FallbackAsync(Fn, param, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait))
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		/// <summary>
		/// Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.
		/// </summary>
		public bool OnlyGenericFallbackForGenericDelegate => _fallbackFuncsProvider.OnlyGenericFallbackForGenericDelegate;

		public bool HasFallbackFunc<T>() => _fallbackFuncsProvider.HasFallbackFunc<T>();

		public bool HasFallbackAction() => _fallbackFuncsProvider.HasFallbackAction();

		public bool HasAsyncFallbackFunc<T>() => _fallbackFuncsProvider.HasAsyncFallbackFunc<T>();

		public bool HasAsyncFallbackFunc() => _fallbackFuncsProvider.HasAsyncFallbackFunc();

		public FallbackPolicyBase WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicyBase, T>(fallbackFunc, convertType);

		public FallbackPolicyBase WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyBase, T>(fallbackFunc);

		public FallbackPolicyBase WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyBase, T>(fallbackAsync, convertType);

		public FallbackPolicyBase WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyBase, T>(fallbackAsync);

		public FallbackPolicyBase IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyBase, TException>(func);

		public FallbackPolicyBase IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicyBase>(expression);

		public FallbackPolicyBase IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<FallbackPolicyBase, TException1, TException2>();

		public FallbackPolicyBase IncludeErrorSet(IErrorSet errorSet) => this.IncludeErrorSet<FallbackPolicyBase>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the handling by the Fallback policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public FallbackPolicyBase IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<FallbackPolicyBase, TInnerException>(predicate);

		public FallbackPolicyBase ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyBase, TException>(func);

		public FallbackPolicyBase ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyBase>(expression);

		public FallbackPolicyBase ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<FallbackPolicyBase, TException1, TException2>();

		public FallbackPolicyBase ExcludeErrorSet(IErrorSet errorSet) => this.ExcludeErrorSet<FallbackPolicyBase>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the handling by the Fallback policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public FallbackPolicyBase ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<FallbackPolicyBase, TInnerException>(predicate);

		public FallbackPolicyBase AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyBase}"/>
		public FallbackPolicyBase SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyBase, T}"/>
		public FallbackPolicyBase SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{FallbackPolicyBase, T}"/>
		public FallbackPolicyBase SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}

		public FallbackPolicyBase WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyBase, TErrorContext>(actionProcessor);
		}

		public FallbackPolicyBase WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyBase, TErrorContext>(actionProcessor, cancellationType);
		}

		public FallbackPolicyBase WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyBase, TErrorContext>(actionProcessor);
		}

		public FallbackPolicyBase WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyBase, TErrorContext>(funcProcessor);
		}

		public FallbackPolicyBase WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyBase, TErrorContext>(funcProcessor, cancellationType);
		}

		public FallbackPolicyBase WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyBase, TErrorContext>(funcProcessor);
		}

		public FallbackPolicyBase WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessor<FallbackPolicyBase, TErrorContext>(errorProcessor);
		}

		protected void ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor proc)
		{
			ThrowHelper.ThrowIfNotImplemented(_fallbackProcessor, out proc);
		}

		///<inheritdoc cref = "ICanAddErrorFilter{FallbackPolicyBase}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public FallbackPolicyBase AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		///<inheritdoc cref = "ICanAddErrorFilter{FallbackPolicyBase}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public FallbackPolicyBase AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}
	}
}
