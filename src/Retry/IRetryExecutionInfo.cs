namespace PoliNorError
{
	/// <summary>
	/// Represents information related to retry execution.
	/// </summary>
	public interface IRetryExecutionInfo
	{
		/// <summary>
		/// Represents the current number of retries.
		/// </summary>
		int RetryCount { get; }
	}
}
