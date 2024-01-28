using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicy : FallbackPolicyBase, IWithErrorFilter<FallbackPolicy>, IWithInnerErrorFilter<FallbackPolicy>
	{
		public FallbackPolicy(bool onlyGenericFallbackForGenericDelegate = false) : this(new DefaultFallbackProcessor(), onlyGenericFallbackForGenericDelegate) { }

		public FallbackPolicy(IBulkErrorProcessor processor, bool onlyGenericFallbackForGenericDelegate = false) : this(new DefaultFallbackProcessor(processor), onlyGenericFallbackForGenericDelegate) {}

		public FallbackPolicy(IFallbackProcessor processor, bool onlyGenericFallbackForGenericDelegate = false) : base(processor, onlyGenericFallbackForGenericDelegate) {}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			var fallbackPolicyWithAsyncFunc = new FallbackPolicyWithAsyncFunc(_fallbackProcessor, _onlyGenericFallbackForGenericDelegate);
			fallbackPolicyWithAsyncFunc._fallbackFuncsProvider.FallbackAsync = fallbackAsync;
			return fallbackPolicyWithAsyncFunc;
		}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			var fallbackPolicyWithAsyncFunc = new FallbackPolicyWithAsyncFunc(_fallbackProcessor, _onlyGenericFallbackForGenericDelegate);
			fallbackPolicyWithAsyncFunc._fallbackFuncsProvider.FallbackAsync = fallbackAsync.ToCancelableFunc(convertType);
			return fallbackPolicyWithAsyncFunc;
		}

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync, convertType);

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync);

		public FallbackPolicyWithAction WithFallbackAction(Action<CancellationToken> fallback)
		{
			var fallbackPolicyWithAction = new FallbackPolicyWithAction(_fallbackProcessor, _onlyGenericFallbackForGenericDelegate);
			fallbackPolicyWithAction._fallbackFuncsProvider.Fallback = fallback;
			return fallbackPolicyWithAction;
		}

		public FallbackPolicyWithAction WithFallbackAction(Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			var fallbackPolicyWithAction = new FallbackPolicyWithAction(_fallbackProcessor, _onlyGenericFallbackForGenericDelegate);
			fallbackPolicyWithAction._fallbackFuncsProvider.Fallback = fallback.ToCancelableAction(convertType, true);
			return fallbackPolicyWithAction;
		}

		public new FallbackPolicy WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc);

		public new FallbackPolicy WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc, convertType);

		public new FallbackPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicy>(expression);

		public new FallbackPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<FallbackPolicy, TException1, TException2>();

		///<inheritdoc cref = "FallbackPolicyBase.IncludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicy IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<FallbackPolicy, TInnerException>(predicate);

		public new FallbackPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicy>(expression);

		public new FallbackPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<FallbackPolicy, TException1, TException2>();

		///<inheritdoc cref = "FallbackPolicyBase.ExcludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicy ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<FallbackPolicy, TInnerException>(predicate);

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
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
	}
}
