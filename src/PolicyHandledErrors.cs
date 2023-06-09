using System;
using System.Collections.Generic;

namespace PoliNorError
{
	public class PolicyHandledErrors
	{
		internal protected PolicyHandledErrors(IEnumerable<Exception> errors, PolicyDelegateInfo policyInfo)
		{
			Errors = errors;
			PolicyInfo = policyInfo;
		}

		public PolicyDelegateInfo PolicyInfo { get; }

		public IEnumerable<Exception> Errors { get; }

		public static PolicyHandledErrors FromHandledResult(PolicyHandledResult handledResult)
		{
			return new PolicyHandledErrors(handledResult.Result.Errors, handledResult.PolicyInfo);
		}
	}

	public sealed class PolicyHandledErrors<T> : PolicyHandledErrors
	{
		internal PolicyHandledErrors(IEnumerable<Exception> errors, PolicyDelegateInfo policyInfo, T result) : base(errors, policyInfo)
		{
			Result = result;
		}

		public T Result { get; private set; }

		public static PolicyHandledErrors<T> FromHandledResult(PolicyHandledResult<T> handledResult)
		{
			return new PolicyHandledErrors<T>(handledResult.Result.Errors, handledResult.PolicyInfo, handledResult.Result.Result);
		}

		public  PolicyHandledErrors ToPolicyHandledErrors()
		{
			return new PolicyHandledErrors(Errors, PolicyInfo);
		}
	}
}
