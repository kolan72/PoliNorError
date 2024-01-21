using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyDelegateCollectionRegistrar
	{
		public static IPolicyDelegateCollection<T> WithPolicyAndDelegate<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, IPolicyBase errorPolicy, Func<CancellationToken, Task<T>> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static IPolicyDelegateCollection<T> WithPolicyAndDelegate<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, IPolicyBase errorPolicy, Func<T> func) => policyDelegateCollection.WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public static INeedDelegateCollection<T> WithRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(retryCount, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection<T> WithWaitAndRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection<T> WithWaitAndRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(retryCount, delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection<T> WithInfiniteRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection<T> WithWaitAndInfiniteRetry<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return policyDelegateCollection.WithRetryInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, T> fallbackFunc, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<T> fallbackFunc, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams, convertType);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<CancellationToken, Task<T>> fallbackFunc, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams);
		}

		public static INeedDelegateCollection<T> WithFallback<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<Task<T>> fallbackFunc, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return policyDelegateCollection.WithFallbackInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>, T>(fallbackFunc, policyParams, convertType);
		}

		public static INeedDelegateCollection<T> WithSimple<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, ErrorProcessorParam policyParams = null)
		{
			return policyDelegateCollection.WithSimpleInner<IPolicyDelegateCollection<T>, INeedDelegateCollection<T>>(policyParams);
		}

		public static IPolicyDelegateCollection<T> IncludeErrorForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddIncludedErrorFilterForAll(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> ExcludeErrorForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddExcludedErrorFilterForAll(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> IncludeErrorForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddIncludedErrorFilterForLast(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> IncludeErrorForLast<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateCollection.AddIncludedErrorFilterForLast(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> ExcludeErrorForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddExcludedErrorFilterForLast(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> ExcludeErrorForLast<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, bool> func = null) where TException : Exception
		{
			policyDelegateCollection.AddExcludedErrorFilterForLast(func);
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies two types-based filter condition for including an exception in the processing performed by the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/>
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> IncludeErrorSet<T, TException1, TException2>(this IPolicyDelegateCollection<T> policyDelegateCollection) where TException1: Exception where TException2: Exception
		{
			policyDelegateCollection.AddIncludedErrorSetFilter<TException1, TException2>();
			return policyDelegateCollection;
		}

		/// <summary>
		/// Specifies two types-based filter condition for excluding an exception from the processing performed by the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/>
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> ExcludeErrorSet<T, TException1, TException2>(this IPolicyDelegateCollection<T> policyDelegateCollection) where TException1 : Exception where TException2 : Exception
		{
			policyDelegateCollection.AddExcludedErrorSetFilter<TException1, TException2>();
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>> act)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>> act, CancellationType convertType)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(act.ToCancelableAction(convertType));
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>, CancellationToken> act)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, Task> func)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return policyDelegateCollection.AddPolicyResultHandlerForAll(func.ToCancelableFunc(convertType));
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			policyDelegateCollection.Select(pd => pd.Policy).SetResultHandler(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>> act)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>> act, CancellationType convertType)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(act, convertType);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<PolicyResult<T>, CancellationToken> act)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(act);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, Task> func)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(func);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(func, convertType);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> AddPolicyResultHandlerForLast<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			policyDelegateCollection.GetPolicies().AddPolicyResultHandlerToLastPolicyInner(func);
			return policyDelegateCollection;
		}

		/// <summary>
		/// For the policy of the last element in the <see cref="PolicyDelegateCollection{T}"/>, this method adds a special PolicyResult handler that sets <see cref="PolicyResult.IsFailed"/> to true only if the executed <paramref name="predicate"/> returns true.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <param name="policyDelegateCollection"></param>
		/// <param name="predicate">A predicate that a PolicyResult should satisfy.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> SetPolicyResultFailedIf<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<PolicyResult<T>, bool> predicate)
		{
			policyDelegateCollection.GetPolicies().SetPolicyResultFailedIfInner(predicate);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> WithThrowOnLastFailed<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<IEnumerable<PolicyDelegateResult<T>>, Exception> func = null)
		{
			policyDelegateCollection.WithThrowOnLastFailed(new DefaultPolicyDelegateResultsToErrorConverter<T>(func));
			return policyDelegateCollection;
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<TException> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor to the policy of the last element of the <see cref="PolicyDelegateCollection{T}"/> to handle an inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="T">A return type of handling delegate</typeparam>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="policyDelegateCollection">PolicyDelegateCollection{T}</param>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns></returns>
		public static IPolicyDelegateCollection<T> WithInnerErrorProcessorOf<T, TException>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyDelegateCollection.WithInnerErrorProcessorOf<IPolicyDelegateCollection<T>, TException>(funcProcessor);
		}
	}
}
