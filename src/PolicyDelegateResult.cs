namespace PoliNorError
{
	public class PolicyDelegateResult
	{
		public PolicyDelegateResult(PolicyDelegateInfo policyDelegateInfo, PolicyResult result)
		{
			PolicyInfo = policyDelegateInfo;
			Result = result;
		}

		public PolicyDelegateInfo PolicyInfo { get; }

		public PolicyResult Result { get; }
	}

	public sealed class PolicyDelegateResult<T>
	{
		public PolicyDelegateResult(PolicyDelegateInfo policyDelegateInfo, PolicyResult<T> result)
		{
			PolicyInfo = policyDelegateInfo;
			Result = result;
		}

		public PolicyDelegateInfo PolicyInfo { get; }

		public PolicyResult<T> Result { get; }

		internal PolicyDelegateResult ToPolicyDelegateResult()
		{
			return new PolicyDelegateResult(PolicyInfo, Result);
		}
	}
}
