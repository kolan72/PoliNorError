using System.Collections.Generic;

namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Default implementation of <see cref="ITryCatchBuilder"/>.
	/// </summary>
	public sealed class TryCatchBuilder : ITryCatchBuilder
	{
		private readonly List<CatchBlockHandler> _catchBlockHandlers;

		private TryCatchBuilder()
		{
			_catchBlockHandlers = new List<CatchBlockHandler>();
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
			return this;
		}

		public ITryCatch Build() => new TryCatch(_catchBlockHandlers);
	}
}
