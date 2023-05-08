using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class FallbackPolicyF : FallbackPolicyBase
	{
		internal FallbackPolicyF(IFallbackProcessor processor) : base(processor ?? new DefaultFallbackProcessor()){}

		public new FallbackPolicyF WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyF, T>(fallbackFunc);

		public new FallbackPolicyF WithFallbackFunc<T>(Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithFallbackFunc<FallbackPolicyF, T>(fallbackFunc, convertType);

		public FallbackPolicyBase WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			_fallbackAsync = fallbackAsync;
			return this;
		}

		public FallbackPolicyBase WithAsyncFallbackFunc(Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			_fallbackAsync = convertType == ConvertToCancelableFuncType.Precancelable ? fallbackAsync.ToPrecancelableFunc() : fallbackAsync.ToCancelableFunc();
			return this;
		}

		public new FallbackPolicyF WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyF, T>(fallbackAsync, convertType);

		public new FallbackPolicyF WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyF, T>(fallbackAsync);

		public new FallbackPolicyF ForError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ForError<FallbackPolicyF, TException>(func);

		public new FallbackPolicyF ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyF, TException>(func);
	}
}
