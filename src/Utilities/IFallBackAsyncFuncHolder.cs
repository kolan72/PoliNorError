using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal interface IFallBackAsyncFuncHolder
	{
		Func<CancellationToken, Task<T>> GetFallbackAsyncFunc<T>();
	}

	internal class FallBackAsyncFuncHolder<U> : IFallBackAsyncFuncHolder
	{
		private readonly Func<CancellationToken, Task<U>> _func;

		public FallBackAsyncFuncHolder(Func<CancellationToken, Task<U>> func)
		{
			_func = func;
		}

		public Func<CancellationToken, Task<T>> GetFallbackAsyncFunc<T>()
		{
			if (typeof(T) != typeof(U))
			{
				return null;
			}
			return async (ctx) => BoxingSafeConverter<U, T>.Instance.Convert(await _func(ctx));
		}
	}
}
