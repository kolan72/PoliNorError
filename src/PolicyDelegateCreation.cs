using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of extension methods to create a <see cref="PolicyDelegate"></see> or <see cref="PolicyDelegate{T}"/> from policy
	/// </summary>
	public static class PolicyDelegateCreation
	{
		public static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy, Action action)
		{
			var res = new PolicyDelegate(errorPolicy);
			res.SetDelegate(action);
			return res;
		}

		public static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy, Func<CancellationToken, Task> func)
		{
			var res = new PolicyDelegate(errorPolicy);
			res.SetDelegate(func);
			return res;
		}

		public static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy, Func<T> func)
		{
			var res = new PolicyDelegate<T>(errorPolicy);
			res.SetDelegate(func);
			return res;
		}

		public static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy, Func<CancellationToken, Task<T>> func)
		{
			var res = new PolicyDelegate<T>(errorPolicy);
			res.SetDelegate(func);
			return res;
		}

		internal static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate(errorPolicy);
		}

		internal static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate<T>(errorPolicy);
		}
	}
}
