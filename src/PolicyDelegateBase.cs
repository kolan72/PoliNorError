namespace PoliNorError
{
	public abstract class PolicyDelegateBase
	{
		private protected PolicyDelegateBase(IPolicyBase policy)
		{
			Policy = policy;
		}

		public IPolicyBase Policy { get; }

		public abstract bool DelegateExists { get; }

		public SyncPolicyDelegateType UseSync => GetSyncType();

		protected abstract SyncPolicyDelegateType GetSyncType();
	}
}
