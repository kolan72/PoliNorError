using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PoliNorError
{
	/// <summary>
	/// Represents an exception that occurs during the execution of a policy delegate collection.
	/// Contains information about errors from multiple policy delegates.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Constructed internally only from a collection of policy-delegate results; standard exception constructors are not applicable.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Exception is not intended to be serialized across an AppDomain or remoting boundary.")]
	public class PolicyDelegateCollectionException : Exception
	{
		private readonly IReadOnlyList<Exception>[] _materializedErrors;
		private string _message;

		private readonly IReadOnlyList<PolicyDelegateResultBase> _policyDelegateResults;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResultBase> policyDelegateResults)
		{
			_policyDelegateResults = policyDelegateResults as IReadOnlyList<PolicyDelegateResultBase> ?? policyDelegateResults.ToArray();

			_materializedErrors = new IReadOnlyList<Exception>[_policyDelegateResults.Count];
			var allErrors = new List<Exception>();

			for (int i = 0; i < _policyDelegateResults.Count; i++)
			{
				var errors = _policyDelegateResults[i].Errors as IReadOnlyList<Exception> ?? _policyDelegateResults[i].Errors.ToArray();
				_materializedErrors[i] = errors;
				allErrors.AddRange(errors);
			}

			InnerExceptions = allErrors;
		}

		/// <summary>
		/// Gets the error message for this exception.
		/// The message contains details about all the exceptions that occurred during policy delegate execution.
		/// </summary>
		public override string Message
		{
			get
			{
				return _message ?? (_message = string.Join(";", MessageCore()));
			}
		}

		private IEnumerable<string> MessageCore()
		{
			for (int i = 0; i < _policyDelegateResults.Count; i++)
			{
				var pdr = _policyDelegateResults[i];
				yield return string.Join(";", _materializedErrors[i].Select(er => MapExceptionToSubMessage(er, pdr.PolicyName, pdr.PolicyMethodInfo)));
			}
		}

		private static string MapExceptionToSubMessage(Exception exc, string policyName, MethodInfo methodInfo)
		{
			return $"Policy {policyName} handled {methodInfo?.DeclaringType?.Name}.{methodInfo?.Name} method with exception: '{exc.Message}'.";
		}

		/// <summary>
		/// Gets the collection of all inner exceptions that occurred during policy delegate execution.
		/// </summary>
		public IEnumerable<Exception> InnerExceptions { get; }
	}

	/// <summary>
	/// Represents a strongly-typed exception that occurs during the execution of a policy delegate collection,
	/// containing both error information and the result values of all the policy delegates in the collection.
	/// </summary>
	/// <typeparam name="T">The type of the result values returned by the policy delegates.</typeparam>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Constructed internally only from a collection of policy-delegate results; standard exception constructors are not applicable.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Exception is not intended to be serialized across an AppDomain or remoting boundary.")]
	public class PolicyDelegateCollectionException<T> : PolicyDelegateCollectionException
	{
		private readonly Lazy<IReadOnlyList<T>> _results;

		internal PolicyDelegateCollectionException(IEnumerable<PolicyDelegateResult<T>> policyDelegateResult)
			: this(policyDelegateResult as IReadOnlyList<PolicyDelegateResult<T>> ?? policyDelegateResult.ToArray())
		{
		}

		private PolicyDelegateCollectionException(IReadOnlyList<PolicyDelegateResult<T>> materialized)
			: base(materialized)
		{
			_results = new Lazy<IReadOnlyList<T>>(() =>
			{
				var results = new T[materialized.Count];
				for (int i = 0; i < materialized.Count; i++)
				{
					results[i] = materialized[i].Result.Result;
				}
				return results;
			}, isThreadSafe: true);
		}

		/// <summary>
		/// Gets the result values of all the policy delegate results in the collection, in their handling order.
		/// </summary>
		/// <remarks>
		/// The values are taken from the <c>PolicyResult&lt;T&gt;.Result</c> property of each policy delegate result.
		/// For a policy delegate whose result has no value (for example, a failed policy that produced none), the default value of <typeparamref name="T"/> is returned in that position.
		/// The sequence is evaluated lazily on the first call and cached; subsequent calls return the same instance.
		/// </remarks>
		public IEnumerable<T> GetResults() => _results.Value;
	}
}