namespace PoliNorError
{
	/// <summary>
	/// Shows how cancellation will be performed for a delegate that is not cancelable.
	/// </summary>
	public enum CancellationType
	{
		/// <summary>
		/// The execution will not be done if a token has already been canceled.
		/// </summary>
		Precancelable,
		/// <summary>
		/// The execution will be made cancelable. Usually it means that a new task that supports cancellation will be used.
		/// </summary>
		Cancelable
	}
}
