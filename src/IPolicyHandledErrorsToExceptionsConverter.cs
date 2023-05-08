using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public interface IPolicyHandledErrorsToExceptionsConverter
	{
		IEnumerable<Exception> Convert(IEnumerable<PolicyHandledErrors> policyHandledErrors);
	}

	public class DefaultPolicyHandledErrorsConverter : IPolicyHandledErrorsToExceptionsConverter
	{
		public IEnumerable<Exception> Convert(IEnumerable<PolicyHandledErrors> policyHandledErrors)
		{
			return policyHandledErrors.SelectMany(pe => pe.Errors, (polErrors, exc) => new { polErrors, exc }) //Get it flattened 
									  .Select(polErrorAndExc => GetResultException(polErrorAndExc.polErrors, polErrorAndExc.exc));
		}

		private Exception GetResultException(PolicyHandledErrors policyHandledErrors, Exception exc)
		{
			var res = $"Policy {policyHandledErrors.PolicyInfo.Policy.PolicyName} handled {policyHandledErrors.PolicyInfo.PolicyMethodInfo?.DeclaringType.Name}.{policyHandledErrors.PolicyInfo.PolicyMethodInfo?.Name} method with exception: '{exc.Message}'.";
			return new Exception(res);
		}
	}
}
