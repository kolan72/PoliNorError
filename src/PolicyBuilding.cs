using System;

namespace PoliNorError
{
	/// <summary>
	/// Provides extra static methods to build policy that inherits from <see cref="Policy"></see>
	/// </summary>
	public static class PolicyBuilding
	{
		public static T WrapPolicy<T>(this T errorPolicyBase, IPolicyBase wrappedPolicy) where T : Policy
		{
			if (errorPolicyBase._wrappedPolicy != null)
			{
				throw new NotImplementedException("More than one wrapped policy is not supported.");
			}
			errorPolicyBase._wrappedPolicy = wrappedPolicy;
			return errorPolicyBase;
		}

		public static T WithPolicyName<T>(this T errorPolicyBase, string policyName) where T : Policy
		{
			errorPolicyBase.PolicyName = policyName;
			return errorPolicyBase;
		}
	}
}
