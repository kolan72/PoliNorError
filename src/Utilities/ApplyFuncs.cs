using System;

namespace PoliNorError
{
	/// <summary>
	/// Use methods of this class to turn a many-args delegate into a zero or one-arg one.
	/// </summary>
	public static class ApplyFuncs
	{
		public static Action Apply<T>(this Action<T> func, T t1)
					=> () => func(t1);

		public static Action<T2> Apply<T1, T2>(this Action<T1, T2> func, T1 t1)
					=> (t2) => func(t1, t2);

		public static Func<R> Apply<T1, R>(this Func<T1, R> func, T1 t1)
				=> () => func(t1);

		public static Action<T2, T3> Apply<T1, T2, T3>(this Action<T1, T2, T3> func, T1 t1)
					=> (t2, t3) => func(t1, t2, t3);

		public static Func<T2, R> Apply<T1, T2, R>(this Func<T1, T2, R> func, T1 t1)
					=> t2 => func(t1, t2);

		public static Func<T2, T3, R> Apply<T1, T2, T3, R>(this Func<T1, T2, T3, R> func, T1 t1)
					=> (t2, t3) => func(t1, t2, t3);

		public static Action<T3> BulkApply<T1, T2, T3>(this Action<T1, T2, T3> func, T1 t1, T2 t2) => func.Apply(t1).Apply(t2);

		public static Func<T3, R> BulkApply<T1, T2, T3, R>(this Func<T1, T2, T3, R> func, T1 t1, T2 t2) => func.Apply(t1).Apply(t2);
	}
}
