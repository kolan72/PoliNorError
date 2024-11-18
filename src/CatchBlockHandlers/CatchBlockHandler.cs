namespace PoliNorError
{
	/// <summary>
	/// Creates classes that inherit from the <see cref="CatchBlockHandler"/> class.
	/// </summary>
	public static class CatchBlockHandlerFactory
	{
		/// <summary>
		/// Creates <see cref="CatchBlockFilteredHandler"/> that filters an exception using <paramref name="catchBlockFilter"/>.
		/// </summary>
		/// <param name="catchBlockFilter"><see cref="PoliNorError.CatchBlockFilter"/></param>
		/// <returns></returns>
		public static CatchBlockFilteredHandler FilterExceptionsBy(NonEmptyCatchBlockFilter catchBlockFilter) => new CatchBlockFilteredHandler(catchBlockFilter);

		/// <summary>
		/// Creates a <see cref="CatchBlockFilteredHandler"/> that filters an exception using a filter that includes error types from <paramref name="errorSet"/>.
		/// </summary>
		/// <param name="errorSet">ErrorSet to include in a filter.</param>
		/// <returns></returns>
		public static CatchBlockFilteredHandler FilterExceptionsByIncluding(IErrorSet errorSet) => new CatchBlockFilteredHandler(NonEmptyCatchBlockFilter.CreateByIncluding(errorSet));

		/// <summary>
		/// Creates a <see cref="CatchBlockFilteredHandler"/> that filters an exception using a filter that excludes error types from <paramref name="errorSet"/>.
		/// </summary>
		/// <param name="errorSet">ErrorSet to exclude from a filter.</param>
		/// <returns></returns>
		public static CatchBlockFilteredHandler FilterExceptionsByExcluding(IErrorSet errorSet) => new CatchBlockFilteredHandler(NonEmptyCatchBlockFilter.CreateByExcluding(errorSet));

		/// <summary>
		/// Creates <see cref="CatchBlockForAllHandler"/> with an empty <see cref="PoliNorError.CatchBlockFilter"/>.
		/// </summary>
		/// <returns></returns>
		public static CatchBlockForAllHandler ForAllExceptions() => new CatchBlockForAllHandler();
	}

	/// <summary>
	/// Contains a filter and error processors that are applied to an exception within the catch block.
	/// </summary>
	public abstract class CatchBlockHandler : ICanAddErrorProcessor
	{
		protected CatchBlockHandler(CatchBlockFilter catchBlockFilter)
		{
			CatchBlockFilter = catchBlockFilter;
		}

		internal CatchBlockFilter CatchBlockFilter { get; }

		internal PolicyProcessor.ExceptionFilter ErrorFilter => CatchBlockFilter.ErrorFilter;

		internal void SetBulkErrorProcessor(IBulkErrorProcessor processor)
		{
			BulkErrorProcessor = processor;
		}

		internal IBulkErrorProcessor BulkErrorProcessor { get; private set; } = new BulkErrorProcessor();
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
