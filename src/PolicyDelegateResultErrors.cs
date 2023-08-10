using System;
using System.Collections.Generic;
using System.Reflection;

namespace PoliNorError
{
	public class PolicyDelegateResultErrors
	{
		internal protected PolicyDelegateResultErrors(IEnumerable<Exception> errors, string policyName, MethodInfo policyMethodInfo)
		{
			Errors = errors;
			PolicyName = policyName;
			PolicyMethodInfo = policyMethodInfo;
		}

		public string PolicyName { get; }

		public MethodInfo PolicyMethodInfo { get; }

		public IEnumerable<Exception> Errors { get; }

		public static PolicyDelegateResultErrors FromDelegateResult(PolicyDelegateResult handledResult)
		{
			return new PolicyDelegateResultErrors(handledResult.Result.Errors, handledResult.PolicyName, handledResult.PolicyMethodInfo);
		}
	}

	public sealed class PolicyDelegateResultErrors<T> : PolicyDelegateResultErrors
	{
		internal PolicyDelegateResultErrors(IEnumerable<Exception> errors, string policyName, MethodInfo policyMethodInfo, T result) : base(errors, policyName, policyMethodInfo)
		{
			Result = result;
		}

		public T Result { get; private set; }

		public static PolicyDelegateResultErrors<T> FromDelegateResult(PolicyDelegateResult<T> handledResult)
		{
			return new PolicyDelegateResultErrors<T>(handledResult.Result.Errors, handledResult.PolicyName, handledResult.PolicyMethodInfo, handledResult.Result.Result);
		}

		public  PolicyDelegateResultErrors ToPolicyDelegateResultErrors()
		{
			return new PolicyDelegateResultErrors(Errors, PolicyName, PolicyMethodInfo);
		}
	}
}
