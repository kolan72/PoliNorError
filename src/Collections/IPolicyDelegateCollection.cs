using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IPolicyDelegateCollection : IWithPolicyBase<INeedDelegateCollection>, IEnumerable<PolicyDelegate>, ICanAddErrorProcessor
	{
		IPolicyDelegateCollection WithPolicyDelegate(PolicyDelegate errorPolicy);

		IPolicyDelegateCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter errorConverter = null);

		/// <summary>
		/// Builds a handler for the entire collection of PolicyDelegates.
		/// The handler captures the delegates present at build time; later mutations of this collection do not affect it.
		/// The <see cref="PolicyDelegate"/> instances themselves are shared, so changing a delegate's bound func via the collection after build is still observed.
		/// </summary>
		/// <returns>An instance of <see cref="IPolicyDelegateCollectionHandler"/></returns>
		IPolicyDelegateCollectionHandler BuildCollectionHandler();
	}

	public interface IPolicyDelegateCollection<T> : IWithPolicyBase<INeedDelegateCollection<T>>, IEnumerable<PolicyDelegate<T>>, ICanAddErrorProcessor
	{
		IPolicyDelegateCollection<T> WithPolicyDelegate(PolicyDelegate<T> errorPolicy);

		IPolicyDelegateCollection<T> IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null);

		/// <summary>
		/// Builds a handler for the entire collection of generic PolicyDelegates.
		/// The handler captures the delegates present at build time; later mutations of this collection do not affect it.
		/// The <see cref="PolicyDelegate{T}"/> instances themselves are shared, so changing a delegate's bound func via the collection after build is still observed.
		/// </summary>
		/// <returns>An instance of <see cref="IPolicyDelegateCollectionHandler{T}"/>.</returns>
		IPolicyDelegateCollectionHandler<T> BuildCollectionHandler();
	}
}
