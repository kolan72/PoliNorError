namespace PoliNorError
{
	/// <summary>
	/// Contains a filter and error processors that are applied to an exception within the catch block.
	/// </summary>
	public abstract class CatchBlockHandler : ICanAddErrorProcessor
	{
		protected CatchBlockHandler(CatchBlockFilter catchBlockFilter)
		{
			CatchBlockFilter = catchBlockFilter;
		}

		/// <summary>
		/// Creates <see cref="CatchBlockFilteredHandler"/> that filters an exception using <paramref name="catchBlockFilter"/>.
		/// </summary>
		/// <param name="catchBlockFilter"><see cref="PoliNorError.CatchBlockFilter"/></param>
		/// <returns></returns>
		public static CatchBlockFilteredHandler FilterExceptionsBy(NonEmptyCatchBlockFilter catchBlockFilter) => new CatchBlockFilteredHandler(catchBlockFilter);

		/// <summary>
		/// Creates <see cref="CatchBlockForAllHandler"/> with an empty <see cref="PoliNorError.CatchBlockFilter"/>.
		/// </summary>
		/// <returns></returns>
		public static CatchBlockForAllHandler ForAllExceptions() => new CatchBlockForAllHandler();

		internal CatchBlockFilter CatchBlockFilter { get; }

		internal PolicyProcessor.ExceptionFilter ErrorFilter => CatchBlockFilter.ErrorFilter;

		internal IBulkErrorProcessor BulkErrorProcessor { get; } = new BulkErrorProcessor();
	}

	/// <summary>
	/// Contains an empty <see cref="PoliNorError.CatchBlockFilter"/> filter and error processors that are applied to an exception within the catch block.
	/// </summary>
	public class CatchBlockForAllHandler : CatchBlockHandler
	{
		internal CatchBlockForAllHandler() : base(CatchBlockFilter.Empty()){}
	}

	/// <summary>
	/// Contains a non-empty <see cref="PoliNorError.NonEmptyCatchBlockFilter"/> filter and error processors that are applied to an exception within the catch block.
	/// </summary>
	public class CatchBlockFilteredHandler : CatchBlockHandler
	{
		internal CatchBlockFilteredHandler(NonEmptyCatchBlockFilter nonEmptyCatchBlockFilter) : base(nonEmptyCatchBlockFilter){}
	}
}
