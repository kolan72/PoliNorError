using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Orchestrates sequential handling of PolicyDelegate collections.
	/// All execution variants share two loop cores (sync/async) parameterized by
	/// per-item execution and result-accumulation delegates.
	/// </summary>
	internal static class PolicyDelegatesHandler
	{
		// ----- Sync entry points (signatures unchanged: consumed by Wrap and collection handlers) -----

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllSync(
			IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
			=> policyDelegateInfos.GetHandleType() == PolicyDelegateHandleType.Sync
				? HandleWhenAllReallySync(policyDelegateInfos, token)
				: HandleAllForceSync(policyDelegateInfos, token);

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllSync<T>(
			IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
			=> policyDelegateInfos.GetHandleType() == PolicyDelegateHandleType.Sync
				? HandleWhenAllReallySync(policyDelegateInfos, token)
				: HandleAllForceSync(policyDelegateInfos, token);

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState) HandleWhenAllReallySync(
			IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
			=> HandleAllCore(policyDelegateInfos, NonGenericAdapters.HandleSync, NonGenericAdapters.AddResult, token);

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) HandleWhenAllReallySync<T>(
			IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
			=> HandleAllCore(policyDelegateInfos, GenericAdapters<T>.HandleSync, GenericAdapters<T>.AddResult, token);

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllForceSync(
			IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
			=> HandleAllCore(policyDelegateInfos, NonGenericAdapters.HandleForceSync, NonGenericAdapters.AddResult, token);

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllForceSync<T>(
			IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
			=> HandleAllCore(policyDelegateInfos, GenericAdapters<T>.HandleForceSync, GenericAdapters<T>.AddResult, token);

		// ----- Async entry points -----

		internal static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllBySyncType(
			IEnumerable<PolicyDelegate> policyDelegateInfos, PolicyDelegateHandleType handleType,
			CancellationToken token = default, bool configureAwait = false)
		{
			// No upfront Any() short-circuit: the cores return Default() for empty input
			// without enumerating a second time.
			switch (handleType)
			{
				case PolicyDelegateHandleType.Sync:
					try
					{
						// Deliberate design (kept): offload purely-sync delegates so a captured
						// synchronization context is not blocked by CPU-bound work.
						return await Task.Run(() => HandleWhenAllReallySync(policyDelegateInfos, token), token)
							.ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
					{
						return (new FlexSyncEnumerable<PolicyDelegateResult>(), LastPolicyResultState.FromCanceled());
					}
				case PolicyDelegateHandleType.Misc:
					return await HandleAllMisc(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				case PolicyDelegateHandleType.Async:
					return await HandleWhenAllAsync(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				default:
					throw new NotImplementedException();
			}
		}

		internal static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllBySyncType<T>(
			IEnumerable<PolicyDelegate<T>> policyDelegateInfos, PolicyDelegateHandleType handleType,
			CancellationToken token = default, bool configureAwait = false)
		{
			switch (handleType)
			{
				case PolicyDelegateHandleType.Sync:
					try
					{
						return await Task.Run(() => HandleWhenAllReallySync(policyDelegateInfos, token), token)
							.ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
					{
						return (new FlexSyncEnumerable<PolicyDelegateResult<T>>(), LastPolicyResultState.FromCanceled());
					}
				case PolicyDelegateHandleType.Misc:
					return await HandleAllMisc(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				case PolicyDelegateHandleType.Async:
					return await HandleWhenAllAsync(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				default:
					throw new NotImplementedException();
			}
		}

		private static Task<(IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllMisc(
			IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token, bool configureAwait)
			=> HandleAllAsyncCore(policyDelegateInfos, NonGenericAdapters.HandleMisc, NonGenericAdapters.AddResult, configureAwait, token);

		private static Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllMisc<T>(
			IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token, bool configureAwait)
			=> HandleAllAsyncCore(policyDelegateInfos, GenericAdapters<T>.HandleMisc, GenericAdapters<T>.AddResult, configureAwait, token);

		private static Task<(IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleWhenAllAsync(
			IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token, bool configureAwait)
			=> HandleAllAsyncCore(policyDelegateInfos, NonGenericAdapters.HandleAsync, NonGenericAdapters.AddResult, configureAwait, token);

		private static Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleWhenAllAsync<T>(
			IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token, bool configureAwait)
			=> HandleAllAsyncCore(policyDelegateInfos, GenericAdapters<T>.HandleAsync, GenericAdapters<T>.AddResult, configureAwait, token);

		// ----- Shared loop cores -----

		// Single-pass enumeration: replaces the previous Any() + foreach double pass, which
		// re-enumerated lazy sources (e.g. PolicyWrapperCollection's Select(ToPolicyDelegate)
		// projections) and allocated throwaway PolicyDelegate instances on every extra pass.
		private static (IEnumerable<TCollected> HandleResults, LastPolicyResultState lastPolicyResultState)
			HandleAllCore<TDelegate, TCollected, TResult>(
				IEnumerable<TDelegate> policyDelegateInfos,
				Func<TDelegate, CancellationToken, TResult> handleItem,
				Action<FlexSyncEnumerable<TCollected>, TDelegate, TResult> addResult,
				CancellationToken token)
			where TResult : PolicyResult
		{
			var handledResults = new FlexSyncEnumerable<TCollected>();

			using (var e = policyDelegateInfos.GetEnumerator())
			{
				if (!e.MoveNext())
					return (handledResults, LastPolicyResultState.Default());

				TResult curPolResult = null;
				do
				{
					// Stop once the previous result succeeded or was canceled by the policy itself.
					// First iteration sees null here; NotFailedOrCanceled(null) is false, matching
					// the original foreach-based loops.
					if (curPolResult.NotFailedOrCanceled())
						break;

					if (token.IsCancellationRequested)
						return (handledResults, LastPolicyResultState.FromCanceled());

					curPolResult = handleItem(e.Current, token);

					// Null is the cancellation marker returned by HandleAsyncAsSyncSafely
					// (the original code checked its IsCanceled tuple flag with the same effect:
					// return early without appending the result).
					if (curPolResult == null)
						return (handledResults, LastPolicyResultState.FromCanceled());

					addResult(handledResults, e.Current, curPolResult);
				}
				while (e.MoveNext());

				return (handledResults, curPolResult.ToLastPolicyResultState());
			}
		}

		private static async Task<(IEnumerable<TCollected> HandleResults, LastPolicyResultState lastPolicyResultState)>
			HandleAllAsyncCore<TDelegate, TCollected, TResult>(
				IEnumerable<TDelegate> policyDelegateInfos,
				Func<TDelegate, bool, CancellationToken, Task<TResult>> handleItemAsync,
				Action<FlexSyncEnumerable<TCollected>, TDelegate, TResult> addResult,
				bool configureAwait,
				CancellationToken token)
			where TResult : PolicyResult
		{
			// BUG FIX: forAsync was previously derived from configureAwait ("!configureAwait"
			// in most methods, plain "configureAwait" in HandleAllMisc<T>), so configAwait: true
			// silently selected the unsynchronized wrapper on the async path. The flag describes
			// the execution mode, not context capture, and is now explicit.
			var handledResults = new FlexSyncEnumerable<TCollected>(forAsync: true);

			TResult curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
					break;

				if (token.IsCancellationRequested)
					return (handledResults, LastPolicyResultState.FromCanceled());

				curPolResult = await handleItemAsync(si, configureAwait, token).ConfigureAwait(configureAwait);
				addResult(handledResults, si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		// ----- Cached adapters (static per closed generic type: zero per-call delegate allocation) -----

		private static class NonGenericAdapters
		{
			internal static readonly Func<PolicyDelegate, CancellationToken, PolicyResult> HandleSync =
				(si, t) => si.HandleSafely(t);

			internal static readonly Func<PolicyDelegate, CancellationToken, PolicyResult> HandleForceSync =
				HandleForceSyncCore;

			// Per-item dispatch as in the original HandleAllForceSync: sync delegates run
			// directly (HandleSafely), async delegates are offloaded via HandleAsyncAsSyncSafely
			// (whose null result is the cancellation marker consumed by the core).
			internal static PolicyResult HandleForceSyncCore(PolicyDelegate si, CancellationToken t)
			{
				if (si.SyncType == SyncPolicyDelegateType.Sync)
					return si.HandleSafely(t);
				return si.HandleAsyncAsSyncSafely(t).policyResult;
			}

			internal static readonly Func<PolicyDelegate, bool, CancellationToken, Task<PolicyResult>> HandleAsync =
				(si, configureAwait, t) => si.HandleSafelyAsync(configureAwait, t);

			internal static readonly Func<PolicyDelegate, bool, CancellationToken, Task<PolicyResult>> HandleMisc =
				HandleMiscCore;

			internal static async Task<PolicyResult> HandleMiscCore(PolicyDelegate si, bool configureAwait, CancellationToken t)
				=> si.SyncType == SyncPolicyDelegateType.Sync
					? si.HandleSafely(t)
					: await si.HandleSafelyAsync(configureAwait, t).ConfigureAwait(configureAwait);

			internal static readonly Action<FlexSyncEnumerable<PolicyDelegateResult>, PolicyDelegate, PolicyResult> AddResult =
				EnumerablePolicyDelegateResultExtensions.AddPolicyDelegateResult;
		}

		private static class GenericAdapters<T>
		{
			internal static readonly Func<PolicyDelegate<T>, CancellationToken, PolicyResult<T>> HandleSync =
				(si, t) => si.HandleSafely(t);

			internal static readonly Func<PolicyDelegate<T>, CancellationToken, PolicyResult<T>> HandleForceSync =
				HandleForceSyncCore;

			// Per-item dispatch as in the original HandleAllForceSync<T>: sync delegates run
			// directly (HandleSafely), async delegates are offloaded via HandleAsyncAsSyncSafely
			// (whose null result is the cancellation marker consumed by the core).
			internal static PolicyResult<T> HandleForceSyncCore(PolicyDelegate<T> si, CancellationToken t)
			{
				if (si.SyncType == SyncPolicyDelegateType.Sync)
					return si.HandleSafely(t);
				return si.HandleAsyncAsSyncSafely(t).policyResult;
			}

			internal static readonly Func<PolicyDelegate<T>, bool, CancellationToken, Task<PolicyResult<T>>> HandleAsync =
				(si, configureAwait, t) => si.HandleSafelyAsync(configureAwait, t);

			internal static readonly Func<PolicyDelegate<T>, bool, CancellationToken, Task<PolicyResult<T>>> HandleMisc =
				HandleMiscCore;

			internal static async Task<PolicyResult<T>> HandleMiscCore(PolicyDelegate<T> si, bool configureAwait, CancellationToken t)
				=> si.SyncType == SyncPolicyDelegateType.Sync
					? si.HandleSafely(t)
					: await si.HandleSafelyAsync(configureAwait, t).ConfigureAwait(configureAwait);

			internal static readonly Action<FlexSyncEnumerable<PolicyDelegateResult<T>>, PolicyDelegate<T>, PolicyResult<T>> AddResult =
				EnumerablePolicyDelegateResultExtensions.AddPolicyDelegateResult;
		}
	}
}
