using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PoliNorError
{
	internal static class PolicyDelegatesHandler
    {
		internal static (IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllSync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = policyDelegateInfos.GetHandleType();
			if (handleType == PolicyDelegateHandleType.Sync)
			{
				return HandleWhenAllReallySync(policyDelegateInfos, token);
			}
			else
			{
				return HandleAllForceSync(policyDelegateInfos, token);
			}
		}

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllSync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = policyDelegateInfos.GetHandleType();
			if (handleType == PolicyDelegateHandleType.Sync)
			{
				return HandleWhenAllReallySync(policyDelegateInfos, token);
			}
			else
			{
				return HandleAllForceSync(policyDelegateInfos, token);
			}
		}

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllForceSync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			if (!policyDelegateInfos.Any())
				return (handledResults, LastPolicyResultState.Default());

			PolicyResult curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.HandleSafely(token);
				}
				else
				{
					var (policyResult, IsCanceled) = si.HandleAsyncAsSyncSafely(token);
					curPolResult = policyResult;
					if (IsCanceled)
					{
						return (handledResults, LastPolicyResultState.FromCanceled());
					}
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) HandleAllForceSync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			if (!policyDelegateInfos.Any())
				return (handledResults, LastPolicyResultState.Default());

			PolicyResult<T> curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.HandleSafely(token);
				}
				else
				{
					var (policyResult, IsCanceled) = si.HandleAsyncAsSyncSafely(token);
					curPolResult = policyResult;
					if (IsCanceled)
					{
						return (handledResults, LastPolicyResultState.FromCanceled());
					}
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		internal static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllBySyncType(IEnumerable<PolicyDelegate> policyDelegateInfos, PolicyDelegateHandleType handleType, CancellationToken token = default, bool configureAwait = false)
		{
			if (!policyDelegateInfos.Any())
				return (new FlexSyncEnumerable<PolicyDelegateResult>(), LastPolicyResultState.Default());
			switch (handleType)
			{
				case PolicyDelegateHandleType.Sync:
					try
					{
						return await Task.Run(() => HandleWhenAllReallySync(policyDelegateInfos, token), token).ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
					{
						var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
						return (handledResults, LastPolicyResultState.FromCanceled());
					}
				case PolicyDelegateHandleType.Misc:
					return await HandleAllMisc(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				case PolicyDelegateHandleType.Async:
					return await HandleWhenAllAsync(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				default:
					throw new NotImplementedException();
			}
		}

		internal static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllBySyncType<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, PolicyDelegateHandleType handleType, CancellationToken token = default, bool configureAwait = false)
		{
			if (!policyDelegateInfos.Any())
				return (new FlexSyncEnumerable<PolicyDelegateResult<T>>(), LastPolicyResultState.Default());
			switch (handleType)
			{
				case PolicyDelegateHandleType.Sync:
					try
					{
						return await Task.Run(() => HandleWhenAllReallySync(policyDelegateInfos, token), token).ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
					{
						var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
						return (handledResults, LastPolicyResultState.FromCanceled());
					}
				case PolicyDelegateHandleType.Misc:
					return await HandleAllMisc(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				case PolicyDelegateHandleType.Async:
					return await HandleWhenAllAsync(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				default:
					throw new NotImplementedException();
			}
		}

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) HandleWhenAllReallySync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			if (!policyDelegateInfos.Any())
				return (handledResults, LastPolicyResultState.Default());

			PolicyResult<T> curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}

				curPolResult = si.HandleSafely(token);

				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState) HandleWhenAllReallySync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			if (!policyDelegateInfos.Any())
				return (handledResults, LastPolicyResultState.Default());

			PolicyResult curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}

				curPolResult = si.HandleSafely(token);

				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		private static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllMisc(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>(!configureAwait);
			PolicyResult curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.HandleSafely(token);
				}
				else
				{
					curPolResult = await si.HandleSafelyAsync(configureAwait, token).ConfigureAwait(configureAwait);
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		private static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleAllMisc<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>(configureAwait);
			PolicyResult<T> curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.HandleSafely(token);
				}
				else
				{
					curPolResult = await si.HandleSafelyAsync(configureAwait, token).ConfigureAwait(configureAwait);
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		private static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleWhenAllAsync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>(!configureAwait);
			PolicyResult<T> curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}
				curPolResult = await si.HandleSafelyAsync(configureAwait, token).ConfigureAwait(configureAwait);
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}

		private static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, LastPolicyResultState lastPolicyResultState)> HandleWhenAllAsync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>(!configureAwait);
			PolicyResult curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					return (handledResults, LastPolicyResultState.FromCanceled());
				}
				curPolResult = await si.HandleSafelyAsync(configureAwait, token).ConfigureAwait(configureAwait);
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult.ToLastPolicyResultState());
		}
	}
}
