namespace PoliNorError
{
	internal sealed class LastPolicyResultState
	{
		private LastPolicyResultState() { }

		public bool? IsCanceled { get; private set; }
		public bool? IsFailed { get; private set; }

		public static LastPolicyResultState FromPolicyResult(PolicyResult policyResult) => new LastPolicyResultState() { IsCanceled = policyResult?.IsCanceled, IsFailed = policyResult?.IsFailed };

		public static LastPolicyResultState FromCanceled() => new LastPolicyResultState() { IsCanceled = true };

		public static LastPolicyResultState FromFailed() => new LastPolicyResultState() { IsFailed = true };

		public static LastPolicyResultState Default() => new LastPolicyResultState();
	}
}
