using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

namespace PoliNorError
{
	/// <summary>
	///  This class is primarily for internal use by PoliNorError
	/// </summary>
	public sealed partial class FallbackPolicyWithAsyncFunc : FallbackPolicyBase, IWithErrorFilter<FallbackPolicyWithAsyncFunc>, IWithInnerErrorFilter<FallbackPolicyWithAsyncFunc>, ICanAddErrorFilter<FallbackPolicyWithAsyncFunc>
	{
		internal FallbackPolicyWithAsyncFunc(IFallbackProcessor processor, FallbackFuncsProvider fallbackFuncsProvider) : base(processor ?? new DefaultFallbackProcessor(), fallbackFuncsProvider) {}

		public FallbackPolicyBase WithFallbackAction(Action<CancellationToken> fallback)
		{
			_fallbackFuncsProvider.Fallback = fallback;
			return this;
		}

		public FallbackPolicyBase WithFallbackAction(Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			_fallbackFuncsProvider.Fallback = fallback.ToCancelableAction(convertType, true);
			return this;
		}

		public new FallbackPolicyWithAsyncFunc WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackFunc);

		public new FallbackPolicyWithAsyncFunc WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackFunc, convertType);

		public new FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackAsync, convertType);

		public new FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackAsync);

		public new FallbackPolicyWithAsyncFunc IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyWithAsyncFunc, TException>(func);

		public new FallbackPolicyWithAsyncFunc IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicyWithAsyncFunc>(expression);

		///<inheritdoc cref = "FallbackPolicyBase.IncludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicyWithAsyncFunc IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<FallbackPolicyWithAsyncFunc, TInnerException>(predicate);

		public new FallbackPolicyWithAsyncFunc IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<FallbackPolicyWithAsyncFunc, TException1, TException2>();

		public new FallbackPolicyWithAsyncFunc IncludeErrorSet(IErrorSet errorSet)
			=> this.IncludeErrorSet<FallbackPolicyWithAsyncFunc>(errorSet);

		public new FallbackPolicyWithAsyncFunc ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyWithAsyncFunc, TException>(func);

		public new FallbackPolicyWithAsyncFunc ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyWithAsyncFunc>(expression);

		public new FallbackPolicyWithAsyncFunc ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<FallbackPolicyWithAsyncFunc, TException1, TException2>();

		public new FallbackPolicyWithAsyncFunc ExcludeErrorSet(IErrorSet errorSet)
			=> this.ExcludeErrorSet<FallbackPolicyWithAsyncFunc>(errorSet);

		///<inheritdoc cref = "FallbackPolicyBase.ExcludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicyWithAsyncFunc ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<FallbackPolicyWithAsyncFunc, TInnerException>(predicate);

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyWithAsyncFunc}"/>
		public new FallbackPolicyWithAsyncFunc SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyWithAsyncFunc, T}"/>
		public new FallbackPolicyWithAsyncFunc SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{FallbackPolicyWithAsyncFunc, T}"/>
		public new FallbackPolicyWithAsyncFunc SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyWithAsyncFunc, TErrorContext>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyWithAsyncFunc, TErrorContext>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyWithAsyncFunc, TErrorContext>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyWithAsyncFunc, TErrorContext>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyWithAsyncFunc, TErrorContext>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessorOf<FallbackPolicyWithAsyncFunc, TErrorContext>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			ThrowIfProcessorIsNotDefault(out DefaultFallbackProcessor _);
			return this.WithErrorContextProcessor<FallbackPolicyWithAsyncFunc, TErrorContext>(errorProcessor);
		}

		///<inheritdoc cref = "ICanAddErrorFilter{FallbackPolicyWithAsyncFunc}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public new FallbackPolicyWithAsyncFunc AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		///<inheritdoc cref = "ICanAddErrorFilter{FallbackPolicyWithAsyncFunc}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public new FallbackPolicyWithAsyncFunc AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}
	}
}
