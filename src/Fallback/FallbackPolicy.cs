using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class FallbackPolicy : FallbackPolicyBase
	{
		public FallbackPolicy(IBulkErrorProcessor processor = null) : this(new DefaultFallbackProcessor(processor ?? new BulkErrorProcessor())){}

		public FallbackPolicy(IFallbackProcessor processor) : base(processor){}

		public FallbackPolicyA WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			return new FallbackPolicyA(_fallbackProcessor) { _fallbackAsync = fallbackAsync };
		}

		public FallbackPolicyA WithAsyncFallbackFunc(Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new FallbackPolicyA(_fallbackProcessor) { _fallbackAsync = fallbackAsync.ToCancelableFunc(convertType) };
		}

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync, convertType);

		public new FallbackPolicy WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicy, T>(fallbackAsync);

		public FallbackPolicyF WithFallbackAction(Action<CancellationToken> fallback)
		{
			return new FallbackPolicyF(_fallbackProcessor) { _fallback = fallback };
		}

		public FallbackPolicyF WithFallbackAction(Action fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new FallbackPolicyF(_fallbackProcessor) { _fallback = fallback.ToCancelableAction(convertType) };
		}

		public new FallbackPolicy WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc);

		public new FallbackPolicy WithFallbackFunc<T>(Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithFallbackFunc<FallbackPolicy, T>(fallbackFunc, convertType);

		public new FallbackPolicy ForError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ForError<FallbackPolicy, TException>(func);

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
