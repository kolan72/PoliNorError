using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IPolicyBaseExtensions
	{
		public static Task<PolicyResult> HandleAsync(this IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token)
		{
			return policyBase.HandleAsync(func, false, token);
		}

		public static Task<PolicyResult<T>> HandleAsync<T>(this IPolicyBase policyBase, Func<CancellationToken, Task<T>> func, CancellationToken token)
		{
			return policyBase.HandleAsync(func, false, token);
		}

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

		public static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate(errorPolicy);
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

		public static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate<T>(errorPolicy);
		}
	}
}
