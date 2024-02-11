using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IFallbackGenericFuncHolder { }

	internal class SyncFallbackGenericFuncHolder<T> : IFallbackGenericFuncHolder
	{
		public SyncFallbackGenericFuncHolder(Func<CancellationToken, T> fun) => Fun = fun;
		public Func<CancellationToken, T> Fun { get; }
	}

	internal class AsyncFallbackGenericFuncHolder<T> : IFallbackGenericFuncHolder
	{
		public AsyncFallbackGenericFuncHolder(Func<CancellationToken, Task<T>> asyncFun) => AsyncFun = asyncFun;
		public Func<CancellationToken, Task<T>> AsyncFun { get; }
	}
}
