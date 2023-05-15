using System;

namespace PoliNorError
{
	internal static class Predicates
	{
		public static Func<T, bool> Not<T>(Func<T, bool> func)
		{
			return (t) => !func(t);
		}
	}
}
