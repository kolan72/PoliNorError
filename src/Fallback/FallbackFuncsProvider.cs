using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Registers and provides <see cref="FallbackPolicy"/> delegates.
	/// </summary>
	public class FallbackFuncsProvider
	{
		/// <summary>
		/// Creates a new instance of <see cref="FallbackFuncsProvider"/>. Use this method for handling only generic delegates
		/// </summary>
		/// <returns></returns>
		public static FallbackFuncsProvider Create() => new FallbackFuncsProvider(true);

		///<inheritdoc cref = "Create(Func{CancellationToken, Task},  Action{CancellationToken}, bool)"/>
		public static FallbackFuncsProvider Create(Func<CancellationToken, Task> fallbackAsync, bool onlyGenericFallbackForGenericDelegate = false)
									=> Create(fallbackAsync, null, onlyGenericFallbackForGenericDelegate);

		///<inheritdoc cref = "Create(Func{CancellationToken, Task}, Action{CancellationToken}, bool)"/>
		public static FallbackFuncsProvider Create(Action<CancellationToken> fallback, bool onlyGenericFallbackForGenericDelegate = false)
									=> Create(null, fallback, onlyGenericFallbackForGenericDelegate);

		/// <summary>
		/// Creates a new instance of <see cref="FallbackFuncsProvider"/>.
		/// </summary>
		/// <param name="fallbackAsync">An async fallback delegate.</param>
		/// <param name="fallback">A fallback delegate.</param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.</param>
		/// <returns></returns>
		public static FallbackFuncsProvider Create(Func<CancellationToken, Task> fallbackAsync, Action<CancellationToken> fallback, bool onlyGenericFallbackForGenericDelegate = false)
									=> new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate) {FallbackAsync = fallbackAsync, Fallback = fallback};

		internal FallbackFuncsProvider(bool onlyGenericFallbackForGenericDelegate)
		{
			OnlyGenericFallbackForGenericDelegate = onlyGenericFallbackForGenericDelegate;
		}

		internal Action<CancellationToken> Fallback { get; set; }
		internal Func<CancellationToken, Task> FallbackAsync { get; set; }

		private readonly Dictionary<Type, IFallbackGenericFuncHolder> _syncGenericFuncsHolder = new Dictionary<Type, IFallbackGenericFuncHolder>();
		private readonly Dictionary<Type, IFallbackGenericFuncHolder> _asyncGenericFuncsHolder = new Dictionary<Type, IFallbackGenericFuncHolder>();

		private static Func<CancellationToken, T> DefaulFallbackFunc<T>() => (_) => default;
		private static Func<CancellationToken, Task<T>> DefaulFallbackAsyncFunc<T>() => (_) => Task.FromResult(default(T));

		private static Action<CancellationToken> DefaultFallbackAction => (_) => Expression.Empty();
		private static Func<CancellationToken, Task> DefaultFallbackAsyncFunc => (_) => Task.CompletedTask;

		/// <summary>
		/// Adds or replaces a generic fallback delegate to the internal fallback delegate store, pre-converting it to the Func&lt;CancellationToken, T&gt; delegate.
		/// </summary>
		/// <typeparam name="T">A return type of fallback delegate.</typeparam>
		/// <param name="fallbackFunc">A fallback delegate to store.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackFunc<T>(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackFunc(fallbackFunc, convertType);
			return this;
		}

		/// <summary>
		/// Adds or replaces a generic fallback delegate to the internal fallback delegate store.
		/// </summary>
		/// <typeparam name="T">A return type of fallback delegate.</typeparam>
		/// <param name="fallbackFunc">A fallback delegate to store.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackFunc<T>(Func<CancellationToken, T> fallbackFunc)
		{
			SetFallbackFunc(fallbackFunc);
			return this;
		}

		/// <summary>
		/// Adds or replaces a generic async fallback delegate to the internal fallback delegate store, pre-converting it to the Func&lt;CancellationToken, &lt;Task&lt;T&gt;&gt; delegate.
		/// </summary>
		/// <typeparam name="T">A return type of fallback delegate.</typeparam>
		/// <param name="fallbackAsync">A fallback delegate to store.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc(fallbackAsync, convertType);
			return this;
		}

		/// <summary>
		/// Adds or replaces a generic async fallback delegate to the internal fallback delegate store.
		/// </summary>
		/// <typeparam name="T">A return type of fallback delegate.</typeparam>
		/// <param name="fallbackAsync">A fallback delegate to store.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync)
		{
			SetAsyncFallbackFunc(fallbackAsync);
			return this;
		}

		protected void SetFallbackAction(Action action, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackAction(convertType == CancellationType.Precancelable ? action.ToPrecancelableAction(true) : action.ToCancelableAction());
		}

		protected void SetFallbackAction(Action<CancellationToken> action)
		{
			Fallback = action;
		}

		protected void SetAsyncFallbackFunc(Func<Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc(convertType == CancellationType.Precancelable ? func.ToPrecancelableFunc(true) : func.ToCancelableFunc());
		}

		protected void SetAsyncFallbackFunc(Func<CancellationToken, Task> func)
		{
			FallbackAsync = func;
		}

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
			else if (!OnlyGenericFallbackForGenericDelegate)
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
			else if (!OnlyGenericFallbackForGenericDelegate)
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

		internal bool OnlyGenericFallbackForGenericDelegate { get; }
	}
}
