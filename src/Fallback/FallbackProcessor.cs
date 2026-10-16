namespace PoliNorError
{
	/// <summary>
	/// A fallback processor that can create default fallback processor instances.
	/// </summary>
	public static class FallbackProcessor
	{
		/// <summary>
		/// Creates and returns the <see cref="DefaultFallbackProcessor"/> class, which represents the default implementation of the <see cref="IFallbackProcessor"/> interface.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <returns></returns>
		public static IFallbackProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor = null) => new DefaultFallbackProcessor(bulkErrorProcessor);
	}
}
