using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;
using PoliNorError.Extensions.PolicyResultHandling;

namespace PoliNorError
{
	/// <summary>
	/// A fallback policy that can handle delegates.
	/// </summary>
	public sealed partial class FallbackPolicy : FallbackPolicyBase, IWithErrorFilter<FallbackPolicy>, IWithInnerErrorFilter<FallbackPolicy>, ICanAddErrorFilter<FallbackPolicy>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FallbackPolicy"/>.
		/// </summary>
		/// <param name="onlyGenericFallbackForGenericDelegate">Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.</param>
		public FallbackPolicy(bool onlyGenericFallbackForGenericDelegate = false) : this(new DefaultFallbackProcessor(), onlyGenericFallbackForGenericDelegate) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FallbackPolicy"/>.
		/// </summary>
		/// <param name="processor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.</param>
		public FallbackPolicy(IBulkErrorProcessor processor, bool onlyGenericFallbackForGenericDelegate = false) : this(new DefaultFallbackProcessor(processor), onlyGenericFallbackForGenericDelegate) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="FallbackPolicy"/>.
		/// </summary>
		/// <param name="processor"><see cref="IFallbackProcessor"/></param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.</param>
		public FallbackPolicy(IFallbackProcessor processor, bool onlyGenericFallbackForGenericDelegate = false) : base(processor, onlyGenericFallbackForGenericDelegate) {}

		internal FallbackPolicy(FallbackFuncsProvider fallbackFuncsProvider) : base(fallbackFuncsProvider){}

		public FallbackPolicyWithAction WithFallbackAction(Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			return WithFallbackAction(fallback.ToCancelableAction(convertType, true));
		}

		public FallbackPolicyWithAction WithFallbackAction(Action<CancellationToken> fallback)
		{
			var fallbackPolicyWithAction = new FallbackPolicyWithAction(_fallbackProcessor, _fallbackFuncsProvider);
			fallbackPolicyWithAction._fallbackFuncsProvider.Fallback = fallback;
			return fallbackPolicyWithAction;
		}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			return WithAsyncFallbackFunc(fallbackAsync.ToCancelableFunc(convertType, true));
		}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			var fallbackPolicyWithAsyncFunc = new FallbackPolicyWithAsyncFunc(_fallbackProcessor, _fallbackFuncsProvider);
			fallbackPolicyWithAsyncFunc._fallbackFuncsProvider.FallbackAsync = fallbackAsync;
			return fallbackPolicyWithAsyncFunc;
		}

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync, convertType);

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync);

		public new FallbackPolicy WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc);

		public new FallbackPolicy WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc, convertType);

		public new FallbackPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicy>(expression);

		public new FallbackPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<FallbackPolicy, TException1, TException2>();

		public new FallbackPolicy IncludeErrorSet(IErrorSet errorSet) => this.IncludeErrorSet<FallbackPolicy>(errorSet);

		///<inheritdoc cref = "FallbackPolicyBase.IncludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicy IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<FallbackPolicy, TInnerException>(predicate);

		public new FallbackPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicy>(expression);

		public new FallbackPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<FallbackPolicy, TException1, TException2>();

		public new FallbackPolicy ExcludeErrorSet(IErrorSet errorSet) => this.ExcludeErrorSet<FallbackPolicy>(errorSet);

		///<inheritdoc cref = "FallbackPolicyBase.ExcludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicy ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<FallbackPolicy, TInnerException>(predicate);

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicy}"/>
		public new FallbackPolicy SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicy, T}"/>
		public new FallbackPolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{FallbackPolicy, T}"/>
		public new FallbackPolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}

		public new FallbackPolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicy, TErrorContext>(actionProcessor);
		}

		public new FallbackPolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicy, TErrorContext>(actionProcessor, cancellationType);
		}

		public new FallbackPolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicy, TErrorContext>(actionProcessor);
		}

		public new FallbackPolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicy, TErrorContext>(funcProcessor);
		}

		public new FallbackPolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicy, TErrorContext>(funcProcessor, cancellationType);
		}

		public new FallbackPolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicy, TErrorContext>(funcProcessor);
		}

		public new FallbackPolicy WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessor<FallbackPolicy, TErrorContext>(errorProcessor);
		}

		///<inheritdoc cref = "ICanAddErrorFilter{FallbackPolicy}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public new FallbackPolicy AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		///<inheritdoc cref = "ICanAddErrorFilter{FallbackPolicy}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public new FallbackPolicy AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}
	}
}
