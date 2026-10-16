using System;
using System.Threading;

namespace PoliNorError
{
#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This interface is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
	internal interface IFallBackFuncHolder
	{
		Func<CancellationToken, T> GetFallbackFunc<T>();
	}

#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This class is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
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
