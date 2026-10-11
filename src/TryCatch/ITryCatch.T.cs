namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Defines methods for executing delegates with help of added <see cref="CatchBlockHandler"/> objects, returning a typed result.
	/// </summary>
	/// <typeparam name="T">The type of result returned by the execution delegate.</typeparam>
	public interface ITryCatch<T> : ITryCatch where T : ITryCatch<T>{}
}
