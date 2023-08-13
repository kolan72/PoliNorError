using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	///  This class is primarily for internal use by PoliNorError
	/// </summary>
	public sealed class FallbackPolicyWithAction : FallbackPolicyBase, IWithErrorFilter<FallbackPolicyWithAction>
	{
		internal FallbackPolicyWithAction(IFallbackProcessor processor) : base(processor ?? new DefaultFallbackProcessor()){}

		public new FallbackPolicyWithAction WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyWithAction, T>(fallbackFunc);

		public new FallbackPolicyWithAction WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicyWithAction, T>(fallbackFunc, convertType);

		public FallbackPolicyBase WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			_fallbackAsync = fallbackAsync;
			return this;
		}

		public FallbackPolicyBase WithAsyncFallbackFunc(Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			_fallbackAsync = convertType == CancellationType.Precancelable ? fallbackAsync.ToPrecancelableFunc() : fallbackAsync.ToCancelableFunc();
			return this;
		}

		public new FallbackPolicyWithAction WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyWithAction, T>(fallbackAsync, convertType);

		public new FallbackPolicyWithAction WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyWithAction, T>(fallbackAsync);

		public new FallbackPolicyWithAction IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyWithAction, TException>(func);

		public new FallbackPolicyWithAction IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicyWithAction>(expression);

		public new FallbackPolicyWithAction ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyWithAction, TException>(func);

		public new FallbackPolicyWithAction ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyWithAction>(expression);

		public new FallbackPolicyWithAction AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}
	}
}
