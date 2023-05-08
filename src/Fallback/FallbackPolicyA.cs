using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class FallbackPolicyA : FallbackPolicyBase
	{
		internal FallbackPolicyA(IFallbackProcessor processor) : base(processor ?? new DefaultFallbackProcessor()){}

		public FallbackPolicyBase WithFallbackAction(Action<CancellationToken> fallback)
		{
			_fallback = fallback;
			return this;
		}

		public FallbackPolicyBase WithFallbackAction(Action fallback, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			_fallback = convertType == ConvertToCancelableFuncType.Precancelable ? fallback.ToPrecancelableAction() : fallback.ToCancelableAction();
			return this;
		}

		public new FallbackPolicyA WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyA, T>(fallbackFunc);

		public new FallbackPolicyA WithFallbackFunc<T>(Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithFallbackFunc<FallbackPolicyA, T>(fallbackFunc, convertType);

		public new FallbackPolicyA WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyA, T>(fallbackAsync, convertType);

		public new FallbackPolicyA WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyA, T>(fallbackAsync);

		public new FallbackPolicyA ForError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ForError<FallbackPolicyA, TException>(func);

		public new FallbackPolicyA ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyA, TException>(func);
	}
}
