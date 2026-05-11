using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Sentinel type used as the dictionary key in <c>_syncGenericFuncsHolder</c> and
	/// <c>_asyncGenericFuncsHolder</c> to store the non-generic <see cref="FallbackFuncsProvider.Fallback"/>
	/// and <see cref="FallbackFuncsProvider.FallbackAsync"/> delegates.
	/// </summary>
	internal struct VoidType { }

	internal interface IFallbackGenericFuncHolder { }

	/// <summary>
	/// Marker interface for holders stored in the parameterized dictionary
	/// keyed by <c>(TParam, T)</c>.
	/// </summary>
	internal interface IFallbackParamGenericFuncHolder { }

	internal class SyncFallbackGenericFuncHolder<T> : IFallbackGenericFuncHolder
	{
		public SyncFallbackGenericFuncHolder(Func<CancellationToken, T> fun) => Fun = fun;
		public Func<CancellationToken, T> Fun { get; }
	}

	/// <summary>
	/// Holds a <see cref="Func{TParam, CancellationToken, T}"/> delegate.
	/// Stored in the <c>_paramSyncGenericFuncsHolder</c> dictionary keyed by
	/// <c>(typeof(TParam), typeof(T))</c>.
	/// </summary>
	internal class SyncFallbackParamGenericFuncHolder<TParam, T> : IFallbackParamGenericFuncHolder
	{
		public SyncFallbackParamGenericFuncHolder(Func<TParam, CancellationToken, T> fun) => Fun = fun;

		public Func<TParam, CancellationToken, T> Fun { get; }
	}

	internal class AsyncFallbackGenericFuncHolder<T> : IFallbackGenericFuncHolder
	{
		public AsyncFallbackGenericFuncHolder(Func<CancellationToken, Task<T>> asyncFun) => AsyncFun = asyncFun;
		public Func<CancellationToken, Task<T>> AsyncFun { get; }
	}

	/// <summary>
	/// Holds an async delegate.
	/// Stored in the <c>_paramAsyncGenericFuncsHolder</c> dictionary keyed by
	/// <c>(typeof(TParam), typeof(T))</c>.
	/// </summary>
	internal class AsyncFallbackParamGenericFuncHolder<TParam, T> : IFallbackParamGenericFuncHolder
	{
		public AsyncFallbackParamGenericFuncHolder(Func<TParam, CancellationToken, Task<T>> asyncFun) => AsyncFun = asyncFun;
		public Func<TParam, CancellationToken, Task<T>> AsyncFun { get; }
	}

	/// <summary>
	/// Holds the non-generic <see cref="Action{CancellationToken}"/> fallback delegate,
	/// stored under the <see cref="VoidType"/> key in <c>_syncGenericFuncsHolder</c>.
	/// </summary>
	internal class SyncFallbackActionHolder : IFallbackGenericFuncHolder
	{
		public SyncFallbackActionHolder(Action<CancellationToken> action) => Action = action;
		public Action<CancellationToken> Action { get; }
	}

	/// <summary>
	/// Holds the non-generic <see cref="Func{CancellationToken, Task}"/> fallback delegate,
	/// stored under the <see cref="VoidType"/> key in <c>_asyncGenericFuncsHolder</c>.
	/// </summary>
	internal class AsyncFallbackFuncHolder : IFallbackGenericFuncHolder
	{
		public AsyncFallbackFuncHolder(Func<CancellationToken, Task> func) => Func = func;
		public Func<CancellationToken, Task> Func { get; }
	}

	/// <summary>
	/// Marker interface for holders stored in the parameterized action dictionary
	/// keyed by <c>TParam</c>.
	/// </summary>
	internal interface IFallbackParamFuncHolder { }

	/// <summary>
	/// Holds an <see cref="Action{TParam, CancellationToken}"/> fallback delegate.
	/// Stored in <c>_paramFallbackActionHolder</c> keyed by <c>typeof(TParam)</c>.
	/// </summary>
	internal class SyncFallbackParamActionHolder<TParam> : IFallbackParamFuncHolder
	{
		public SyncFallbackParamActionHolder(Action<TParam, CancellationToken> action) => Action = action;
		public Action<TParam, CancellationToken> Action { get; }
	}
}
