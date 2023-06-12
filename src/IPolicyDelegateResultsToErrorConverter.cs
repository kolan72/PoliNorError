using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public interface IPolicyDelegateResultsToErrorConverter
	{
		Func<IEnumerable<PolicyDelegateResult>, Exception> ToExceptionConverter();
	}

	public interface IPolicyDelegateResultsToErrorConverter<T>
	{
		Func<IEnumerable<PolicyDelegateResult<T>>, Exception> ToExceptionConverter();
	}

	internal class PolicyDelegateResultsToErrorConverter<T> : IPolicyDelegateResultsToErrorConverter<T>
	{
		public Func<IEnumerable<PolicyDelegateResult<T>>, Exception> ToExceptionConverter() => (hResults) => new PolicyDelegateCollectionException<T>(hResults.Select(hr => PolicyDelegateResultErrors<T>.FromDelegateResult(hr)));
	}

	internal class PolicyDelegateResultsToErrorConverter : IPolicyDelegateResultsToErrorConverter
	{
		public Func<IEnumerable<PolicyDelegateResult>, Exception> ToExceptionConverter() => (hResults) => new PolicyDelegateCollectionException(hResults.Select(hr => PolicyDelegateResultErrors.FromDelegateResult(hr)));
	}
}
