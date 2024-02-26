using System;
using System.Threading;

namespace PoliNorError
{
	internal interface IFallBackFuncHolder
	{
		Func<CancellationToken, T> GetFallbackFunc<T>();
	}

	internal class FallBackFuncHolder<U> : IFallBackFuncHolder
	{
		private readonly Func<CancellationToken, U> _func;

		public FallBackFuncHolder(Func<CancellationToken, U> func)
		{
			_func = func;
		}

		public Func<CancellationToken, T> GetFallbackFunc<T>()
		{
			if (typeof(T) != typeof(U))
			{
				return null;
			}
			return (ctx) => BoxingSafeConverter<U, T>.Instance.Convert(_func(ctx));
		}
	}
}
