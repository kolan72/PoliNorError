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

		internal Action<CancellationToken> Fallback
		{
			get
			{
				if (!_syncGenericFuncsHolder.TryGetValue(typeof(VoidType), out var holder))
				{
					return null;
				}
				return ((SyncFallbackActionHolder)holder).Action;
			}
			set
			{
				if (value == null)
					_syncGenericFuncsHolder.Remove(typeof(VoidType));
				else
					_syncGenericFuncsHolder[typeof(VoidType)] = new SyncFallbackActionHolder(value);
			}
		}

		internal Func<CancellationToken, Task> FallbackAsync
		{
			get
			{
				if (!_asyncGenericFuncsHolder.TryGetValue(typeof(VoidType), out var holder))
				{
					return null;
				}
				return ((AsyncFallbackFuncHolder)holder).Func;
			}
			set
			{
				if (value == null)
					_asyncGenericFuncsHolder.Remove(typeof(VoidType));
				else
					_asyncGenericFuncsHolder[typeof(VoidType)] = new AsyncFallbackFuncHolder(value);
			}
		}

		// Keyed by (TParam, T) — stores Func<TParam, CancellationToken, T> delegates.
		// Separate from _syncGenericFuncsHolder so parameterized and non-parameterized
		// registrations for the same T coexist without ambiguity.
		private readonly Dictionary<(Type ParamType, Type ReturnType), IFallbackParamGenericFuncHolder> _paramSyncGenericFuncsHolder
			= new Dictionary<(Type, Type), IFallbackParamGenericFuncHolder>();

		// Keyed by (TParam, T) — stores Func<TParam, CancellationToken, Task<T>> delegates.
		private readonly Dictionary<(Type ParamType, Type ReturnType), IFallbackParamGenericFuncHolder> _paramAsyncGenericFuncsHolder
			= new Dictionary<(Type, Type), IFallbackParamGenericFuncHolder>();

		// Keyed by TParam — stores Action<TParam, CancellationToken> delegates.
		private readonly Dictionary<Type, IFallbackParamFuncHolder> _paramFallbackActionHolder
			= new Dictionary<Type, IFallbackParamFuncHolder>();

		// Keyed by TParam — stores Func<TParam, CancellationToken, Task> delegates.
		private readonly Dictionary<Type, IFallbackParamFuncHolder> _paramAsyncFallbackFuncHolder
			= new Dictionary<Type, IFallbackParamFuncHolder>();

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
		/// Adds or replaces a parameterized generic fallback delegate that accepts both the parameter and a
		/// <see cref="CancellationToken"/> to the internal fallback delegate store.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback delegate.</typeparam>
		/// <typeparam name="T">The return type of the fallback delegate.</typeparam>
		/// <param name="fallbackFunc">A fallback delegate to store.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackFunc<TParam, T>(Func<TParam, CancellationToken, T> fallbackFunc)
		{
			SetFallbackFunc(fallbackFunc);
			return this;
		}

		/// <summary>
		/// Adds or replaces a parameterized generic fallback delegate to the internal fallback delegate store,
		/// pre-converting <paramref name="fallbackFunc"/> to a <see cref="Func{TParam, CancellationToken, T}"/>
		/// according to <paramref name="convertType"/>.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback delegate.</typeparam>
		/// <typeparam name="T">The return type of the fallback delegate.</typeparam>
		/// <param name="fallbackFunc">A fallback delegate to store.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackFunc<TParam, T>(Func<TParam, T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackFunc(fallbackFunc, convertType);
			return this;
		}

		/// <summary>
		/// Adds or replaces a parameterized fallback action that accepts both the parameter and a
		/// <see cref="CancellationToken"/> to the internal fallback delegate store.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback action.</typeparam>
		/// <param name="fallbackAction">A fallback action to store.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackAction<TParam>(Action<TParam, CancellationToken> fallbackAction)
		{
			SetFallbackAction(fallbackAction);
			return this;
		}

		/// <summary>
		/// Adds or replaces a parameterized fallback action to the internal fallback delegate store,
		/// pre-converting <paramref name="fallbackAction"/> to an <see cref="Action{TParam, CancellationToken}"/>
		/// according to <paramref name="convertType"/>.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback action.</typeparam>
		/// <param name="fallbackAction">A fallback action to store.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackAction<TParam>(Action<TParam> fallbackAction, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackAction(fallbackAction, convertType);
			return this;
		}

		/// <summary>
		/// Adds or replaces a generic async fallback delegate to the internal fallback delegate store, pre-converting it to the Func&lt;CancellationToken, &lt;Task&lt;T&gt;&gt; delegate.
		/// </summary>
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

		/// <summary>
		/// Adds or replaces a parameterized generic async fallback delegate that accepts both the parameter and a
		/// <see cref="CancellationToken"/> to the internal fallback delegate store.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback delegate.</typeparam>
		/// <typeparam name="T">The return type of the fallback delegate.</typeparam>
		/// <param name="fallbackAsync">A fallback delegate to store.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceAsyncFallbackFunc<TParam, T>(Func<TParam, CancellationToken, Task<T>> fallbackAsync)
		{
			SetAsyncFallbackFunc(fallbackAsync);
			return this;
		}

		/// <summary>
		/// Adds or replaces a parameterized generic async fallback delegate to the internal fallback delegate store,
		/// pre-converting <paramref name="fallbackAsync"/> to a cancellable delegate
		/// according to <paramref name="convertType"/>.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback delegate.</typeparam>
		/// <typeparam name="T">The return type of the fallback delegate.</typeparam>
		/// <param name="fallbackAsync">A fallback delegate to store.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceAsyncFallbackFunc<TParam, T>(Func<TParam, Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc(fallbackAsync, convertType);
			return this;
		}

		/// <summary>
		/// Adds or replaces a parameterized async fallback delegate that accepts both the parameter and a
		/// <see cref="CancellationToken"/> to the internal fallback delegate store.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback delegate.</typeparam>
		/// <param name="fallbackFunc">A fallback delegate to store.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceAsyncFallbackFunc<TParam>(Func<TParam, CancellationToken, Task> fallbackFunc)
		{
			SetAsyncFallbackFunc(fallbackFunc);
			return this;
		}

		/// <summary>
		/// Adds or replaces a parameterized async fallback delegate to the internal fallback delegate store,
		/// pre-converting <paramref name="fallbackFunc"/> to a <see cref="Func{TParam, CancellationToken, Task}"/>
		/// according to <paramref name="convertType"/>.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the fallback delegate.</typeparam>
		/// <param name="fallbackFunc">A fallback delegate to store.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceAsyncFallbackFunc<TParam>(Func<TParam, Task> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc(fallbackFunc, convertType);
			return this;
		}

		/// <summary>
		/// Creates a <see cref="FallbackPolicy"/>.
		/// </summary>
		/// <returns></returns>
		public FallbackPolicy ToFallbackPolicy()
		{
			return new FallbackPolicy(this);
		}

		/// <summary>
		/// Adds or replaces a generic fallback delegate from a <see cref="FallbackBehavior{T}"/>.
		/// </summary>
		/// <typeparam name="T">A return type of fallback delegate.</typeparam>
		/// <param name="fallbackBehavior">A <see cref="FallbackBehavior{T}"/> containing the fallback delegates to add.</param>
		/// <returns></returns>
		public FallbackFuncsProvider AddOrReplaceFallbackBehavior<T>(FallbackBehavior<T> fallbackBehavior)
		{
			if (fallbackBehavior == null)
			{
				return this;
			}

			if (fallbackBehavior.Fallback != null)
			{
				SetFallbackFunc(fallbackBehavior.Fallback);
			}

			if (fallbackBehavior.AsyncFallback != null)
			{
				SetAsyncFallbackFunc(fallbackBehavior.AsyncFallback);
			}

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

		internal void SetFallbackFunc<TParam, T>(Func<TParam, T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackFunc.ToPrecancelableFunc(true) : fallbackFunc.ToCancelableFunc());
		}

		internal void SetFallbackFunc<TParam, T>(Func<TParam, CancellationToken, T> fallbackFunc)
		{
			_paramSyncGenericFuncsHolder[(typeof(TParam), typeof(T))] = new SyncFallbackParamGenericFuncHolder<TParam, T>(fallbackFunc);
		}

		internal void SetFallbackAction<TParam>(Action<TParam> fallbackAction, CancellationType convertType = CancellationType.Precancelable)
		{
			SetFallbackAction((convertType == CancellationType.Precancelable) ? fallbackAction.ToPrecancelableAction() : fallbackAction.ToCancelableAction());
		}

		internal void SetFallbackAction<TParam>(Action<TParam, CancellationToken> fallbackAction)
		{
			_paramFallbackActionHolder[typeof(TParam)] = new SyncFallbackParamActionHolder<TParam>(fallbackAction);
		}

		internal void SetAsyncFallbackFunc<T>(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc(true) : fallbackAsync.ToCancelableFunc());
		}

		internal void SetAsyncFallbackFunc<T>(Func<CancellationToken, Task<T>> fallbackAsync)
		{
			_asyncGenericFuncsHolder[typeof(T)] = new AsyncFallbackGenericFuncHolder<T>(fallbackAsync);
		}

		internal void SetAsyncFallbackFunc<TParam, T>(Func<TParam, Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc() : fallbackAsync.ToCancelableFunc());
		}

		internal void SetAsyncFallbackFunc<TParam, T>(Func<TParam, CancellationToken, Task<T>> fallbackAsync)
		{
			_paramAsyncGenericFuncsHolder[(typeof(TParam), typeof(T))] = new AsyncFallbackParamGenericFuncHolder<TParam, T>(fallbackAsync);
		}

		internal void SetAsyncFallbackFunc<TParam>(Func<TParam, Task> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			SetAsyncFallbackFunc((convertType == CancellationType.Precancelable) ? fallbackFunc.ToPrecancelableFunc() : fallbackFunc.ToCancelableFunc());
		}

		internal void SetAsyncFallbackFunc<TParam>(Func<TParam, CancellationToken, Task> fallbackFunc)
		{
			_paramAsyncFallbackFuncHolder[typeof(TParam)] = new AsyncFallbackParamFuncHolder<TParam>(fallbackFunc);
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

		/// <summary>
		/// Retrieves the stored <see cref="Func{TParam, CancellationToken, T}"/> fallback delegate with
		/// <paramref name="param"/> applied, returning a <see cref="Func{CancellationToken, T}"/> ready
		/// for execution. Falls back to the non-parameterized path when no parameterized delegate is
		/// registered for the <c>(TParam, T)</c> combination.
		/// </summary>
		internal Func<CancellationToken, T> GetFallbackFunc<TParam, T>(TParam param)
		{
			if (_paramSyncGenericFuncsHolder.TryGetValue((typeof(TParam), typeof(T)), out var holder))
				return ((SyncFallbackParamGenericFuncHolder<TParam, T>)holder).Fun.Apply(param);

			return GetFallbackFunc<T>();
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

		/// <summary>
		/// Retrieves the stored <see cref="Action{TParam, CancellationToken}"/> fallback action with
		/// <paramref name="param"/> applied, returning an <see cref="Action{CancellationToken}"/> ready
		/// for execution. Falls back to the non-parameterized path when no parameterized action is
		/// registered for <typeparamref name="TParam"/>.
		/// </summary>
		internal Action<CancellationToken> GetFallbackAction<TParam>(TParam param)
		{
			if (_paramFallbackActionHolder.TryGetValue(typeof(TParam), out var holder))
				return ((SyncFallbackParamActionHolder<TParam>)holder).Action.Apply(param);

			return GetFallbackAction();
		}

		/// <summary>
		/// Retrieves the stored <see cref="Func{TParam, CancellationToken, Task}"/> async fallback delegate with
		/// <paramref name="param"/> applied, returning a <see cref="Func{CancellationToken, Task}"/> ready
		/// for execution. Falls back to the non-parameterized path when no parameterized delegate is
		/// registered for <typeparamref name="TParam"/>.
		/// </summary>
		internal Func<CancellationToken, Task> GetAsyncFallbackFunc<TParam>(TParam param)
		{
			if (_paramAsyncFallbackFuncHolder.TryGetValue(typeof(TParam), out var holder))
				return ((AsyncFallbackParamFuncHolder<TParam>)holder).Func.Apply(param);

			return GetAsyncFallbackFunc();
		}

		/// <summary>
		/// Retrieves the stored async fallback delegate with
		/// <paramref name="param"/> applied, returning a async delegate ready
		/// for execution. Falls back to the non-parameterized path when no parameterized delegate is
		/// registered for the <c>(TParam, T)</c> combination.
		/// </summary>
		internal Func<CancellationToken, Task<T>> GetAsyncFallbackFunc<TParam, T>(TParam param, bool configureAwait)
		{
			if (_paramAsyncGenericFuncsHolder.TryGetValue((typeof(TParam), typeof(T)), out var holder))
				return ((AsyncFallbackParamGenericFuncHolder<TParam, T>)holder).AsyncFun.Apply(param);

			return GetAsyncFallbackFunc<T>(configureAwait);
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

		internal bool HasParamFallbackFunc<TParam, T>() => _paramSyncGenericFuncsHolder.ContainsKey((typeof(TParam), typeof(T)));

		internal bool HasParamFallbackAction<TParam>() => _paramFallbackActionHolder.ContainsKey(typeof(TParam));

		internal bool HasAsyncParamFallbackFunc<TParam, T>() => _paramAsyncGenericFuncsHolder.ContainsKey((typeof(TParam), typeof(T)));

		internal bool HasAsyncParamFallbackFunc<TParam>() => _paramAsyncFallbackFuncHolder.ContainsKey(typeof(TParam));

		internal bool HasFallbackAction() => _syncGenericFuncsHolder.ContainsKey(typeof(VoidType));

		internal bool HasAsyncFallbackFunc<T>() => _asyncGenericFuncsHolder.ContainsKey(typeof(T));

		internal bool HasAsyncFallbackFunc() => _asyncGenericFuncsHolder.ContainsKey(typeof(VoidType));

		internal bool OnlyGenericFallbackForGenericDelegate { get; }
	}
}
