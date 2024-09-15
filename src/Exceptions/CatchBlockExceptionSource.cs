namespace PoliNorError
{
	/// <summary>
	/// Represents the source of the thrown CatchBlockException.
	/// </summary>
	public enum CatchBlockExceptionSource
	{
		/// <summary>
		/// The default value.
		/// </summary>
		Unknown,
		/// <summary>
		/// Saving handling exception thrown.
		/// </summary>
		ErrorSaver,
		/// <summary>
		/// Exception thrown in error processor during processing by <see cref="IBulkErrorProcessor"/>.
		/// </summary>
		ErrorProcessor,
		/// <summary>
		/// Exception thrown when trying to apply a policy rule.
		/// </summary>
		PolicyRule,
		/// <summary>
		///  Exception thrown when trying to apply an error filter.
		/// </summary>
		ErrorFilter
	}
}
