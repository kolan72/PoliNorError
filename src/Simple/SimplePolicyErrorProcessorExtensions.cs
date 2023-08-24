namespace PoliNorError
{
	internal static class SimplePolicyErrorProcessorExtensions
	{
		public static SimplePolicy ToSimplePolicy(this ErrorProcessorParam policyParams)
		{
			return (SimplePolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new SimplePolicy());
		}
	}
}
