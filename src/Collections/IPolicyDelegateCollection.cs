using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IPolicyDelegateCollection : IWithPolicyBase<INeedDelegateCollection>, IEnumerable<PolicyDelegate>
	{
		IPolicyDelegateCollection WithPolicyDelegate(PolicyDelegate errorPolicy);

		IPolicyDelegateCollection IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter);

		IPolicyDelegateCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter);

		IPolicyDelegateCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act);

		IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func);

		IPolicyDelegateCollection WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter errorConverter = null);

		PolicyDelegateCollectionResult HandleAll(CancellationToken token = default);

		Task<PolicyDelegateCollectionResult> HandleAllAsync(bool configAwait, CancellationToken token);
	}

	public interface IPolicyDelegateCollection<T> : IWithPolicyBase<INeedDelegateCollection<T>>, IEnumerable<PolicyDelegate<T>>
	{
		IPolicyDelegateCollection<T> WithPolicyDelegate(PolicyDelegate<T> errorPolicy);

		IPolicyDelegateCollection<T> IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter);

		IPolicyDelegateCollection<T> IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection<T> ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter);

		IPolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception;

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>, CancellationToken> act);

		IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, CancellationToken, Task> func);

		IPolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null);

		PolicyDelegateCollectionResult<T> HandleAll(CancellationToken token = default);

		Task<PolicyDelegateCollectionResult<T>> HandleAllAsync(bool configAwait, CancellationToken token);
	}
}
