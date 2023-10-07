namespace PoliNorError
{
	/// <summary>
	/// The way in which an exception will be generated if the last policy in the PolicyCollection fails.
	/// </summary>
	public enum ThrowOnWrappedCollectionFailed
	{
		None,
		/// <summary>
		/// The last <see cref="PolicyResult.UnprocessedError"/>  exception will be thrown.
		/// </summary>
		LastError,
		/// <summary>
		/// The <see cref="PolicyDelegateCollectionException"/>  exception will be thrown.
		/// </summary>
		CollectionError
	}
}
