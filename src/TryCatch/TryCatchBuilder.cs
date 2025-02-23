using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		/// Creates <see cref="ITryCatch"/> which mimics a try/catch block without a filter that will process an exception by the <paramref name="bulkErrorProcessor"/>.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch CreateAndBuild(IBulkErrorProcessor bulkErrorProcessor)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(bulkErrorProcessor).Build();
		}

		/// <summary>
		/// Creates <see cref="ITryCatch"/> which mimics a try/catch block without a filter that will process an exception by the <paramref name="errorProcessorAction"/>.
		/// </summary>
		/// <param name="errorProcessorAction">Delegate that will process an exception.</param>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch CreateAndBuild(Action<Exception> errorProcessorAction)
		{
			var bulkProcessor = new BulkErrorProcessor().WithErrorProcessorOf(errorProcessorAction);
			return CreateAndBuild(bulkProcessor);
		}

		/// <summary>
		/// Creates <see cref="ITryCatch"/> which mimics a try/catch block without a filter that will process an exception by the <paramref name="errorProcessorFunc"/>.
		/// </summary>
		/// <param name="errorProcessorFunc">Delegate that will process an exception.</param>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch CreateAndBuild(Func<Exception, Task> errorProcessorFunc)
		{
			var bulkProcessor = new BulkErrorProcessor().WithErrorProcessorOf(errorProcessorFunc);
			return CreateAndBuild(bulkProcessor);
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

		/// <summary>
		/// Adds a <see cref="CatchBlockFilteredHandler"/> handler consisting of <see cref="NonEmptyCatchBlockFilter"/> and <see cref="IBulkErrorProcessor"/> to builder.
		/// </summary>
		/// <param name="filterFactory">Factory to create the <see cref="NonEmptyCatchBlockFilter"/>.</param>
		/// <param name="configure">Action to configure the <see cref="IBulkErrorProcessor"/>.</param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public TryCatchBuilder AddCatchBlock(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory, Action<IBulkErrorProcessor> configure)
		{
			var cb =  new EmptyCatchBlockFilter();
			var filter = filterFactory(cb);
			var bp = new BulkErrorProcessor();
			configure(bp);
			return AddCatchBlock(filter, bp);
		}

		public ITryCatch Build() => new TryCatch(_catchBlockHandlers, _hasCatchBlockForAll);
	}
}
