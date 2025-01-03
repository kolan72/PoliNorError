﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

namespace PoliNorError
{
	public abstract partial class FallbackPolicyBase : Policy, IFallbackPolicy, IWithErrorFilter<FallbackPolicyBase>, IWithInnerErrorFilter<FallbackPolicyBase>
	{
		internal IFallbackProcessor _fallbackProcessor;

		internal FallbackFuncsProvider _fallbackFuncsProvider;

		internal Action<CancellationToken> _fallback;
		internal Func<CancellationToken, Task> _fallbackAsync;

		protected FallbackPolicyBase(IFallbackProcessor processor, bool onlyGenericFallbackForGenericDelegate) : this(processor, new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate))
		{}

		protected FallbackPolicyBase(FallbackFuncsProvider fallbackFuncsProvider) : this(null, fallbackFuncsProvider)
		{}

		private protected FallbackPolicyBase(IFallbackProcessor processor, FallbackFuncsProvider fallbackFuncsProvider) : base(processor)
		{
			_fallbackProcessor = processor;
			_fallbackFuncsProvider = fallbackFuncsProvider;
		}

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Action<CancellationToken> curFallback = _fallbackFuncsProvider.GetFallbackAction();

			var fallBackRes = _fallbackProcessor.Fallback(Act, curFallback, token)
								.SetWrappedPolicyResults(Wrapper)
								.SetPolicyName(PolicyName);

			HandlePolicyResult(fallBackRes, token);
			return fallBackRes;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Func<CancellationToken, T> fallBackFunc = _fallbackFuncsProvider.GetFallbackFunc<T>();

			var fallbackResult = _fallbackProcessor.Fallback(Fn, fallBackFunc, token)
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			HandlePolicyResult(fallbackResult, token);
			return fallbackResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Func<CancellationToken, Task> curFallbackAsync = _fallbackFuncsProvider.GetAsyncFallbackFunc();

			var fallBackRes = (await _fallbackProcessor.FallbackAsync(Fn, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait, token).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			Func<CancellationToken, Task<T>> fallBackAsyncFunc = _fallbackFuncsProvider.GetAsyncFallbackFunc<T>(configureAwait);

			var fallBackRes = (await _fallbackProcessor.FallbackAsync(Fn, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait))
				.SetWrappedPolicyResults(Wrapper)
				.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(fallBackRes, configureAwait).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		/// <summary>
		/// Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.
		/// </summary>
		public bool OnlyGenericFallbackForGenericDelegate => _fallbackFuncsProvider.OnlyGenericFallbackForGenericDelegate;

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

		public FallbackPolicyBase IncludeErrorSet(IErrorSet errorSet) => this.IncludeErrorSet<FallbackPolicyBase>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the handling by the Fallback policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public FallbackPolicyBase IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<FallbackPolicyBase, TInnerException>(predicate);

		public FallbackPolicyBase ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyBase, TException>(func);

		public FallbackPolicyBase ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<FallbackPolicyBase>(expression);

		public FallbackPolicyBase ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<FallbackPolicyBase, TException1, TException2>();

		public FallbackPolicyBase ExcludeErrorSet(IErrorSet errorSet) => this.ExcludeErrorSet<FallbackPolicyBase>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the handling by the Fallback policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public FallbackPolicyBase ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<FallbackPolicyBase, TInnerException>(predicate);

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

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{FallbackPolicyBase, T}"/>
		public FallbackPolicyBase SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}
	}
}
