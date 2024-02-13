namespace PoliNorError
{
	internal static class SimplePolicyErrorProcessorExtensions
	{
		public static SimplePolicy ToSimplePolicy(this ErrorProcessorParam policyParams)
		{
			return (SimplePolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new SimplePolicy());
		}

		public static SimplePolicy ToSimplePolicy(this ErrorProcessorParam policyParams, ErrorFilter errorFilter, IBulkErrorProcessor processor = null, bool rethrowIfErrorFilterUnsatisfied = false)
		{
			return (SimplePolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new SimplePolicy(errorFilter, processor, rethrowIfErrorFilterUnsatisfied));
		}
	}
}
