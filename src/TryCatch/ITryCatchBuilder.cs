namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Defines method to build  <see cref="ITryCatch"/>.
	/// </summary>
	public interface ITryCatchBuilder
	{
		/// <summary>
		/// Builds <see cref="ITryCatch"/> with previously added <see cref="CatchBlockHandler"/> handlers..
		/// </summary>
		/// <returns></returns>
		ITryCatch Build();
	}
}
