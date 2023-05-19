namespace PoliNorError
{
	public abstract class PolicyDelegateBase
	{
		private protected PolicyDelegateBase(IPolicyBase policy)
		{
			Policy = policy;
		}

		internal SingleDelegateContainerBase DelegateContainer { get; set; }

		public IPolicyBase Policy { get; }

		public virtual bool DelegateExists => DelegateContainer?.DelegateExists == true;

		public SyncPolicyDelegateType UseSync => GetSyncType();

		protected abstract SyncPolicyDelegateType GetSyncType();
	}
}
