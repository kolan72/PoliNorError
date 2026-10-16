using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This interface is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
	internal interface IFallBackAsyncFuncHolder
	{
		Func<CancellationToken, Task<T>> GetFallbackAsyncFunc<T>();
	}

#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This class is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
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
