using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class FallbackPolicy : FallbackPolicyBase, IWithErrorFilter<FallbackPolicy>
	{
		public FallbackPolicy(IBulkErrorProcessor processor = null) : this(new DefaultFallbackProcessor(processor ?? new BulkErrorProcessor(PolicyAlias.Fallback))){}

		public FallbackPolicy(IFallbackProcessor processor) : base(processor){}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			return new FallbackPolicyWithAsyncFunc(_fallbackProcessor) { _fallbackAsync = fallbackAsync };
		}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			return new FallbackPolicyWithAsyncFunc(_fallbackProcessor) { _fallbackAsync = fallbackAsync.ToCancelableFunc(convertType) };
		}

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync, convertType);

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync);

		public FallbackPolicyWithAction WithFallbackAction(Action<CancellationToken> fallback)
		{
			return new FallbackPolicyWithAction(_fallbackProcessor) { _fallback = fallback };
		}

		public FallbackPolicyWithAction WithFallbackAction(Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			return new FallbackPolicyWithAction(_fallbackProcessor) { _fallback = fallback.ToCancelableAction(convertType) };
		}

		public new FallbackPolicy WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc);

		public new FallbackPolicy WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc, convertType);

		public new FallbackPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicy>(expression);

		public new FallbackPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicy>(expression);

		public new FallbackPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		internal FallbackPolicy WithAsyncFallbackFuncAndReturnSelf(Func<CancellationToken, Task> fallbackAsync)
		{
			_fallbackAsync = fallbackAsync;
			return this;
		}

		internal FallbackPolicy WithAsyncFallbackFuncAndReturnSelf(Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			_fallbackAsync = fallbackAsync.ToCancelableFunc(convertType);
			return this;
		}

		internal FallbackPolicy WithFallbackActionAndReturnSelf(Action<CancellationToken> fallback)
		{
			_fallback = fallback;
			return this;
		}

		internal FallbackPolicy WithFallbackActionAndReturnSelf(Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			_fallback = fallback.ToCancelableAction(convertType);
			return this;
		}
	}
}
