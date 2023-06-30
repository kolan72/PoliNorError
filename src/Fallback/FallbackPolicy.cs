using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class FallbackPolicy : FallbackPolicyBase, IWithErrorFilter<FallbackPolicy>
	{
		public FallbackPolicy(IBulkErrorProcessor processor = null) : this(new DefaultFallbackProcessor(processor ?? new BulkErrorProcessor())){}

		public FallbackPolicy(IFallbackProcessor processor) : base(processor){}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			return new FallbackPolicyWithAsyncFunc(_fallbackProcessor) { _fallbackAsync = fallbackAsync };
		}

		public FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc(Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new FallbackPolicyWithAsyncFunc(_fallbackProcessor) { _fallbackAsync = fallbackAsync.ToCancelableFunc(convertType) };
		}

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync, convertType);

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync);

		public FallbackPolicyWithAction WithFallbackAction(Action<CancellationToken> fallback)
		{
			return new FallbackPolicyWithAction(_fallbackProcessor) { _fallback = fallback };
		}

		public FallbackPolicyWithAction WithFallbackAction(Action fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new FallbackPolicyWithAction(_fallbackProcessor) { _fallback = fallback.ToCancelableAction(convertType) };
		}

		public new FallbackPolicy WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc);

		public new FallbackPolicy WithFallbackFunc<T>(Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc, convertType);

		public new FallbackPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicy>(expression);

		public new FallbackPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicy, TException>(func);

		public new FallbackPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicy>(expression);

		public new FallbackPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicy, TException>(func);

		internal FallbackPolicy WithAsyncFallbackFuncAndReturnSelf(Func<CancellationToken, Task> fallbackAsync)
		{
			_fallbackAsync = fallbackAsync;
			return this;
		}

		internal FallbackPolicy WithAsyncFallbackFuncAndReturnSelf(Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			_fallbackAsync = fallbackAsync.ToCancelableFunc(convertType);
			return this;
		}

		internal FallbackPolicy WithFallbackActionAndReturnSelf(Action<CancellationToken> fallback)
		{
			_fallback = fallback;
			return this;
		}

		internal FallbackPolicy WithFallbackActionAndReturnSelf(Action fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			_fallback = fallback.ToCancelableAction(convertType);
			return this;
		}
	}
}
