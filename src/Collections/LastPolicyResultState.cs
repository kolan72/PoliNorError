namespace PoliNorError
{
	internal sealed class LastPolicyResultState
	{
		internal LastPolicyResultState() { }

		public bool? IsCanceled { get; internal set; }
		public bool? IsFailed { get; internal set; }

		public static LastPolicyResultState FromCanceled() => new LastPolicyResultState() { IsCanceled = true };

		public static LastPolicyResultState FromFailed() => new LastPolicyResultState() { IsFailed = true };

		public static LastPolicyResultState Default() => new LastPolicyResultState();
	}
}
