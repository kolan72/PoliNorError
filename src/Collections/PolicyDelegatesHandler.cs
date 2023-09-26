using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PoliNorError
{
	internal static class PolicyDelegatesHandler
    {
		internal static (IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult) HandleAllForceSync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			if (!policyDelegateInfos.Any())
				return (handledResults, null);

			PolicyResult curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = new PolicyResult();
					curPolResult.SetCanceled();
					break;
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.Handle(token);
				}
				else
				{
					var (policyResult, IsCanceled) = HandleAsyncAsSync(si);
					curPolResult = policyResult;
					if (IsCanceled)
					{
						curPolResult = new PolicyResult();
						curPolResult.SetCanceled();
						break;
					}
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);

			(PolicyResult policyResult, bool IsCanceled) HandleAsyncAsSync(PolicyDelegate si)
			{
				PolicyResult polResult = null;
				try
				{
					polResult = Task.Run(() => si.HandleAsync(false, token), token).Result;
					return (polResult, false);
				}
				catch (AggregateException ae) when (ae.HasCanceledException(token))
				{
					return (null, true);
				}
			}
		}

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult) HandleAllForceSync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			if (!policyDelegateInfos.Any())
				return (handledResults, null);

			PolicyResult<T> curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = new PolicyResult<T>();
					curPolResult.SetCanceled();
					break;
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.Handle(token);
				}
				else
				{
					var (policyResult, IsCanceled) = HandleAsyncAsSync(si);
					curPolResult = policyResult;
					if (IsCanceled)
					{
						curPolResult = new PolicyResult<T>();
						curPolResult.SetCanceled();
						break;
					}
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);

			(PolicyResult<T> policyResult, bool IsCanceled) HandleAsyncAsSync(PolicyDelegate<T> si)
			{
				PolicyResult<T> polResult = null;
				try
				{
					polResult = Task.Run(() => si.HandleAsync(false, token), token).Result;
					return (polResult, false);
				}
				catch (AggregateException ae) when (ae.HasCanceledException(token))
				{
					return (null, true);
				}
			}
		}

		internal static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult)> HandleAllBySyncType(IEnumerable<PolicyDelegate> policyDelegateInfos, PolicyDelegateHandleType handleType, CancellationToken token = default, bool configureAwait = false)
		{
			if (!policyDelegateInfos.Any())
				return (new FlexSyncEnumerable<PolicyDelegateResult>(), null);
			switch (handleType)
			{
				case PolicyDelegateHandleType.Sync:
					try
					{
						return await Task.Run(() => HandleWhenAllSync(policyDelegateInfos, token), token).ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
					{
						var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
						var curPolResult = PolicyResult.ForSync();
						curPolResult.SetCanceled();
						return (handledResults, curPolResult);
					}
				case PolicyDelegateHandleType.Misc:
					return await HandleAllMisc(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				case PolicyDelegateHandleType.Async:
					return await HandleWhenAllAsync(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				default:
					throw new NotImplementedException();
			}
		}

		internal static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult)> HandleAllBySyncType<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, PolicyDelegateHandleType handleType, CancellationToken token = default, bool configureAwait = false)
		{
			if (!policyDelegateInfos.Any())
				return (new FlexSyncEnumerable<PolicyDelegateResult<T>>(), null);
			switch (handleType)
			{
				case PolicyDelegateHandleType.Sync:
					try
					{
						return await Task.Run(() => HandleWhenAllSync(policyDelegateInfos, token), token).ConfigureAwait(configureAwait);
					}
					catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
					{
						var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
						var curPolResult = PolicyResult<T>.ForSync();
						curPolResult.SetCanceled();
						return (handledResults, curPolResult);
					}
				case PolicyDelegateHandleType.Misc:
					return await HandleAllMisc(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				case PolicyDelegateHandleType.Async:
					return await HandleWhenAllAsync(policyDelegateInfos, token, configureAwait).ConfigureAwait(configureAwait);
				default:
					throw new NotImplementedException();
			}
		}

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult) HandleWhenAllSync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			if (!policyDelegateInfos.Any())
				return (handledResults, null);

			PolicyResult<T> curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = new PolicyResult<T>();
					curPolResult.SetCanceled();
					break;
				}

				curPolResult = si.Handle(token);

				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult) HandleWhenAllSync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			if (!policyDelegateInfos.Any())
				return (handledResults, null);

			PolicyResult curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = new PolicyResult();
					curPolResult.SetCanceled();
					break;
				}

				curPolResult = si.Handle(token);

				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		private static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult)> HandleAllMisc(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = new PolicyResult();
					curPolResult.SetCanceled();
					break;
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.Handle(token);
				}
				else
				{
					curPolResult = await si.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		private static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult)> HandleAllMisc<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = new PolicyResult<T>();
					curPolResult.SetCanceled();
					break;
				}

				if (si.SyncType == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.Handle(token);
				}
				else
				{
					curPolResult = await si.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
				}
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		private static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult)> HandleWhenAllAsync<T>(IEnumerable<PolicyDelegate<T>> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = new PolicyResult<T>();
					curPolResult.SetCanceled();
					break;
				}
				curPolResult = await si.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		private static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult)> HandleWhenAllAsync(IEnumerable<PolicyDelegate> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = new PolicyResult();
					curPolResult.SetCanceled();
					break;
				}
				curPolResult = await si.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}
	}
}
