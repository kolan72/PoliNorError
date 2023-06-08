using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class DelegateTExtensions
	{
		public static PolicyResult<T> InvokeWithRetry<T>(this Func<T> func, int retryCount,  CancellationToken token = default) => InvokeWithRetry(func, retryCount, null, token);
		public static PolicyResult<T> InvokeWithRetry<T>(this Func<T> func, int retryCount, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => policyParams.ToRetryPolicy(retryCount).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetry(func, retryCount, delay, null, token);
		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, TimeSpan delay, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetry(func, retryCount, retryFunc, null, token);
		public static PolicyResult<T> InvokeWithWaitAndRetry<T>(this Func<T> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, CancellationToken token = default) => InvokeWithRetryAsync(func, retryCount, null, token);
		public static Task<PolicyResult<T>> InvokeWithRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => InvokeWithRetryAsync(func, retryCount, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, InvokeParams<RetryPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToRetryPolicy(retryCount).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, delay, null, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, TimeSpan delay, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, delay, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, TimeSpan delay, InvokeParams<RetryPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, null, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryAsync<T>(this Func<CancellationToken, Task<T>> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, InvokeParams<RetryPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc).HandleAsync(func, configureAwait, token);

		public static PolicyResult<T> InvokeWithRetryInfinite<T>(this Func<T> func, CancellationToken token = default) => InvokeWithRetryInfinite(func, null, token);
		public static PolicyResult<T> InvokeWithRetryInfinite<T>(this Func<T> func, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => policyParams.ToInfiniteRetryPolicy().Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetryInfinite(func, delay, null, token);
		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, TimeSpan delay, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay).Handle(func, token);

		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetryInfinite(func, retryFunc, null, token);
		public static PolicyResult<T> InvokeWithWaitAndRetryInfinite<T>(this Func<T> func, Func<int, Exception, TimeSpan> retryFunc, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, CancellationToken token = default) => InvokeWithRetryInfiniteAsync(func, null, token);
		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => InvokeWithRetryInfiniteAsync(func, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, InvokeParams<RetryPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToInfiniteRetryPolicy().HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, delay, null, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, TimeSpan delay, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, delay, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, TimeSpan delay, InvokeParams<RetryPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, null, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, Func<int, Exception, TimeSpan> retryFunc, InvokeParams<RetryPolicy> policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithWaitAndRetryInfiniteAsync<T>(this Func<CancellationToken, Task<T>> func, Func<int, Exception, TimeSpan> retryFunc, InvokeParams<RetryPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc).HandleAsync(func, configureAwait, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<CancellationToken, T> fallback, CancellationToken token = default) => InvokeWithFallback(func, fallback, null, token);
		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<CancellationToken, T> fallback, InvokeParams<FallbackPolicy> policyParams, CancellationToken token = default) => policyParams.ToFallbackPolicy(fallback).Handle(func, token);

		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<T> fallback, CancellationToken token = default) => InvokeWithFallback(func, fallback, null, token);
		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<T> fallback, InvokeParams<FallbackPolicy> policyParams, CancellationToken token = default) => InvokeWithFallback(func, fallback, policyParams, ConvertToCancelableFuncType.Precancelable, token);
		public static PolicyResult<T> InvokeWithFallback<T>(this Func<T> func, Func<T> fallback, InvokeParams<FallbackPolicy> policyParams, ConvertToCancelableFuncType convertType, CancellationToken token) => policyParams.ToFallbackPolicy(fallback, convertType).Handle(func, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> funcAsync, Func<Task<T>> fallbackAsync, CancellationToken token = default) => InvokeWithFallbackAsync(funcAsync, fallbackAsync, null, ConvertToCancelableFuncType.Precancelable, token);
		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> funcAsync, Func<Task<T>> fallbackAsync, InvokeParams<FallbackPolicy> policyParams, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable, CancellationToken token = default) => InvokeWithFallbackAsync(funcAsync, fallbackAsync, policyParams, false, convertType, token);
		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> funcAsync, Func<Task<T>> fallbackAsync, InvokeParams<FallbackPolicy> policyParams, bool configureAwait, ConvertToCancelableFuncType convertType, CancellationToken token) => policyParams.ToFallbackPolicy(fallbackAsync, convertType).HandleAsync(funcAsync, configureAwait, token);

		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallbackAsync, CancellationToken token = default) => InvokeWithFallbackAsync(func, fallbackAsync, null, token);
		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallbackAsync, InvokeParams<FallbackPolicy> policyParams, CancellationToken token = default) => InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, token);
		public static Task<PolicyResult<T>> InvokeWithFallbackAsync<T>(this Func<CancellationToken, Task<T>> func, Func<CancellationToken, Task<T>> fallbackAsync, InvokeParams<FallbackPolicy> policyParams, bool configureAwait, CancellationToken token) => policyParams.ToFallbackPolicy(fallbackAsync).HandleAsync(func, configureAwait, token);
	}
}
