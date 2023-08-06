namespace PoliNorError
{
	internal static class SimpleInvokeParamsExtensions
	{
		public static SimplePolicy ToSimplePolicy(this ErrorProcessorDelegate policyParams)
		{
			return (SimplePolicy)(policyParams ?? ErrorProcessorDelegate.Default()).ConfigurePolicy(new SimplePolicy());
		}
	}
}
