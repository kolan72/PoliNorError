using System;
using System.Collections.Generic;

namespace PoliNorError
{
	internal static class EnumerablePolicyHandledErrorsExtensions
    {
        public static IEnumerable<Exception> ToExceptions(this IEnumerable<PolicyHandledErrors> policyHandledErrors, IPolicyHandledErrorsToExceptionsConverter policyHandledErrorsConverter)
        {
            return policyHandledErrorsConverter.Convert(policyHandledErrors);
        }
    }
}
