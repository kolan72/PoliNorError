namespace PoliNorError.TryCatch
{
	public static class CatchBlockHandlerExtensions
	{
		/// <summary>
		/// Creates <see cref="ITryCatch"/> with one <see cref="CatchBlockForAllHandler"/> handler.
		/// </summary>
		/// <param name="catchBlockForAllHandler"><see cref="CatchBlockForAllHandler"/></param>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch ToTryCatch(this CatchBlockForAllHandler catchBlockForAllHandler)
		{
			return TryCatchBuilder.CreateFrom(catchBlockForAllHandler).Build();
		}

		/// <summary>
		/// Creates <see cref="ITryCatch"/> with one <see cref="CatchBlockFilteredHandler"/> handler.
		/// </summary>
		/// <param name="catchBlockFilteredHandler"><see cref="CatchBlockFilteredHandler"/></param>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch ToTryCatch(this CatchBlockFilteredHandler catchBlockFilteredHandler)
		{
			return catchBlockFilteredHandler.ToTryCatchBuilder().Build();
		}

		/// <summary>
		/// Creates <see cref="TryCatchBuilder"/> with one <see cref="CatchBlockFilteredHandler"/> handler.
		/// </summary>
		/// <param name="catchBlockFilteredHandler"><see cref="CatchBlockFilteredHandler"/></param>
		/// <returns><see cref="TryCatchBuilder"/></returns>
		public static TryCatchBuilder ToTryCatchBuilder(this CatchBlockFilteredHandler catchBlockFilteredHandler)
		{
			return TryCatchBuilder.CreateFrom(catchBlockFilteredHandler);
		}
	}
}
