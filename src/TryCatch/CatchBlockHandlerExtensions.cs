namespace PoliNorError.TryCatch
{
	public static class CatchBlockHandlerExtensions
	{
		/// <summary>
		/// Creates <see cref="ITryCatch"/> with one <see cref="CatchBlockForAllHandler"/> handler.
		/// </summary>
		/// <param name="catchBlockForAllHandler"></param>
		/// <returns><see cref="ITryCatch"/></returns>
		public static ITryCatch ToTryCatch(this CatchBlockForAllHandler catchBlockForAllHandler)
		{
			return (TryCatchBuilder.CreateFrom(catchBlockForAllHandler)).Build();
		}
	}
}
