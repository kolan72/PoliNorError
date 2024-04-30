namespace PoliNorError.TryCatch
{
	public interface ITryCatch<T> : ITryCatch where T : ITryCatch<T>{}
}
