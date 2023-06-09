namespace PoliNorError
{
	public class PolicyHandledResult
	{
		public PolicyHandledResult(PolicyDelegateInfo policyDelegateInfo, PolicyResult result)
		{
			PolicyInfo = policyDelegateInfo;
			Result = result;
		}

		public PolicyDelegateInfo PolicyInfo { get; }

		public PolicyResult Result { get; }
	}

	public sealed class PolicyHandledResult<T>
	{
		public PolicyHandledResult(PolicyDelegateInfo policyDelegateInfo, PolicyResult<T> result)
		{
			PolicyInfo = policyDelegateInfo;
			Result = result;
		}

		public PolicyDelegateInfo PolicyInfo { get; }

		public PolicyResult<T> Result { get; }

		internal PolicyHandledResult ToPolicyHandledResult()
		{
			return new PolicyHandledResult(PolicyInfo, Result);
		}
	}
}
