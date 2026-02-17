using System;

namespace PoliNorError
{
	public static class PolicyTelemetryExtensions
	{
		public static T WithOperationKey<T>(this T policy, string operationKey) where T : Policy
		{
			PolicyRuntimeMetadata.SetOperationKey(policy, operationKey);
			return policy;
		}

		public static T WithTelemetry<T>(this T policy, Action<PolicyTelemetryEvent> handler) where T : Policy
		{
			PolicyRuntimeMetadata.SetEventSink(policy, new DelegatePolicyEventSink(handler));
			return policy;
		}
	}
}
