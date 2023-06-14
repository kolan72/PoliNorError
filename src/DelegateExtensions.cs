using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class DelegateExtensions
	{
		public static PolicyResult InvokeWithRetry(this Action action, int retryCount, CancellationToken token = default) => InvokeWithRetry(action,  retryCount, null, token);
		public static PolicyResult InvokeWithRetry(this Action action, int retryCount, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToRetryPolicy(retryCount).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetry(action, retryCount, delay, null, token);
		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, TimeSpan delay, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, delay).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetry(action, retryCount, retryFunc, null, token);
		public static PolicyResult InvokeWithWaitAndRetry(this Action action, int retryCount, Func<int, Exception, TimeSpan> retryFunc, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, CancellationToken token = default) => InvokeWithRetryAsync(func, retryCount, null, token);
		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, InvokeParams policyParams, CancellationToken token = default) => InvokeWithRetryAsync(func, retryCount, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithRetryAsync(this Func<CancellationToken, Task> func, int retryCount, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToRetryPolicy(retryCount).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, delay, null, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, InvokeParams policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, delay, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, TimeSpan delay, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount,delay).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, null, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, InvokeParams policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryAsync(func, retryCount, retryFunc, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryAsync(this Func<CancellationToken, Task> func, int retryCount, Func<int, Exception, TimeSpan> retryFunc, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToRetryPolicyWithDelayProcessorOf(retryCount, retryFunc).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithRetryInfinite(this Action action, CancellationToken token = default) => InvokeWithRetryInfinite(action, null, token);
		public static PolicyResult InvokeWithRetryInfinite(this Action action, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToInfiniteRetryPolicy().Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetryInfinite(action, delay, null, token);
		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, TimeSpan delay, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay).Handle(action, token);

		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetryInfinite(action, retryFunc, null, token);
		public static PolicyResult InvokeWithWaitAndRetryInfinite(this Action action, Func<int, Exception, TimeSpan> retryFunc, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc).Handle(action, token);

		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, CancellationToken token = default) => InvokeWithRetryInfiniteAsync(func, null, token);
		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, InvokeParams policyParams, CancellationToken token = default) => InvokeWithRetryInfiniteAsync(func, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithRetryInfiniteAsync(this Func<CancellationToken, Task> func, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToInfiniteRetryPolicy().HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, delay, null, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, InvokeParams policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, delay, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, TimeSpan delay, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(delay).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, null, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, InvokeParams policyParams, CancellationToken token = default) => InvokeWithWaitAndRetryInfiniteAsync(func, retryFunc, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithWaitAndRetryInfiniteAsync(this Func<CancellationToken, Task> func, Func<int, Exception, TimeSpan> retryFunc, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToInfiniteRetryPolicyWithDelayProcessorOf(retryFunc).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action<CancellationToken> fallback, CancellationToken token = default) => InvokeWithFallback(action, fallback, null, token);
		public static PolicyResult InvokeWithFallback(this Action action, Action<CancellationToken> fallback, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToFallbackPolicy(fallback).Handle(action, token);

		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, CancellationToken token = default) => InvokeWithFallback(action, fallback, null, token);
		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, InvokeParams policyParams, CancellationToken token = default) => InvokeWithFallback(action, fallback, policyParams, ConvertToCancelableFuncType.Precancelable, token);
		public static PolicyResult InvokeWithFallback(this Action action, Action fallback, InvokeParams policyParams, ConvertToCancelableFuncType convertType, CancellationToken token) => policyParams.ToFallbackPolicy(fallback, convertType).Handle(action, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, CancellationToken token = default) => InvokeWithFallbackAsync(func, fallbackAsync, null, ConvertToCancelableFuncType.Precancelable, token);
		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, InvokeParams policyParams, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable, CancellationToken token = default) => InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, convertType, token);
		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<Task> fallbackAsync, InvokeParams policyParams, bool configureAwait, ConvertToCancelableFuncType convertType, CancellationToken token) => policyParams.ToFallbackPolicy(fallbackAsync, convertType).HandleAsync(func, configureAwait, token);

		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, CancellationToken token = default) => InvokeWithFallbackAsync(func, fallbackAsync, null, token);
		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams, CancellationToken token = default) => InvokeWithFallbackAsync(func, fallbackAsync, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithFallbackAsync(this Func<CancellationToken, Task> func, Func<CancellationToken, Task> fallbackAsync, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToFallbackPolicy(fallbackAsync).HandleAsync(func, configureAwait, token);

		public static PolicyResult InvokeWithSimple(this Action action, CancellationToken token = default) => InvokeWithSimple(action, null, token);
		public static PolicyResult InvokeWithSimple(this Action action, InvokeParams policyParams, CancellationToken token = default) => policyParams.ToSimplePolicy().Handle(action, token);

		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, CancellationToken token = default) => InvokeWithSimpleAsync(func, null, token);
		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, InvokeParams policyParams, CancellationToken token = default) => InvokeWithSimpleAsync(func, policyParams, false, token);
		public static Task<PolicyResult> InvokeWithSimpleAsync(this Func<CancellationToken, Task> func, InvokeParams policyParams, bool configureAwait, CancellationToken token) => policyParams.ToSimplePolicy().HandleAsync(func, configureAwait, token);
	}
}
