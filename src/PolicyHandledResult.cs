namespace PoliNorError
{
	public class PolicyHandledResult
	{
		public PolicyHandledResult(PolicyHandledInfo policyHandledInfo, PolicyResult result)
		{
			PolicyInfo = policyHandledInfo;
			Result = result;
		}

		public PolicyHandledInfo PolicyInfo { get; }

		public PolicyResult Result { get; }
	}

	public sealed class PolicyHandledResult<T>
	{
		public PolicyHandledResult(PolicyHandledInfo policyHandledInfo, PolicyResult<T> result)
		{
			PolicyInfo = policyHandledInfo;
			Result = result;
		}

		public PolicyHandledInfo PolicyInfo { get; }

		public PolicyResult<T> Result { get; }

		internal PolicyHandledResult ToPolicyHandledResult()
		{
			return new PolicyHandledResult(PolicyInfo, Result);
		}
	}
}
