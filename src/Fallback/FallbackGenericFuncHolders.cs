using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
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
}
