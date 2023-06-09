using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	///  This class is primarily for internal use by PoliNorError
	/// </summary>
	public sealed class FallbackPolicyWithAction : FallbackPolicyBase
	{
		internal FallbackPolicyWithAction(IFallbackProcessor processor) : base(processor ?? new DefaultFallbackProcessor()){}

		public new FallbackPolicyWithAction WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyWithAction, T>(fallbackFunc);

		public new FallbackPolicyWithAction WithFallbackFunc<T>(Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithFallbackFunc<FallbackPolicyWithAction, T>(fallbackFunc, convertType);

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

		public new FallbackPolicyWithAction WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyWithAction, T>(fallbackAsync, convertType);

		public new FallbackPolicyWithAction WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyWithAction, T>(fallbackAsync);

		public new FallbackPolicyWithAction IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyWithAction, TException>(func);

		public new FallbackPolicyWithAction ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyWithAction, TException>(func);
	}
}
