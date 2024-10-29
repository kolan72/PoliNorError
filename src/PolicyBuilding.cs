using System;

namespace PoliNorError
{
	/// <summary>
	/// Provides extra extension methods to build policy that inherits from <see cref="Policy"></see>
	/// </summary>
	public static class PolicyBuilding
	{
		/// <summary>
		/// Wraps another policy.
		/// </summary>
		/// <typeparam name="T">Type of policy that wraps another policy.</typeparam>
		/// <param name="errorPolicyBase">Policy that wraps wrappedPolicy</param>
		/// <param name="wrappedPolicy">Policy to be wrapped.</param>
		/// <returns></returns>
		public static T WrapPolicy<T>(this T errorPolicyBase, IPolicyBase wrappedPolicy) where T : Policy
		{
			errorPolicyBase.SetWrap(wrappedPolicy);
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

		/// <summary>
		/// Gives a name to the <typeparamref name="T"/> policy.
		/// </summary>
		/// <typeparam name="T">Type of policy.</typeparam>
		/// <param name="errorPolicyBase">Policy that will have a name.</param>
		/// <param name="policyName">Policy name.</param>
		/// <returns></returns>
		public static T WithPolicyName<T>(this T errorPolicyBase, string policyName) where T : Policy
		{
			errorPolicyBase.PolicyName = policyName;
			return errorPolicyBase;
		}
	}
}
