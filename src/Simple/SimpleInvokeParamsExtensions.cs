namespace PoliNorError
{
	internal static class SimpleInvokeParamsExtensions
	{
		public static SimplePolicy ToSimplePolicy(this InvokeParams policyParams)
		{
			return (SimplePolicy)(policyParams ?? InvokeParams.Default()).ConfigurePolicy(new SimplePolicy());
		}
	}
}
