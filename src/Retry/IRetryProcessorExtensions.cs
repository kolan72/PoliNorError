using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IRetryProcessorExtensions
	{
		public static Task<PolicyResult<T>> RetryAsync<T>(this IRetryProcessor retryProcessor, Func<CancellationToken, Task<T>> func, int retryCount, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Limited(retryCount), token);

		public static Task<PolicyResult> RetryAsync(this IRetryProcessor retryProcessor, Func<CancellationToken, Task> func, int retryCount, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Limited(retryCount), token);

		public static Task<PolicyResult> RetryAsync(this IRetryProcessor retryProcessor, Func<CancellationToken, Task> func, RetryCountInfo retryCountInfo, CancellationToken token)
											=> retryProcessor.RetryAsync(func, retryCountInfo, false, token);

		public static Task<PolicyResult<T>> RetryAsync<T>(this IRetryProcessor retryProcessor, Func<CancellationToken, Task<T>> func, RetryCountInfo retryCountInfo, CancellationToken token)
													=> retryProcessor.RetryAsync(func, retryCountInfo, false, token);

		public static PolicyResult<T> Retry<T>(this IRetryProcessor retryProcessor, Func<T> func, int retryCount, CancellationToken token = default)
													=> retryProcessor.Retry(func, RetryCountInfo.Limited(retryCount), token);

		public static PolicyResult Retry(this IRetryProcessor retryProcessor, Action action, int retryCount, CancellationToken token = default)
													=> retryProcessor.Retry(action, RetryCountInfo.Limited(retryCount), token);

		public static Task<PolicyResult<T>> RetryInfiniteAsync<T>(this IRetryProcessor retryProcessor, Func<CancellationToken, Task<T>> func, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Infinite(), token);

		public static Task<PolicyResult> RetryInfiniteAsync(this IRetryProcessor retryProcessor, Func<CancellationToken, Task> func, CancellationToken token = default)
													=> retryProcessor.RetryAsync(func, RetryCountInfo.Infinite(), token);

		public static PolicyResult<T> RetryInfinite<T>(this IRetryProcessor retryProcessor, Func<T> func, CancellationToken token = default)
												=> retryProcessor.Retry(func, RetryCountInfo.Infinite(), token);

		public static PolicyResult RetryInfinite(this IRetryProcessor retryProcessor, Action action, CancellationToken token = default)
													=> retryProcessor.Retry(action, RetryCountInfo.Infinite(), token);

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, TimeSpan delay)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(delay));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, Func<int, TimeSpan> delayOnRetryFunc)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(delayOnRetryFunc));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, Func<TimeSpan, int, Exception, TimeSpan> delayOnRetryFunc, TimeSpan delayFuncArg)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(delayOnRetryFunc, delayFuncArg));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, Func<int, Exception, TimeSpan> retryFunc)
		{
			return retryProcessor.WithWait(new DelayErrorProcessor(retryFunc));
		}

		public static IRetryProcessor WithWait(this IRetryProcessor retryProcessor, DelayErrorProcessor delayErrorProcessor)
		{
			retryProcessor.WithErrorProcessor(delayErrorProcessor);
			return retryProcessor;
		}

		public static IRetryProcessor IncludeError<TException>(this IRetryProcessor retryProcessor, Func<TException, bool> func = null) where TException : Exception => retryProcessor.IncludeError<IRetryProcessor, TException>(func);

		public static IRetryProcessor IncludeError(this IRetryProcessor retryProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => retryProcessor.IncludeError<IRetryProcessor>(handledErrorFilter);

		public static IRetryProcessor ExcludeError<TException>(this IRetryProcessor retryProcessor, Func<TException, bool> func = null) where TException : Exception => retryProcessor.ExcludeError<IRetryProcessor, TException>(func);

		public static IRetryProcessor ExcludeError(this IRetryProcessor retryProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => retryProcessor.ExcludeError<IRetryProcessor>(handledErrorFilter);
	}
}
