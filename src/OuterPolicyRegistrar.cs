namespace PoliNorError
{
	public class OuterPolicyRegistrar<TWrapperPolicy> where TWrapperPolicy : Policy
	{
		internal OuterPolicyRegistrar(TWrapperPolicy wrapperPolicy, IPolicyBase policyBase)
		{
			OuterPolicy = wrapperPolicy;
			OuterPolicy._wrappedPolicy = policyBase;
		}

		public TWrapperPolicy OuterPolicy { get; }
	}
}
