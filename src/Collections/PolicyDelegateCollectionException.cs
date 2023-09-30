using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class PolicyDelegateCollectionException : Exception
	{
		private string _message;

		private readonly IEnumerable<PolicyDelegateResultBase> _policyDelegateResults;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResultBase> policyDelegateResults)
		{
			_policyDelegateResults = policyDelegateResults;
			InnerExceptions = policyDelegateResults.SelectMany(pdr => pdr.Errors);
		}

		public override string Message
		{
			get
			{
				return _message ?? (_message = string.Join(";", _policyDelegateResults.Select(pdr => MapPolicyDelegateResultToExceptionMessage(pdr))));
			}
		}

		private static string MapPolicyDelegateResultToExceptionMessage(PolicyDelegateResultBase policyDelegateResult)
		{
			return string.Join(";", policyDelegateResult.Errors.Select(er => MapExceptionToSubMessage(er, policyDelegateResult.PolicyName, policyDelegateResult.PolicyMethodInfo)));
		}

		private static string MapExceptionToSubMessage(Exception exc, string policyName,  MethodInfo methodInfo)
		{
			return $"Policy {policyName} handled {methodInfo?.DeclaringType.Name}.{methodInfo?.Name} method with exception: '{exc.Message}'.";
		}

		public IEnumerable<Exception> InnerExceptions { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
	public class PolicyDelegateCollectionException<T> : PolicyDelegateCollectionException
	{
		private readonly IEnumerable<PolicyDelegateResult<T>> _policyDelegateResult;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResult<T>> policyDelegateResult) : base(policyDelegateResult)
		{
			_policyDelegateResult = policyDelegateResult;
		}

		public IEnumerable<T> GetResults() => _policyDelegateResult.Select(pher => pher.Result.Result).ToList();
	}
}
