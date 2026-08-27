using System;

namespace PoliNorError
{
	internal class ResultFilterSlim<T>
	{
		public Func<T, bool> CanHandle { get; }

		internal ResultFilterSlim(ResultFilter<T> resultFilter)
		{
			CanHandle = resultFilter.GetCanHandle();
		}
	}
}
