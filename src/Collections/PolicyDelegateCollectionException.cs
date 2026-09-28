using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PoliNorError
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Constructed internally only from a collection of policy-delegate results; standard exception constructors are not applicable.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Exception is not intended to be serialized across an AppDomain or remoting boundary.")]
	public class PolicyDelegateCollectionException : Exception
	{
		private string _message;

		private readonly IReadOnlyList<PolicyDelegateResultBase> _policyDelegateResults;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResultBase> policyDelegateResults)
		{
			_policyDelegateResults = policyDelegateResults as IReadOnlyList<PolicyDelegateResultBase> ?? policyDelegateResults.ToArray();
			InnerExceptions = _policyDelegateResults.SelectMany(pdr => pdr.Errors).ToArray();
		}

		public override string Message
		{
			get
			{
				return _message ?? (_message = string.Join(";", _policyDelegateResults.Select(MapPolicyDelegateResultToExceptionMessage)));
			}
		}

		private static string MapPolicyDelegateResultToExceptionMessage(PolicyDelegateResultBase policyDelegateResult)
		{
			return string.Join(";", policyDelegateResult.Errors.Select(er => MapExceptionToSubMessage(er, policyDelegateResult.PolicyName, policyDelegateResult.PolicyMethodInfo)));
		}

		private static string MapExceptionToSubMessage(Exception exc, string policyName, MethodInfo methodInfo)
		{
			return $"Policy {policyName} handled {methodInfo?.DeclaringType?.Name}.{methodInfo?.Name} method with exception: '{exc.Message}'.";
		}

		public IEnumerable<Exception> InnerExceptions { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Constructed internally only from a collection of policy-delegate results; standard exception constructors are not applicable.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Exception is not intended to be serialized across an AppDomain or remoting boundary.")]
	public class PolicyDelegateCollectionException<T> : PolicyDelegateCollectionException
	{
		private readonly IReadOnlyList<T> _results;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResult<T>> policyDelegateResult)
			: this(policyDelegateResult as IReadOnlyList<PolicyDelegateResult<T>> ?? policyDelegateResult.ToArray())
		{
		}

		private PolicyDelegateCollectionException(IReadOnlyList<PolicyDelegateResult<T>> materialized)
			: base(materialized)
		{
			var results = new T[materialized.Count];
			for (int i = 0; i < materialized.Count; i++)
			{
				results[i] = materialized[i].Result.Result;
			}
			_results = results;
		}

		public IEnumerable<T> GetResults() => _results;
	}
}
