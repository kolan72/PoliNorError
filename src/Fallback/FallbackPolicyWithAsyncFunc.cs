using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	///  This class is primarily for internal use by PoliNorError
	/// </summary>
	public sealed class FallbackPolicyWithAsyncFunc : FallbackPolicyBase, IWithErrorFilter<FallbackPolicyWithAsyncFunc>
	{
		internal FallbackPolicyWithAsyncFunc(IFallbackProcessor processor) : base(processor ?? new DefaultFallbackProcessor()){}

		public FallbackPolicyBase WithFallbackAction(Action<CancellationToken> fallback)
		{
			_fallback = fallback;
			return this;
		}

		public FallbackPolicyBase WithFallbackAction(Action fallback, CancellationType convertType = CancellationType.Precancelable)
		{
			_fallback = convertType == CancellationType.Precancelable ? fallback.ToPrecancelableAction() : fallback.ToCancelableAction();
			return this;
		}

		public new FallbackPolicyWithAsyncFunc WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackFunc);

		public new FallbackPolicyWithAsyncFunc WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackFunc, convertType);

		public new FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackAsync, convertType);

		public new FallbackPolicyWithAsyncFunc WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyWithAsyncFunc, T>(fallbackAsync);

		public new FallbackPolicyWithAsyncFunc IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyWithAsyncFunc, TException>(func);

		public new FallbackPolicyWithAsyncFunc IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicyWithAsyncFunc>(expression);

		public new FallbackPolicyWithAsyncFunc ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyWithAsyncFunc, TException>(func);

		public new FallbackPolicyWithAsyncFunc ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyWithAsyncFunc>(expression);

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicyWithAsyncFunc AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}
	}
}
