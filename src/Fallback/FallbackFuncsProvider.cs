using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class FallbackFuncsProvider
	{
		private readonly bool _onlyGenericFallbackForGenericDelegate;

		internal FallbackFuncsProvider(bool onlyGenericFallbackForGenericDelegate)
		{
			_onlyGenericFallbackForGenericDelegate = onlyGenericFallbackForGenericDelegate;
		}

		internal Action<CancellationToken> Fallback { get; set; }
		internal Func<CancellationToken, Task> FallbackAsync { get; set; }

		private readonly Dictionary<Type, IFallbackGenericFuncHolder> _syncGenericFuncsHolder = new Dictionary<Type, IFallbackGenericFuncHolder>();
		private readonly Dictionary<Type, IFallbackGenericFuncHolder> _asyncGenericFuncsHolder = new Dictionary<Type, IFallbackGenericFuncHolder>();

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
			_syncGenericFuncsHolder[typeof(T)] = new SyncFallbackGenericFuncHolder<T>(fallbackFunc);
		}

		internal void SetAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc(true) : fallbackAsync.ToCancelableFunc());
		}

		internal void SetAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync)
		{
			_asyncGenericFuncsHolder[typeof(T)] = new AsyncFallbackGenericFuncHolder<T>(fallbackAsync);
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
				return ((SyncFallbackGenericFuncHolder<T>)_syncGenericFuncsHolder[typeof(T)]).Fun;
			}
			else if (HasAsyncFallbackFunc<T>())
			{
				return ((AsyncFallbackGenericFuncHolder<T>)_asyncGenericFuncsHolder[typeof(T)]).AsyncFun.ToSyncFunc();
			}
			else if (!_onlyGenericFallbackForGenericDelegate)
			{
				if (HasFallbackAction())
				{
					return Fallback.ToDefaultReturnFunc<T>();
				}
				else if (HasAsyncFallbackFunc())
				{
					return FallbackAsync.ToSyncFunc().ToDefaultReturnFunc<T>();
				}
				else
				{
					return DefaulFallbackFunc<T>();
				}
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
				return ((AsyncFallbackGenericFuncHolder<T>)_asyncGenericFuncsHolder[typeof(T)]).AsyncFun;
			}
			else if (HasFallbackFunc<T>())
			{
				return ((SyncFallbackGenericFuncHolder<T>)_syncGenericFuncsHolder[typeof(T)]).Fun.ToTaskReturnFunc();
			}
			else if (!_onlyGenericFallbackForGenericDelegate)
			{
				if (HasAsyncFallbackFunc())
				{
					return FallbackAsync.ToDefaultReturnFunc<T>(configureAwait);
				}
				else if (HasFallbackAction())
				{
					return Fallback.ToDefaultReturnFunc<T>().ToTaskReturnFunc();
				}
				else
				{
					return DefaulFallbackAsyncFunc<T>();
				}
			}
			else
			{
				return DefaulFallbackAsyncFunc<T>();
			}
		}

		internal bool HasFallbackFunc<T>() => _syncGenericFuncsHolder.ContainsKey(typeof(T));

		internal bool HasFallbackAction() => Fallback != null;

		internal bool HasAsyncFallbackFunc<T>() => _asyncGenericFuncsHolder.ContainsKey(typeof(T));

		internal bool HasAsyncFallbackFunc() => FallbackAsync != null;
	}
}
