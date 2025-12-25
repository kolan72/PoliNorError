namespace PoliNorError
{
	public sealed class PipelineResult<T>
	{
		internal static PipelineResult<T> Failure(PolicyResult failedPolicyResult)
		{
			return new PipelineResult<T>()
			{
				FailedPolicyResult = failedPolicyResult,
				IsCanceled = failedPolicyResult.IsCanceled
			};
		}

		internal static PipelineResult<T> Failure(PolicyResult failedPolicyResult, bool isCanceled)
		{
			return new PipelineResult<T>()
			{
				FailedPolicyResult = failedPolicyResult,
				IsCanceled = isCanceled
			};
		}

		internal static PipelineResult<T> Success(PolicyResult<T> successPolicyResult)
		{
			return new PipelineResult<T>()
			{
				SucceededPolicyResult = successPolicyResult,
			};
		}

		private PipelineResult() { }

		internal PolicyResult FailedPolicyResult { get; private set; }
		internal PolicyResult<T> SucceededPolicyResult { get; private set; }

		public bool IsFailed => SucceededPolicyResult is null;

		public bool IsCanceled { get; private set; }

		public T Result => IsFailed ? default : SucceededPolicyResult.Result;
	}
}
