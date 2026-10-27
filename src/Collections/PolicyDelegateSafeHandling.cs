using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class PolicyDelegateSafeHandling
	{
		internal static PolicyResult HandleSafely(this PolicyDelegate policyDelegate, CancellationToken token)
		{
			try
			{
				return policyDelegate.Handle(token);
			}
			catch (Exception ex)
			{
				return PolicyResult.ForSync().WithUnhandledError(ex);
			}
		}

		internal static PolicyResult<T> HandleSafely<T>(this PolicyDelegate<T> policyDelegate, CancellationToken token)
		{
			try
			{
				return policyDelegate.Handle(token);
			}
			catch (Exception ex)
			{
				return PolicyResult<T>.ForSync().WithUnhandledError(ex);
			}
		}

		internal static async Task<PolicyResult> HandleSafelyAsync(this PolicyDelegate policyDelegate, bool configureAwait, CancellationToken token)
		{
			try
			{
				return await policyDelegate.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
			}
			catch (Exception ex)
			{
				return PolicyResult.InitByConfigureAwait(configureAwait).WithUnhandledError(ex);
			}
		}

		internal static async Task<PolicyResult<T>> HandleSafelyAsync<T>(this PolicyDelegate<T> policyDelegate, bool configureAwait, CancellationToken token)
		{
			try
			{
				return await policyDelegate.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
			}
			catch (Exception ex)
			{
				return PolicyResult<T>.InitByConfigureAwait(configureAwait).WithUnhandledError(ex);
			}
		}

		// Known trade-off (kept for the 2.x line): this is the synchronous facade over async delegates.
		// Task.Run isolates the call from the caller's synchronization context (no classic deadlock),
		// but it blocks a thread-pool thread. .Result is intentional: the catch filters below depend
		// on AggregateException wrapping — GetAwaiter().GetResult() would unwrap the exception and
		// change the observed behavior.
		internal static (PolicyResult policyResult, bool IsCanceled) HandleAsyncAsSyncSafely(this PolicyDelegate si, CancellationToken token)
		{
			try
			{
				return (Task.Run(() => si.HandleAsync(false, token), token).Result, false);
			}
			catch (AggregateException aeWithCanceledException) when (aeWithCanceledException.IsOperationCanceledWithRequestedToken(token))
			{
				return (null, true);
			}
			catch (AggregateException ae)
			{
				return (PolicyResult.ForSync().WithUnhandledError(ae.InnerException), false);
			}
		}

		internal static (PolicyResult<T> policyResult, bool IsCanceled) HandleAsyncAsSyncSafely<T>(this PolicyDelegate<T> si, CancellationToken token)
		{
			try
			{
				return (Task.Run(() => si.HandleAsync(false, token), token).Result, false);
			}
			catch (AggregateException aeWithCanceledException) when (aeWithCanceledException.IsOperationCanceledWithRequestedToken(token))
			{
				return (null, true);
			}
			catch (AggregateException ae)
			{
				return (PolicyResult<T>.ForSync().WithUnhandledError(ae.InnerException), false);
			}
		}

		// Single place for the "unhandled error" bookkeeping that was previously copy-pasted four times.
		private static TResult WithUnhandledError<TResult>(this TResult result, Exception ex) where TResult : PolicyResult
		{
			result.SetFailedWithError(ex, PolicyResultFailedReason.UnhandledError);
			result.ChangeByErrorData(ex);
			return result;
		}
	}
}
