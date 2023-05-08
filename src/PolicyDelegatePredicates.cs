using System;

namespace PoliNorError
{
	internal static class PolicyDelegatePredicates
	{
		internal static readonly Func<PolicyDelegateBase, bool> WithDelegateFunc = si => si.DelegateExists;
	}
}
