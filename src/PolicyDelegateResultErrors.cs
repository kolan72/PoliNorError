using System;
using System.Collections.Generic;

namespace PoliNorError
{
	public class PolicyDelegateResultErrors
	{
		internal protected PolicyDelegateResultErrors(IEnumerable<Exception> errors, PolicyDelegateInfo policyInfo)
		{
			Errors = errors;
			PolicyInfo = policyInfo;
		}

		public PolicyDelegateInfo PolicyInfo { get; }

		public IEnumerable<Exception> Errors { get; }

		public static PolicyDelegateResultErrors FromDelegateResult(PolicyDelegateResult handledResult)
		{
			return new PolicyDelegateResultErrors(handledResult.Result.Errors, handledResult.PolicyInfo);
		}
	}

	public sealed class PolicyDelegateResultErrors<T> : PolicyDelegateResultErrors
	{
		internal PolicyDelegateResultErrors(IEnumerable<Exception> errors, PolicyDelegateInfo policyInfo, T result) : base(errors, policyInfo)
		{
			Result = result;
		}

		public T Result { get; private set; }

		public static PolicyDelegateResultErrors<T> FromDelegateResult(PolicyDelegateResult<T> handledResult)
		{
			return new PolicyDelegateResultErrors<T>(handledResult.Result.Errors, handledResult.PolicyInfo, handledResult.Result.Result);
		}

		public  PolicyDelegateResultErrors ToPolicyDelegateResultErrors()
		{
			return new PolicyDelegateResultErrors(Errors, PolicyInfo);
		}
	}
}
