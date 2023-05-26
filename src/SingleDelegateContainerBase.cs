namespace PoliNorError
{
	internal abstract class SingleDelegateContainerBase
	{
		public SyncPolicyDelegateType UseSync { get; protected set; } = SyncPolicyDelegateType.None;

		public bool DelegateExists => UseSync != SyncPolicyDelegateType.None;
	}
}
