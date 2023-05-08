using System;

namespace PoliNorError
{
	public static class Predicates
	{
		public static Func<T, bool> Not<T>(Func<T, bool> func)
		{
			return (t) => !func(t);
		}
	}
}
