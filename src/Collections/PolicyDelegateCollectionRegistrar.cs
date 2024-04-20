using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of extension methods to add policy, policy and a delegate, error filter or PolicyResult handler to <see cref="IPolicyDelegateCollection"/> or <see cref="IPolicyDelegateCollection{T}"/>.
	/// </summary>
	public static partial class PolicyDelegateCollectionRegistrar
	{
		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Func<CancellationToken, Task> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static IPolicyDelegateCollection WithPolicyAndDelegate(this IPolicyDelegateCollection policyDelegateCollection, IPolicyBase errorPolicy, Action action) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(action));

		public static INeedDelegateCollection WithRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndRetry(this IPolicyDelegateCollection policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(retryCount, delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithWaitAndInfiniteRetry(this IPolicyDelegateCollection policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection, INeedDelegateCollection>(delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action<CancellationToken> fallback, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Action fallback, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallback, policyParams, convertType);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams);
		}

		public static INeedDelegateCollection WithFallback(this IPolicyDelegateCollection policyDelegateCollection, Func<Task> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection, INeedDelegateCollection>(fallbackAsync, policyParams, convertType);
		}

		public static INeedDelegateCollection WithSimple(this IPolicyDelegateCollection policyDelegateCollection, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<IPolicyDelegateCollection, INeedDelegateCollection>(policyParams);
		}

		public static IPolicyDelegateCollection IncludeErrorForAll(this IPolicyDelegateCollection policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddIncludedErrorFilterForAll(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection ExcludeErrorForAll(this IPolicyDelegateCollection policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddExcludedErrorFilterForAll(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection IncludeErrorForLast(this IPolicyDelegateCollection policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddIncludedErrorFilterForLast(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection IncludeErrorForLast<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateCollection.AddIncludedErrorFilterForLast(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection ExcludeErrorForLast(this IPolicyDelegateCollection policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddExcludedErrorFilterForLast(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection ExcludeErrorForLast<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateCollection.AddExcludedErrorFilterForLast(func);
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies two types-based filter condition for including an exception in the processing performed by the policy of the last element of the <see cref="PolicyDelegateCollection"/>
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection IncludeErrorSet<TException1, TException2>(this IPolicyDelegateCollection policyDelegateCollection) where TException1 : Exception where TException2 : Exception
		{
			policyDelegateCollection.AddIncludedErrorSetFilter<TException1, TException2>();
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies the <see cref= "IErrorSet" /> interface-based filter conditions for including an exception in the processing performed by the policy of the last element of the <see cref="PolicyDelegateCollection"/>
		/// </summary>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="errorSet"><see cref="IErrorSet"/></param>
		/// <returns></returns>
		public static IPolicyDelegateCollection IncludeErrorSet(this IPolicyDelegateCollection policyDelegateCollection, IErrorSet errorSet)
		{
			policyDelegateCollection.AddIncludedErrorSetFilter(errorSet);
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies two types-based filter condition for excluding an exception from the processing performed by the policy of the last element of the <see cref="PolicyDelegateCollection"/>
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection ExcludeErrorSet<TException1, TException2>(this IPolicyDelegateCollection policyDelegateCollection) where TException1 : Exception where TException2 : Exception
		{
			policyDelegateCollection.AddExcludedErrorSetFilter<TException1, TException2>();
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies the type- and optionally predicate-based filter condition for the inner exception of a handling exception to be included in the handling by the policy of the last element of the <see cref="PolicyDelegateCollection"/>.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection IncludeInnerError<TInnerException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
		{
			policyDelegateCollection.AddIncludedInnerErrorFilter(predicate);
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies the type- and optionally predicate-based filter condition for the inner exception of a handling exception to be excluded from the handling by the policy of the last element of the <see cref="PolicyDelegateCollection"/>.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection ExcludeInnerError<TInnerException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
		{
			policyDelegateCollection.AddExcludedInnerErrorFilter(predicate);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult> act)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult> act, CancellationType convertType)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(act.ToCancelableAction(convertType));
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult, CancellationToken> act)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, Task> func)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(func.ToCancelableFunc(convertType));
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForAll(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, CancellationToken, Task> func)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForLast(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult> act)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForLast(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult> act, CancellationType convertType)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(act, convertType);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForLast(this IPolicyDelegateCollection policyDelegateCollection, Action<PolicyResult, CancellationToken> act)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForLast(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, Task> func)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForLast(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, Task> func, CancellationType convertType)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(func, convertType);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection AddPolicyResultHandlerForLast(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, CancellationToken, Task> func)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(func);
			return policyDelegateCollection;
		}

		/// <summary>
		/// For the policy of the last element in the <see cref="PolicyDelegateCollection"/>, this method adds a special PolicyResult handler that sets <see cref="PolicyResult.IsFailed"/> to true only if the executed <paramref name="predicate"/> returns true.
		/// </summary>
		/// <param name="policyDelegateCollection"></param>
		/// <param name="predicate">A predicate that a PolicyResult should satisfy.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection SetPolicyResultFailedIf(this IPolicyDelegateCollection policyDelegateCollection, Func<PolicyResult, bool> predicate)
		{
			policyDelegateCollection.GetPolicies().SetPolicyResultFailedIfInner(predicate);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection WithThrowOnLastFailed(this IPolicyDelegateCollection policyDelegateCollection, Func<IEnumerable<PolicyDelegateResult>, Exception> func = null)
		{
			policyDelegateCollection.WithThrowOnLastFailed(new DefaultPolicyDelegateResultsToErrorConverter(func));
			return policyDelegateCollection;
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Action<TException> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection WithInnerErrorProcessorOf<TException>(this IPolicyDelegateCollection policyDelegateCollection, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection, TException>(funcProcessor);
		}
	}
}
