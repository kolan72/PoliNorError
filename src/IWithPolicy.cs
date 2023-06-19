using System;

namespace PoliNorError
{
	public interface IWithPolicy<T> where T : IWithPolicy<T>
	{
		T WithPolicy(IPolicyBase policyBase);
	}

	public static class IWithPolicyExtensions
	{
		public static Func<IPolicyBase, T> ToWithPolicyFunc<T>(this T t) where T : IWithPolicy<T>
		{
			return (p) => t.WithPolicy(p);
		}
	}
}
