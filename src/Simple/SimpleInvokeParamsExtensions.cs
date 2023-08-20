namespace PoliNorError
{
	internal static class SimpleInvokeParamsExtensions
	{
		public static SimplePolicy ToSimplePolicy(this PolicyErrorProcessor policyParams)
		{
			return (SimplePolicy)policyParams.GetValueOrDefault().ConfigurePolicy(new SimplePolicy());
		}
	}
}
