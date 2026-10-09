namespace PoliNorError
{
	/// <summary>
	/// Packs a <see cref="Policy"/> with a delegate to handle.
	/// </summary>
	public abstract class PolicyDelegateBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PolicyDelegateBase"/> class with the specified policy.
		/// </summary>
		private protected PolicyDelegateBase(IPolicyBase policy)
		{
			Policy = policy;
		}

		internal SingleDelegateContainerBase DelegateContainer { get; private protected set; }

		/// <summary>
		/// Gets the policy associated with this delegate.
		/// </summary>
		public IPolicyBase Policy { get; }

		/// <summary>
		/// Gets a value indicating whether a delegate is set for this policy delegate.
		/// </summary>
		public virtual bool DelegateExists => DelegateContainer?.DelegateExists == true;

		/// <summary>
		/// Gets the sync type of the delegate associated with this policy delegate.
		/// </summary>
		public SyncPolicyDelegateType SyncType => GetSyncType();

		/// <summary>
		/// Clears the delegate associated with this policy delegate.
		/// </summary>
		public void ClearDelegate() => DelegateContainer?.ClearDelegate();

		/// <summary>
		/// Returns the sync type of the delegate.
		/// </summary>
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

	/// <summary>
	/// Specifies whether the delegate is synchronous, asynchronous, or not set.
	/// </summary>
	public enum SyncPolicyDelegateType
	{
		/// <summary>
		/// No delegate is set.
		/// </summary>
		None = 0,

		/// <summary>
		/// A synchronous delegate is set.
		/// </summary>
		Sync,

		/// <summary>
		/// An asynchronous delegate is set.
		/// </summary>
		Async
	}
}
