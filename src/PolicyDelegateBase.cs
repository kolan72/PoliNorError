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

		public SyncPolicyDelegateType SyncType => GetSyncType();

		public void ClearDelegate() => DelegateContainer?.ClearDelegate();

		protected abstract SyncPolicyDelegateType GetSyncType();
	}

	internal static class PolicyDelegateBaseExtensions
	{
		public static bool IsNotNullAndWithoutDelegate(this PolicyDelegateBase delegateInfo)
		{
			return (delegateInfo != null) && Predicates.Not(PolicyDelegatePredicates.WithDelegateFunc)(delegateInfo);
		}

		public static bool IsNotNullAndWithDelegate(this PolicyDelegateBase delegateInfo)
		{
			return (delegateInfo != null) && PolicyDelegatePredicates.WithDelegateFunc(delegateInfo);
		}
	}

	public enum SyncPolicyDelegateType
	{
		None = 0,
		Sync,
		Async
	}
}
