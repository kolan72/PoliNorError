namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Defines method to build  <see cref="ITryCatch"/>.
	/// </summary>
	public interface ITryCatchBuilder
	{
		/// <summary>
		/// Builds <see cref="ITryCatch"/>.
		/// </summary>
		/// <returns></returns>
		ITryCatch Build();
	}
}
