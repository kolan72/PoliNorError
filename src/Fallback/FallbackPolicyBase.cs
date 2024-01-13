using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract partial class FallbackPolicyBase : Policy, IFallbackPolicy, IWithErrorFilter<FallbackPolicyBase>
	{
		internal IFallbackProcessor _fallbackProcessor;

		internal FallbackFuncsProvider _fallbackFuncsProvider = new FallbackFuncsProvider();

		internal Action<CancellationToken> _fallback;
		internal Func<CancellationToken, Task> _fallbackAsync;

		protected FallbackPolicyBase(IFallbackProcessor processor) : base(processor)
		{
			_fallbackProcessor = processor;
		}

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			Action<CancellationToken> curFallback = _fallbackFuncsProvider.GetFallbackAction();

			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var fallBackRes = _fallbackProcessor.Fallback(Act, curFallback, token)
								.SetWrappedPolicyResults(Wrapper)
								.SetPolicyName(PolicyName);

			HandlePolicyResult(fallBackRes, token);
			return fallBackRes;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			Func<CancellationToken, T> fallBackFunc = _fallbackFuncsProvider.GetFallbackFunc<T>();

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var fallbackResult = _fallbackProcessor.Fallback(Fn, fallBackFunc, token)
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			HandlePolicyResult(fallbackResult, token);
			return fallbackResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			Func<CancellationToken, Task> curFallbackAsync = _fallbackFuncsProvider.GetAsyncFallbackFunc();

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var fallBackRes = (await _fallbackProcessor.FallbackAsync(Fn, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait, token).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			Func<CancellationToken, Task<T>> fallBackAsyncFunc = _fallbackFuncsProvider.GetAsyncFallbackFunc<T>(configureAwait);

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var fallBackRes = (await _fallbackProcessor.FallbackAsync(Fn, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public bool HasFallbackFunc<T>() => _fallbackFuncsProvider.HasFallbackFunc<T>();

		public bool HasFallbackAction() => _fallbackFuncsProvider.HasFallbackAction();

		public bool HasAsyncFallbackFunc<T>() => _fallbackFuncsProvider.HasAsyncFallbackFunc<T>();

		public bool HasAsyncFallbackFunc() => _fallbackFuncsProvider.HasAsyncFallbackFunc();

		public FallbackPolicyBase WithFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable) => this.WithFallbackFunc<FallbackPolicyBase, T>(fallbackFunc, convertType);

		public FallbackPolicyBase WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyBase, T>(fallbackFunc);

		public FallbackPolicyBase WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyBase, T>(fallbackAsync, convertType);

		public FallbackPolicyBase WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyBase, T>(fallbackAsync);

		public FallbackPolicyBase IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyBase, TException>(func);

		public FallbackPolicyBase IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<FallbackPolicyBase>(expression);

		public FallbackPolicyBase IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<FallbackPolicyBase, TException1, TException2>();

		public FallbackPolicyBase ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyBase, TException>(func);

		public FallbackPolicyBase ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyBase>(expression);

		public FallbackPolicyBase ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<FallbackPolicyBase, TException1, TException2>();

		public FallbackPolicyBase AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public FallbackPolicyBase AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyBase}"/>
		public FallbackPolicyBase SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{FallbackPolicyBase, T}"/>
		public FallbackPolicyBase SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}
	}
}
