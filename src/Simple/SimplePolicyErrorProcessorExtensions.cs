namespace PoliNorError
{
	internal static class SimplePolicyErrorProcessorExtensions
	{
		public static SimplePolicy ToSimplePolicy(this PolicyErrorProcessor policyParams)
		{
			return (SimplePolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new SimplePolicy());
		}
	}
}
