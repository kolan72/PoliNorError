namespace PoliNorError
{
	/// <summary>
	/// Represents the result of a pipeline execution.
	/// </summary>
	/// <typeparam name="T">The type of the result value.</typeparam>
	public sealed class PipelineResult<T>
	{
		/// <summary>
		/// Creates a failed pipeline result.
		/// </summary>
		/// <param name="failedPolicyResult">The policy result containing failure information.</param>
		/// <returns>A failed pipeline result.</returns>
		internal static PipelineResult<T> Failure(PolicyResult failedPolicyResult)
		{
			return new PipelineResult<T>()
			{
				FailedPolicyResult = failedPolicyResult,
				IsCanceled = failedPolicyResult.IsCanceled
			};
		}

		/// <summary>
		/// Creates a successful pipeline result.
		/// </summary>
		/// <param name="successPolicyResult">The policy result containing success information.</param>
		/// <returns>A successful pipeline result.</returns>
		internal static PipelineResult<T> Success(PolicyResult<T> successPolicyResult)
		{
			return new PipelineResult<T>()
			{
				SucceededPolicyResult = successPolicyResult,
			};
		}

		private PipelineResult() { }

		/// <summary>
		/// Gets the failed policy result if the pipeline failed.
		/// </summary>
		internal PolicyResult FailedPolicyResult { get; private set; }

		/// <summary>
		/// Gets the succeeded policy result if the pipeline succeeded.
		/// </summary>
		internal PolicyResult<T> SucceededPolicyResult { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the pipeline execution failed.
		/// </summary>
		public bool IsFailed => SucceededPolicyResult is null;

		/// <summary>
		/// Gets a value indicating whether the pipeline execution was canceled.
		/// </summary>
		public bool IsCanceled { get; private set; }

		/// <summary>
		/// Gets the result value if the pipeline succeeded, or the default value if it failed.
		/// </summary>
		public T Result => IsFailed ? default : SucceededPolicyResult.Result;
	}
}
