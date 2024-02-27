namespace PoliNorError
{
#pragma warning disable RCS1225 // Make class sealed.
	public class CatchBlockHandler : ICanAddErrorProcessor
#pragma warning restore RCS1225 // Make class sealed.
	{
		private readonly CatchBlockFilter _catchBlockFilter;

		private CatchBlockHandler(CatchBlockFilter catchBlockFilter)
		{
			_catchBlockFilter = catchBlockFilter;
		}

		public static CatchBlockHandler FilterExceptionBy(CatchBlockFilter catchBlockFilter) => new CatchBlockHandler(catchBlockFilter);

		public static CatchBlockHandler ForAllExceptions() => new CatchBlockHandler(new CatchBlockFilter());

		internal PolicyProcessor.ExceptionFilter ErrorFilter => _catchBlockFilter.ErrorFilter;

		internal IBulkErrorProcessor BulkErrorProcessor { get; } = new BulkErrorProcessor();
	}
}
