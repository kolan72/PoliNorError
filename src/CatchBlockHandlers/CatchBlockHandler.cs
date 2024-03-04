namespace PoliNorError
{
	/// <summary>
	/// Contains <see cref="PoliNorError.CatchBlockFilter"/> and error processors that are applied to an exception within the catch block.
	/// </summary>
#pragma warning disable RCS1225 // Make class sealed.
	public class CatchBlockHandler : ICanAddErrorProcessor
#pragma warning restore RCS1225 // Make class sealed.
	{
		private CatchBlockHandler(CatchBlockFilter catchBlockFilter)
		{
			CatchBlockFilter = catchBlockFilter;
		}

		/// <summary>
		/// Creates <see cref="CatchBlockHandler"/> that filters an exception using <paramref name="catchBlockFilter"/>.
		/// </summary>
		/// <param name="catchBlockFilter"><see cref="PoliNorError.CatchBlockFilter"/></param>
		/// <returns></returns>
		public static CatchBlockHandler FilterExceptionBy(CatchBlockFilter catchBlockFilter) => new CatchBlockHandler(catchBlockFilter);

		/// <summary>
		/// Creates <see cref="CatchBlockHandler"/> with an empty <see cref="PoliNorError.CatchBlockFilter"/>.
		/// </summary>
		/// <returns></returns>
		public static CatchBlockHandler ForAllExceptions() => new CatchBlockHandler(CatchBlockFilter.Empty());

		internal CatchBlockFilter CatchBlockFilter { get; }

		internal PolicyProcessor.ExceptionFilter ErrorFilter => CatchBlockFilter.ErrorFilter;

		internal IBulkErrorProcessor BulkErrorProcessor { get; } = new BulkErrorProcessor();
	}
}
