using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class PolicyDelegateCollectionHandleException : Exception
	{
		private readonly IEnumerable<PolicyHandledErrors> _policyHandledErrors;
		private readonly IPolicyHandledErrorsToExceptionsConverter _policyHandledErrorsConverter;
		private readonly IErrorsToStringAggregator _errorsToStringAggregator;

		private string _message;

		public PolicyDelegateCollectionHandleException(IEnumerable<PolicyHandledErrors> policyHandledErrors, IPolicyHandledErrorsToExceptionsConverter policyHandledErrorsConverter = null, IErrorsToStringAggregator errorsToStringAggregator = null)
		{
			_policyHandledErrors = policyHandledErrors;
			_policyHandledErrorsConverter = policyHandledErrorsConverter ?? new DefaultPolicyHandledErrorsConverter();
			_errorsToStringAggregator = errorsToStringAggregator ?? new DefaultErrorsToStringAggregator();
		}

		public override string Message
		{
			get
			{
				return _message ?? (_message = GetCustomizedExceptionMessage());
			}
		}

		public IEnumerable<Exception> InnerExceptions => _policyHandledErrors.ToExceptions(_policyHandledErrorsConverter);

		private string GetCustomizedExceptionMessage() => _errorsToStringAggregator.Aggregate(InnerExceptions);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class PolicyDelegateCollectionHandleException<T> : PolicyDelegateCollectionHandleException
	{
		private readonly IEnumerable<PolicyHandledErrors<T>> _policyHandledErrorsT;

		public PolicyDelegateCollectionHandleException(IEnumerable<PolicyHandledErrors<T>> policyHandledErrors, IPolicyHandledErrorsToExceptionsConverter policyHandledErrorsConverter = null, IErrorsToStringAggregator errorsToStringAggregator = null)
							: base(policyHandledErrors.Select(phe => phe.ToPolicyHandledErrors()), policyHandledErrorsConverter, errorsToStringAggregator)
		{
			_policyHandledErrorsT = policyHandledErrors;
		}

		public IEnumerable<T> ErrorResults => _policyHandledErrorsT.Select(pher => pher.Result);
	}
}
