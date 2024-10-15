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
	public sealed partial class FallbackPolicyWithAction : FallbackPolicyBase, IWithErrorFilter<FallbackPolicyWithAction>, IWithInnerErrorFilter<FallbackPolicyWithAction>
	{
		internal FallbackPolicyWithAction(IFallbackProcessor processor, FallbackFuncsProvider fallbackFuncsProvider) : base(processor ?? new DefaultFallbackProcessor(), fallbackFuncsProvider) {}

		public new FallbackPolicyWithAction WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyWithAction, T>(fallbackFunc);

		public new FallbackPolicyWithAction WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
			=> this.WithFallbackFunc<FallbackPolicyWithAction, T>(fallbackFunc, convertType);

		public FallbackPolicyBase WithAsyncFallbackFunc(Func<CancellationToken, Task> fallbackAsync)
		{
			_fallbackFuncsProvider.FallbackAsync = fallbackAsync;
			return this;
		}

		public FallbackPolicyBase WithAsyncFallbackFunc(Func<Task> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			_fallbackFuncsProvider.FallbackAsync = fallbackAsync.ToCancelableFunc(convertType, true);
			return this;
		}

		public new FallbackPolicyWithAction WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
			=> this.WithAsyncFallbackFunc<FallbackPolicyWithAction, T>(fallbackAsync, convertType);

		public new FallbackPolicyWithAction WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync)
			=> this.WithAsyncFallbackFunc<FallbackPolicyWithAction, T>(fallbackAsync);

		public new FallbackPolicyWithAction IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception
			=> this.IncludeError<FallbackPolicyWithAction, TException>(func);

		public new FallbackPolicyWithAction IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicyWithAction>(expression);

		public new FallbackPolicyWithAction IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception
			=> this.IncludeErrorSet<FallbackPolicyWithAction, TException1, TException2>();

		public new FallbackPolicyWithAction IncludeErrorSet(IErrorSet errorSet)
			=> this.IncludeErrorSet<FallbackPolicyWithAction>(errorSet);

		///<inheritdoc cref = "FallbackPolicyBase.IncludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicyWithAction IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<FallbackPolicyWithAction, TInnerException>(predicate);

		public new FallbackPolicyWithAction ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
			=> this.ExcludeError<FallbackPolicyWithAction, TException>(func);

		public new FallbackPolicyWithAction ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyWithAction>(expression);

		public new FallbackPolicyWithAction ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception
			=> this.ExcludeErrorSet<FallbackPolicyWithAction, TException1, TException2>();

		public new FallbackPolicyWithAction ExcludeErrorSet(IErrorSet errorSet)
			=> this.ExcludeErrorSet<FallbackPolicyWithAction>(errorSet);

		///<inheritdoc cref = "FallbackPolicyBase.ExcludeInnerError{TInnerException}(Func{TInnerException, bool})"/>
		public new FallbackPolicyWithAction ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<FallbackPolicyWithAction, TInnerException>(predicate);

		public new FallbackPolicyWithAction AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public new FallbackPolicyWithAction AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyWithAction}"/>
		public new FallbackPolicyWithAction SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyWithAction, T}"/>
		public new FallbackPolicyWithAction SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{FallbackPolicyWithAction, T}"/>
		public new FallbackPolicyWithAction SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}
	}
}
