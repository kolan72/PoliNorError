namespace PoliNorError
{
	public static class FallbackProcessor
	{
		public static IFallbackProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor = null) => new DefaultFallbackProcessor(bulkErrorProcessor);
	}
}
