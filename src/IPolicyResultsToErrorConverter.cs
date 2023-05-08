using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public interface IPolicyResultsToErrorConverter
	{
		Func<IEnumerable<PolicyHandledResult>, Exception> ToExceptionConverter();
	}

	public interface IPolicyResultsToErrorConverter<T>
	{
		Func<IEnumerable<PolicyHandledResult<T>>, Exception> ToExceptionConverter();
	}

	internal class PolicyDelegateCollectionHandleExceptionConverter<T> : IPolicyResultsToErrorConverter<T>
	{
		public Func<IEnumerable<PolicyHandledResult<T>>, Exception> ToExceptionConverter() => (hResults) => new PolicyDelegateCollectionHandleException<T>(hResults.Select(hr => PolicyHandledErrors<T>.FromHandledResult(hr)));
	}

	internal class PolicyDelegateCollectionHandleExceptionConverter : IPolicyResultsToErrorConverter
	{
		public Func<IEnumerable<PolicyHandledResult>, Exception> ToExceptionConverter() => (hResults) => new PolicyDelegateCollectionHandleException(hResults.Select(hr => PolicyHandledErrors.FromHandledResult(hr)));
	}
}
