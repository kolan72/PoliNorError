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

		public static IPolicyDelegateCollection<T> WithThrowOnLastFailed<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Func<IEnumerable<PolicyDelegateResult<T>>, Exception> func = null)
		{
			policyDelegateCollection.WithThrowOnLastFailed(new DefaultPolicyDelegateResultsToErrorConverter<T>(func));
			return policyDelegateCollection;
		}
	}
}
