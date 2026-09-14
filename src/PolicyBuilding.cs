using System;
using System.Collections.Generic;

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

		public static T WrapPolicyCollection<T>(this T errorPolicyBase, IEnumerable<IPolicyBase> wrappedPolicyCollection, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed = ThrowOnWrappedCollectionFailed.LastError) where T : Policy
		{
			errorPolicyBase.SetWrap(wrappedPolicyCollection, throwOnWrappedCollectionFailed);
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
		/// Wraps the current policy inside the specified wrapper policy and returns the wrapper policy.
		/// This is a shorthand for <see cref="WrapUp{TWrapperPolicy}"/> that returns the outer policy directly.
		/// </summary>
		/// <typeparam name="TWrapperPolicy">The type of the policy that will wrap the current policy.</typeparam>
		/// <param name="policy">The policy to be wrapped.</param>
		/// <param name="wrapperPolicy">The policy that will wrap the current policy.</param>
		/// <returns>The wrapper policy that now wraps the current policy.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="wrapperPolicy"/> is <c>null</c>.</exception>
		public static TWrapperPolicy Then<TWrapperPolicy>(this IPolicyBase policy, TWrapperPolicy wrapperPolicy) where TWrapperPolicy : Policy
		{
			return policy.WrapUp(wrapperPolicy).OuterPolicy;
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
