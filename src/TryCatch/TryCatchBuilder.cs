using System.Collections.Generic;

namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Default implementation of <see cref="ITryCatchBuilder"/>.
	/// </summary>
	public sealed class TryCatchBuilder : ITryCatchBuilder
	{
		private readonly List<CatchBlockHandler> _catchBlockHandlers;
		private bool _hasCatchBlockForAll;

		private TryCatchBuilder()
		{
			_catchBlockHandlers = new List<CatchBlockHandler>();
		}

		/// <summary>
		/// Creates <see cref="ITryCatch"/> which mimics a try/catch block without a filter that will swallow any exception.
		/// </summary>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch CreateAndBuild()
		{
			return CreateFrom(CatchBlockHandlerFactory.ForAllExceptions()).Build();
		}

		/// <summary>
		/// Creates a <see cref="TryCatchBuilder"/> class based on the <see cref="CatchBlockFilteredHandler"/>. Other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="filteredHandler"><see cref="CatchBlockFilteredHandler"/></param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public static TryCatchBuilder CreateFrom(CatchBlockFilteredHandler filteredHandler)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(filteredHandler);
		}

		/// <summary>
		/// Creates a <see cref="TryCatchBuilder"/> class based on the  <see cref="NonEmptyCatchBlockFilter"/> and the <see cref="IBulkErrorProcessor"/>. Other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="nonEmptyCatchBlockFilter"><see cref="NonEmptyCatchBlockFilter"/></param>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public static TryCatchBuilder CreateFrom(NonEmptyCatchBlockFilter nonEmptyCatchBlockFilter, IBulkErrorProcessor bulkErrorProcessor)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(nonEmptyCatchBlockFilter, bulkErrorProcessor);
		}

		/// <summary>
		/// Creates a <see cref="TryCatchBuilder"/> class based on the  <see cref="NonEmptyCatchBlockFilter"/>.
		/// The <see cref="TryCatchBuilder"/> created contains a <see cref="CatchBlockFilteredHandler"/> that mimics a try/catch block that swallows an exception.
		/// </summary>
		/// <param name="nonEmptyCatchBlockFilter"></param>
		/// <returns></returns>
		public static TryCatchBuilder CreateFrom(NonEmptyCatchBlockFilter nonEmptyCatchBlockFilter)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(nonEmptyCatchBlockFilter);
		}

		/// <summary>
		/// Creates a <see cref="ITryCatchBuilder"/> class based on the <see cref="CatchBlockForAllHandler"/>. No other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="filteredHandler"><see cref="CatchBlockForAllHandler"/></param>
		/// <returns><see cref="ITryCatchBuilder"/></returns>
		public static ITryCatchBuilder CreateFrom(CatchBlockForAllHandler filteredHandler)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(filteredHandler);
		}

		/// <summary>
		/// Creates a <see cref="ITryCatchBuilder"/> class based on the <see cref="CatchBlockForAllHandler"/> with the <paramref name="bulkErrorProcessor"/>. No other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <returns><see cref="ITryCatchBuilder"/></returns>
		public static ITryCatchBuilder CreateFrom(IBulkErrorProcessor bulkErrorProcessor)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(bulkErrorProcessor);
		}

		/// <summary>
		/// Adds <see cref="CatchBlockFilteredHandler"/> handler to builder. Other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="filteredHandler"><see cref="CatchBlockFilteredHandler"/></param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public TryCatchBuilder AddCatchBlock(CatchBlockFilteredHandler filteredHandler)
		{
			_catchBlockHandlers.Add(filteredHandler);
			return this;
		}

		/// <summary>
		/// Adds <see cref="CatchBlockForAllHandler"/> handler to builder. No other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="filteredHandler"><see cref="CatchBlockForAllHandler"/></param>
		/// <returns><see cref="ITryCatchBuilder"/></returns>
		public ITryCatchBuilder AddCatchBlock(CatchBlockForAllHandler filteredHandler)
		{
			_catchBlockHandlers.Add(filteredHandler);
			_hasCatchBlockForAll = true;
			return this;
		}

		/// <summary>
		/// Adds <see cref="CatchBlockForAllHandler"/> handler with the <paramref name="bulkErrorProcessor"/>. No other <see cref="CatchBlockHandler"/> handlers may be added to the created object.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <returns><see cref="ITryCatchBuilder"/></returns>
		public ITryCatchBuilder AddCatchBlock(IBulkErrorProcessor bulkErrorProcessor)
		{
			var handlerToAdd = CatchBlockHandlerFactory.ForAllExceptions();
			handlerToAdd.SetBulkErrorProcessor(bulkErrorProcessor);
			return AddCatchBlock(handlerToAdd);
		}

		/// <summary>
		/// Adds a <see cref="CatchBlockFilteredHandler"/> handler consisting of <see cref="NonEmptyCatchBlockFilter"/> and <see cref="IBulkErrorProcessor"/> to builder.
		/// </summary>
		/// <param name="nonEmptyCatchBlockFilter"><see cref="NonEmptyCatchBlockFilter"/></param>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public TryCatchBuilder AddCatchBlock(NonEmptyCatchBlockFilter nonEmptyCatchBlockFilter, IBulkErrorProcessor bulkErrorProcessor)
		{
			var handler = new CatchBlockFilteredHandler(nonEmptyCatchBlockFilter);
			handler.SetBulkErrorProcessor(bulkErrorProcessor);
			return AddCatchBlock(handler);
		}

		/// <summary>
		/// Adds a <see cref="CatchBlockFilteredHandler"/> handler consisting of <see cref="NonEmptyCatchBlockFilter"/>. Mimics a try/catch block that swallows an exception.
		/// </summary>
		/// <param name="nonEmptyCatchBlockFilter"><see cref="NonEmptyCatchBlockFilter"/></param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public TryCatchBuilder AddCatchBlock(NonEmptyCatchBlockFilter nonEmptyCatchBlockFilter)
		{
			var handler = new CatchBlockFilteredHandler(nonEmptyCatchBlockFilter);
			return AddCatchBlock(handler);
		}

		public ITryCatch Build() => new TryCatch(_catchBlockHandlers, _hasCatchBlockForAll);
	}
}
