namespace PoliNorError
{
	internal static class SimpleInvokeParamsExtensions
	{
		public static SimplePolicy ToSimplePolicy(this PolicyErrorProcessor policyParams)
		{
			return (SimplePolicy)(policyParams ?? PolicyErrorProcessor.Default()).ConfigurePolicy(new SimplePolicy());
		}
	}
}
