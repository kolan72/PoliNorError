using System;

namespace PoliNorError
{
	/// <summary>
	/// Provides extra extension methods to build policy that inherits from <see cref="Policy"></see>
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

		/// <summary>
		/// Returns an <see cref="OuterPolicyRegistrar{Policy}"></see> with the <see cref="OuterPolicyRegistrar{Policy}.OuterPolicy"></see> that wraps the current policy.
		/// </summary>
		/// <typeparam name="TWrapperPolicy"></typeparam>
		/// <param name="policy">The policy that will be wrapped</param>
		/// <param name="wrapperPolicy">The policy that will wrap the current policy</param>
		/// <returns></returns>
		public static OuterPolicyRegistrar<TWrapperPolicy> WrapUp<TWrapperPolicy>(this IPolicyBase policy, TWrapperPolicy wrapperPolicy) where TWrapperPolicy : Policy
		{
			if(wrapperPolicy == null)
			{
				throw new ArgumentNullException(nameof(wrapperPolicy));
			}
			return new OuterPolicyRegistrar<TWrapperPolicy>(wrapperPolicy, policy);
		}

		public static T WithPolicyName<T>(this T errorPolicyBase, string policyName) where T : Policy
		{
			errorPolicyBase.PolicyName = policyName;
			return errorPolicyBase;
		}
	}
}
