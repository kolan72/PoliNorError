namespace PoliNorError
{
	/// <summary>
	/// Represents a registrar for a wrapper policy that wraps the other policy
	/// </summary>
	/// <typeparam name="TWrapperPolicy">The type of the wrapper policy</typeparam>
	public class OuterPolicyRegistrar<TWrapperPolicy> where TWrapperPolicy : Policy
	{
		internal OuterPolicyRegistrar(TWrapperPolicy wrapperPolicy, IPolicyBase policyBase)
		{
			OuterPolicy = wrapperPolicy;
			OuterPolicy.SetWrap(policyBase);
		}

		internal OuterPolicyRegistrar(TWrapperPolicy wrapperPolicy, PolicyCollection policies, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			OuterPolicy = wrapperPolicy;
			OuterPolicy.SetWrap(policies, throwOnWrappedCollectionFailed);
		}

		/// <summary>
		/// Returns a  wrapper policy that has wrapped the other policy.
		/// </summary>
		public TWrapperPolicy OuterPolicy { get; }
	}
}
