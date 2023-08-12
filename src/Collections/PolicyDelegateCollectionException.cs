using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class PolicyDelegateCollectionException : Exception
	{
		private readonly IEnumerable<PolicyDelegateResultErrors> _policyHandledErrors;
		private readonly IPolicyDelegateResultErrorsToExceptionsConverter _policyHandledErrorsConverter;
		private readonly IErrorsToStringAggregator _errorsToStringAggregator;

		private string _message;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResultErrors> policyHandledErrors, IPolicyDelegateResultErrorsToExceptionsConverter policyHandledErrorsConverter = null, IErrorsToStringAggregator errorsToStringAggregator = null)
		{
			_policyHandledErrors = policyHandledErrors;
			_policyHandledErrorsConverter = policyHandledErrorsConverter ?? new DefaultPolicyDelegateResultErrorsConverter();
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
	public class PolicyDelegateCollectionException<T> : PolicyDelegateCollectionException
	{
		private readonly IEnumerable<PolicyDelegateResultErrors<T>> _policyHandledErrorsT;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResultErrors<T>> policyHandledErrors, IPolicyDelegateResultErrorsToExceptionsConverter policyHandledErrorsConverter = null, IErrorsToStringAggregator errorsToStringAggregator = null)
							: base(policyHandledErrors.Select(phe => phe.ToPolicyDelegateResultErrors()), policyHandledErrorsConverter, errorsToStringAggregator)
		{
			_policyHandledErrorsT = policyHandledErrors;
		}

		public IEnumerable<T> GetResults() => _policyHandledErrorsT.Select(pher => pher.Result).ToList();
	}
}
