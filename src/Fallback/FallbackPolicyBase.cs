using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract class FallbackPolicyBase : HandleErrorPolicyBase, IFallbackPolicy
	{
		internal IFallbackProcessor _fallbackProcessor;

		internal Action<CancellationToken> _fallback;
		internal Func<CancellationToken, Task> _fallbackAsync;

		internal Dictionary<Type, IFallBackFuncHolder> _holders = new Dictionary<Type, IFallBackFuncHolder>();
		internal Dictionary<Type, IFallBackAsyncFuncHolder> _asyncHolders = new Dictionary<Type, IFallBackAsyncFuncHolder>();

		private static Func<CancellationToken, T> DefaulFallbackFunc<T>() => (_) => default;
		private static Func<CancellationToken, Task<T>> DefaulFallbackAsyncFunc<T>() => (_) => Task.FromResult(default(T));

		private static Action<CancellationToken> DefaultFallbackAction => (_) => Expression.Empty();
		private static Func<CancellationToken, Task> DefaultFallbackAsyncFunc => (_) => Task.CompletedTask;

		protected FallbackPolicyBase(IFallbackProcessor processor) : base(processor)
		{
			_fallbackProcessor = processor;
		}

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			if (action == null)
				return PolicyResult.ForSync().SetFailedWithError(new NoDelegateException(this));

			PolicyResult fallBackRes = null;
			Action<CancellationToken> curFallback = null;
			if (_fallback == null)
			{
				if (_fallbackAsync != null)
				{
					curFallback = _fallbackAsync.ToSyncFunc();
				}
				else
				{
					curFallback = DefaultFallbackAction;
				}
			}
			else
			{
				curFallback = _fallback;
			}

			if (_wrappedPolicy == null)
			{
				fallBackRes = _fallbackProcessor.Fallback(action, curFallback, token);
			}
			else
			{
				var wrapper = new PolicyWrapper(_wrappedPolicy, action, token);
				Action actionWrapped = wrapper.Handle;

				fallBackRes = _fallbackProcessor.Fallback(actionWrapped, curFallback, token);
				fallBackRes.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}

			HandlePolicyResult(fallBackRes, token);
			return fallBackRes;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult<T>.ForSync().SetFailedWithError(new NoDelegateException(this));

			Func<CancellationToken, T> fallBackFunc = null;
			if (HasFallbackFunc<T>())
			{
				fallBackFunc = _holders[typeof(T)].GetFallbackFunc<T>();
			}
			else if (HasAsyncFallbackFunc<T>())
			{
				fallBackFunc = _asyncHolders[typeof(T)].GetFallbackAsyncFunc<T>().ToSyncFunc();
			}
			else if (HasFallbackAction())
			{
				fallBackFunc = _fallback.ToDefaultReturnFunc<T>();
			}
			else
			{
				fallBackFunc = DefaulFallbackFunc<T>();
			}

			PolicyResult<T> fallbackResult = null;

			if (_wrappedPolicy == null)
			{
				fallbackResult = _fallbackProcessor.Fallback(func, fallBackFunc, token);
			}
			else
			{
				var wrapper = new PolicyWrapper<T>(_wrappedPolicy, func, token);
				Func<T> funcWrapped = wrapper.Handle;

				fallbackResult = _fallbackProcessor.Fallback(funcWrapped, fallBackFunc, token);

				fallbackResult.WrappedPolicyResults = wrapper.PolicyResults.Select(pr => pr.ToPolicyDelegateResult());
			}

			HandlePolicyResult(fallbackResult, token);
			return fallbackResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult.ForNotSync().SetFailedWithError(new NoDelegateException(this));

			PolicyResult fallBackRes = null;
			Func<CancellationToken, Task> curFallbackAsync = null;

			if (_fallbackAsync == null)
			{
				if (_fallback != null)
				{
					curFallbackAsync = _fallback.ToAsyncFunc();
				}
				else
				{
					curFallbackAsync = DefaultFallbackAsyncFunc;
				}
			}
			else
			{
				curFallbackAsync = _fallbackAsync;
			}

			if (_wrappedPolicy == null)
			{
				fallBackRes = await _fallbackProcessor.FallbackAsync(func, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				var wrapper = new PolicyWrapper(_wrappedPolicy, func, token, configureAwait);
				Func<CancellationToken, Task> funcWrapped = wrapper.HandleAsync;

				fallBackRes = await _fallbackProcessor.FallbackAsync(funcWrapped, curFallbackAsync, configureAwait, token).ConfigureAwait(configureAwait);

				fallBackRes.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}

			await HandlePolicyResultAsync(fallBackRes, configureAwait, token).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult<T>.ForNotSync().SetFailedWithError(new NoDelegateException(this));

			Func<CancellationToken, Task<T>> fallBackAsyncFunc = null;

			if (HasAsyncFallbackFunc<T>())
			{
				fallBackAsyncFunc = _asyncHolders[typeof(T)].GetFallbackAsyncFunc<T>();
			}
			else if (HasFallbackFunc<T>())
			{
				fallBackAsyncFunc = _holders[typeof(T)].GetFallbackFunc<T>().ToAsyncFunc();
			}
			else if (HasAsyncFallbackFunc())
			{
				fallBackAsyncFunc = _fallbackAsync.ToDefaultReturnFunc<T>(configureAwait);
			}
			else
			{
				fallBackAsyncFunc = DefaulFallbackAsyncFunc<T>();
			}

			PolicyResult<T> fallBackRes = null;

			if (_wrappedPolicy == null)
			{
				fallBackRes = await _fallbackProcessor.FallbackAsync(func, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				var wrapper = new PolicyWrapper<T>(_wrappedPolicy, func, token, configureAwait);
				Func<CancellationToken, Task<T>> funcWrapped = wrapper.HandleAsync;

				fallBackRes = await _fallbackProcessor.FallbackAsync(funcWrapped, fallBackAsyncFunc, configureAwait, token).ConfigureAwait(configureAwait);
				fallBackRes.WrappedPolicyResults = wrapper.PolicyResults.Select(pr => pr.ToPolicyDelegateResult());
			}
			await HandlePolicyResultAsync(fallBackRes, configureAwait).ConfigureAwait(configureAwait);
			return fallBackRes;
		}

		public bool HasFallbackFunc<T>() => _holders.ContainsKey(typeof(T));

		public bool HasFallbackAction() => _fallback != null;

		public bool HasAsyncFallbackFunc<T>() => _asyncHolders.ContainsKey(typeof(T));

		public bool HasAsyncFallbackFunc() => _fallbackAsync != null;

		public FallbackPolicyBase WithFallbackFunc<T>(Func<T> fallbackFunc, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithFallbackFunc<FallbackPolicyBase, T>(fallbackFunc, convertType);

		public FallbackPolicyBase WithFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc) => this.WithFallbackFunc<FallbackPolicyBase, T>(fallbackFunc);

		public FallbackPolicyBase WithAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) => this.WithAsyncFallbackFunc<FallbackPolicyBase, T>(fallbackAsync, convertType);

		public FallbackPolicyBase WithAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync) => this.WithAsyncFallbackFunc<FallbackPolicyBase, T>(fallbackAsync);

		public FallbackPolicyBase IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<FallbackPolicyBase, TException>(func);

		public FallbackPolicyBase ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<FallbackPolicyBase, TException>(func);
	}
}
