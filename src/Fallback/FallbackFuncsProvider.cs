using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class FallbackFuncsProvider
	{
		internal Action<CancellationToken> Fallback { get; set; }
		internal Func<CancellationToken, Task> FallbackAsync { get; set; }

		private readonly Dictionary<Type, IFallBackFuncHolder> _holders = new Dictionary<Type, IFallBackFuncHolder>();
		private readonly Dictionary<Type, IFallBackAsyncFuncHolder> _asyncHolders = new Dictionary<Type, IFallBackAsyncFuncHolder>();

		private static Func<CancellationToken, T> DefaulFallbackFunc<T>() => (_) => default;
		private static Func<CancellationToken, Task<T>> DefaulFallbackAsyncFunc<T>() => (_) => Task.FromResult(default(T));

		private static Action<CancellationToken> DefaultFallbackAction => (_) => Expression.Empty();
		private static Func<CancellationToken, Task> DefaultFallbackAsyncFunc => (_) => Task.CompletedTask;

		internal void SetFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackFunc.ToPrecancelableFunc(true) : fallbackFunc.ToCancelableFunc());
		}

		internal void SetFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc)
		{
			_holders[typeof(T)] = new FallBackFuncHolder<T>(fallbackFunc);
		}

		internal void SetAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc(true) : fallbackAsync.ToCancelableFunc());
		}

		internal void SetAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync)
		{
			_asyncHolders[typeof(T)] = new FallBackAsyncFuncHolder<T>(fallbackAsync);
		}

		internal Action<CancellationToken> GetFallbackAction()
		{
			Action<CancellationToken> curFallback = null;
			if (Fallback == null)
			{
				if (HasAsyncFallbackFunc())
				{
					curFallback = FallbackAsync.ToSyncFunc();
				}
				else
				{
					curFallback = DefaultFallbackAction;
				}
			}
			else
			{
				curFallback = Fallback;
			}
			return curFallback;
		}

		internal Func<CancellationToken, T> GetFallbackFunc<T>()
		{
			if (HasFallbackFunc<T>())
			{
				return _holders[typeof(T)].GetFallbackFunc<T>();
			}
			else if (HasAsyncFallbackFunc<T>())
			{
				return _asyncHolders[typeof(T)].GetFallbackAsyncFunc<T>().ToSyncFunc();
			}
			else if (HasFallbackAction())
			{
				return Fallback.ToDefaultReturnFunc<T>();
			}
			else
			{
				return DefaulFallbackFunc<T>();
			}
		}

		internal Func<CancellationToken, Task> GetAsyncFallbackFunc()
		{
			Func<CancellationToken, Task> curFallbackAsync = null;

			if (FallbackAsync == null)
			{
				if (HasFallbackAction())
				{
					curFallbackAsync = Fallback.ToTaskReturnFunc();
				}
				else
				{
					curFallbackAsync = DefaultFallbackAsyncFunc;
				}
			}
			else
			{
				curFallbackAsync = FallbackAsync;
			}
			return curFallbackAsync;
		}

		internal Func<CancellationToken, Task<T>> GetAsyncFallbackFunc<T>(bool configureAwait)
		{
			if (HasAsyncFallbackFunc<T>())
			{
				return _asyncHolders[typeof(T)].GetFallbackAsyncFunc<T>();
			}
			else if (HasFallbackFunc<T>())
			{
				return _holders[typeof(T)].GetFallbackFunc<T>().ToTaskReturnFunc();
			}
			else if (HasAsyncFallbackFunc())
			{
				return FallbackAsync.ToDefaultReturnFunc<T>(configureAwait);
			}
			else
			{
				return DefaulFallbackAsyncFunc<T>();
			}
		}

		internal bool HasFallbackFunc<T>() => _holders.ContainsKey(typeof(T));

		internal bool HasFallbackAction() => Fallback != null;

		internal bool HasAsyncFallbackFunc<T>() => _asyncHolders.ContainsKey(typeof(T));

		internal bool HasAsyncFallbackFunc() => FallbackAsync != null;
	}
}
