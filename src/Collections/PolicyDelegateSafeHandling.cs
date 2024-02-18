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
				var result = PolicyResult.ForSync();
				result.SetFailedWithError(ex, PolicyResultFailedReason.UnhandledError);
				return result;
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
				var result = PolicyResult<T>.ForSync();
				result.SetFailedWithError(ex, PolicyResultFailedReason.UnhandledError);
				return result;
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
				var result = PolicyResult.InitByConfigureAwait(configureAwait);
				result.SetFailedWithError(ex, PolicyResultFailedReason.UnhandledError);
				return result;
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
				var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);
				result.SetFailedWithError(ex, PolicyResultFailedReason.UnhandledError);
				return result;
			}
		}

		internal static (PolicyResult policyResult, bool IsCanceled) HandleAsyncAsSyncSafely(this PolicyDelegate si, CancellationToken token)
		{
			PolicyResult polResult = null;
			try
			{
				polResult = Task.Run(() => si.HandleAsync(false, token), token).Result;
				return (polResult, false);
			}
			catch (AggregateException aeWithCanceledException) when (aeWithCanceledException.HasCanceledException(token))
			{
				return (null, true);
			}
			catch (AggregateException ae)
			{
				var result = PolicyResult.ForSync();
				result.SetFailedWithError(ae.InnerException, PolicyResultFailedReason.UnhandledError);
				return (result, false);
			}
		}

		internal static (PolicyResult<T> policyResult, bool IsCanceled) HandleAsyncAsSyncSafely<T>(this PolicyDelegate<T> si, CancellationToken token)
		{
			PolicyResult<T> polResult = null;
			try
			{
				polResult = Task.Run(() => si.HandleAsync(false, token), token).Result;
				return (polResult, false);
			}
			catch (AggregateException aeWithCanceledException) when (aeWithCanceledException.HasCanceledException(token))
			{
				return (null, true);
			}
			catch (AggregateException ae)
			{
				var result = PolicyResult<T>.ForSync();
				result.SetFailedWithError(ae.InnerException, PolicyResultFailedReason.UnhandledError);
				return (result, false);
			}
		}
	}
}
