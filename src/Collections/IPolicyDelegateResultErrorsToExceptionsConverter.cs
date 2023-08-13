using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	internal interface IPolicyDelegateResultErrorsToExceptionsConverter
	{
		IEnumerable<Exception> Convert(IEnumerable<PolicyDelegateResultErrors> policyHandledErrors);
	}

	internal class DefaultPolicyDelegateResultErrorsConverter : IPolicyDelegateResultErrorsToExceptionsConverter
	{
		public IEnumerable<Exception> Convert(IEnumerable<PolicyDelegateResultErrors> policyHandledErrors)
		{
			return policyHandledErrors.SelectMany(pe => pe.Errors, (polErrors, exc) => new { polErrors, exc }) //Get it flattened 
									  .Select(polErrorAndExc => GetResultException(polErrorAndExc.polErrors, polErrorAndExc.exc));
		}

		private Exception GetResultException(PolicyDelegateResultErrors policyHandledErrors, Exception exc)
		{
			var res = $"Policy {policyHandledErrors.PolicyName} handled {policyHandledErrors.PolicyMethodInfo?.DeclaringType.Name}.{policyHandledErrors.PolicyMethodInfo?.Name} method with exception: '{exc.Message}'.";
			return new Exception(res, exc);
		}
	}
}
