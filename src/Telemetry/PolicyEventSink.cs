using System;
using System.Runtime.CompilerServices;

namespace PoliNorError
{
	internal interface IPolicyEventSink
	{
		void Publish(PolicyTelemetryEvent policyTelemetryEvent);
	}

	internal sealed class DelegatePolicyEventSink : IPolicyEventSink
	{
		private readonly Action<PolicyTelemetryEvent> _handler;

		internal DelegatePolicyEventSink(Action<PolicyTelemetryEvent> handler)
		{
			_handler = handler;
		}

		public void Publish(PolicyTelemetryEvent policyTelemetryEvent)
		{
			_handler?.Invoke(policyTelemetryEvent);
		}
	}

	internal static class PolicyRuntimeMetadata
	{
		private sealed class Bag
		{
			public string OperationKey;
			public IPolicyEventSink EventSink;
		}

		private static readonly ConditionalWeakTable<Policy, Bag> _bags = new ConditionalWeakTable<Policy, Bag>();

		internal static void SetOperationKey(Policy policy, string operationKey)
		{
			_bags.GetOrCreateValue(policy).OperationKey = operationKey;
		}

		internal static string GetOperationKey(Policy policy)
		{
			return _bags.TryGetValue(policy, out var bag) ? bag.OperationKey : null;
		}

		internal static void SetEventSink(Policy policy, IPolicyEventSink eventSink)
		{
			_bags.GetOrCreateValue(policy).EventSink = eventSink;
		}

		internal static IPolicyEventSink GetEventSink(Policy policy)
		{
			return _bags.TryGetValue(policy, out var bag) ? bag.EventSink : null;
		}
	}
}
