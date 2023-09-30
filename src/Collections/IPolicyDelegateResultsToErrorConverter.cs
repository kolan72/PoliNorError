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

	internal class DefaultPolicyDelegateResultsToErrorConverter<T> : IPolicyDelegateResultsToErrorConverter<T>
	{
		private readonly Func<IEnumerable<PolicyDelegateResult<T>>, Exception> _func;
		public DefaultPolicyDelegateResultsToErrorConverter(Func<IEnumerable<PolicyDelegateResult<T>>, Exception> func) => _func = func;

		public Func<IEnumerable<PolicyDelegateResult<T>>, Exception> ToExceptionConverter() => _func;
	}

	internal class DefaultPolicyDelegateResultsToErrorConverter : IPolicyDelegateResultsToErrorConverter
	{
		private readonly Func<IEnumerable<PolicyDelegateResult>, Exception> _func;
		public DefaultPolicyDelegateResultsToErrorConverter(Func<IEnumerable<PolicyDelegateResult>, Exception> func) => _func = func;

		public Func<IEnumerable<PolicyDelegateResult>, Exception> ToExceptionConverter() => _func;
	}
}
