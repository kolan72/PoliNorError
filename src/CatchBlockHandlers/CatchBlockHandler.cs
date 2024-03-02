namespace PoliNorError
{
#pragma warning disable RCS1225 // Make class sealed.
	public class CatchBlockHandler : ICanAddErrorProcessor
#pragma warning restore RCS1225 // Make class sealed.
	{
		private CatchBlockHandler(CatchBlockFilter catchBlockFilter)
		{
			CatchBlockFilter = catchBlockFilter;
		}

		public static CatchBlockHandler FilterExceptionBy(CatchBlockFilter catchBlockFilter) => new CatchBlockHandler(catchBlockFilter);

		public static CatchBlockHandler ForAllExceptions() => new CatchBlockHandler(CatchBlockFilter.Empty());

		internal CatchBlockFilter CatchBlockFilter { get; }

		internal PolicyProcessor.ExceptionFilter ErrorFilter => CatchBlockFilter.ErrorFilter;

		internal IBulkErrorProcessor BulkErrorProcessor { get; } = new BulkErrorProcessor();
	}
}
