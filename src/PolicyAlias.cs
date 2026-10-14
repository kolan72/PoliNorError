namespace PoliNorError
{
	/// <summary>
	/// Defines the alias of a policy.
	/// </summary>
	public enum PolicyAlias
	{
		/// <summary>
		/// The policy alias has not been set.
		/// </summary>
		NotSet,
		/// <summary>
		/// The alias of a retry policy.
		/// </summary>
		Retry,
		/// <summary>
		/// The alias of a fallback policy.
		/// </summary>
		Fallback,
		/// <summary>
		/// The alias of a simple policy.
		/// </summary>
		Simple
	}
}
