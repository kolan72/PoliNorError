using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IPolicyDelegateCollection : IWithPolicyBase<INeedDelegateCollection>, IEnumerable<PolicyDelegate>
	{
		IPolicyDelegateCollection WithPolicyDelegate(PolicyDelegate errorPolicy);

		IPolicyDelegateCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act);

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act);

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act, CancellationType convertType);

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func);

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func);

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func, CancellationType convertType);

		IPolicyDelegateCollection WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter errorConverter = null);

		/// <summary>
		/// Builds a handler for the entire collection of PolicyDelegates
		/// </summary>
		/// <returns>An instance of <see cref="IPolicyDelegateCollectionHandler"/></returns>
		IPolicyDelegateCollectionHandler BuildCollectionHandler();
	}

	public interface IPolicyDelegateCollection<T> : IWithPolicyBase<INeedDelegateCollection<T>>, IEnumerable<PolicyDelegate<T>>
	{
		IPolicyDelegateCollection<T> WithPolicyDelegate(PolicyDelegate<T> errorPolicy);

		IPolicyDelegateCollection<T> IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>> act);

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>, CancellationToken> act);

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>> act, CancellationType convertType);

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, Task> func);

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, CancellationToken, Task> func);

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, Task> func, CancellationType convertType);

		IPolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null);

		/// <summary>
		/// Builds a handler for the entire collection of generic PolicyDelegates
		/// </summary>
		/// <returns>An instance of <see cref="IPolicyDelegateCollectionHandler{T}"/>.</returns>
		IPolicyDelegateCollectionHandler<T> BuildCollectionHandler();
	}
}
