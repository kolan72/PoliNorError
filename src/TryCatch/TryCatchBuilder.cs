using System.Collections.Generic;

namespace PoliNorError.TryCatch
{
	public sealed class TryCatchBuilder : ITryCatchBuilder
	{
		private readonly List<CatchBlockHandler> _catchBlockHandlers;

		private TryCatchBuilder()
		{
			_catchBlockHandlers = new List<CatchBlockHandler>();
		}

		public static TryCatchBuilder CreateFrom(CatchBlockFilteredHandler filteredHandler)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(filteredHandler);
		}

		public static ITryCatchBuilder CreateFrom(CatchBlockForAllHandler filteredHandler)
		{
			var builder = new TryCatchBuilder();
			return builder.AddCatchBlock(filteredHandler);
		}

		public TryCatchBuilder AddCatchBlock(CatchBlockFilteredHandler filteredHandler)
		{
			_catchBlockHandlers.Add(filteredHandler);
			return this;
		}

		public ITryCatchBuilder AddCatchBlock(CatchBlockForAllHandler filteredHandler)
		{
			_catchBlockHandlers.Add(filteredHandler);
			return this;
		}

		public ITryCatch Build() => new TryCatch(_catchBlockHandlers);
	}
}
