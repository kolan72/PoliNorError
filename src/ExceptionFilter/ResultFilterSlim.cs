using System;

namespace PoliNorError
{
	internal class ResultFilterSlim<T>
	{
		public Func<T, bool> IsSuccessful { get; }

		internal ResultFilterSlim(ResultFilter<T> resultFilter)
		{
			IsSuccessful = resultFilter.GetIsSuccessful();
		}
	}
}