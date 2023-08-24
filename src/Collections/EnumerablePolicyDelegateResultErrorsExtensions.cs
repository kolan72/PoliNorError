using System;
using System.Collections.Generic;

namespace PoliNorError
{
	internal static class EnumerablePolicyDelegateResultErrorsExtensions
	{
		public static IEnumerable<Exception> ToExceptions(this IEnumerable<PolicyDelegateResultErrors> policyDelegateResultErrors, IPolicyDelegateResultErrorsToExceptionsConverter policyHandledErrorsConverter)
		{
			return policyHandledErrorsConverter.Convert(policyDelegateResultErrors);
		}
	}
}
