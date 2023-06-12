using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PoliNorError
{
	internal static class PolicyDelegatesHandler
    {
		internal static (IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult) HandleAllForceSync(PolicyDelegateCollection policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();
			PolicyResult curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}

				if (si.UseSync == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.Handle(token);
				}
				else
				{
					var (policyResult, IsCanceled) = HandleAsyncAsSync(si);
					curPolResult = policyResult;
					if (IsCanceled)
					{
						handledResults.AddPolicyDelegateResultCanceled(si);
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

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult) HandleAllForceSync<T>(PolicyDelegateCollection<T> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();
			PolicyResult<T> curPolResult = null;

			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}

				if (si.UseSync == SyncPolicyDelegateType.Sync)
				{
					curPolResult = si.Handle(token);
				}
				else
				{
					var (policyResult, IsCanceled) = HandleAsyncAsSync(si);
					curPolResult = policyResult;
					if (IsCanceled)
					{
						handledResults.AddPolicyDelegateResultCanceled(si);
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

		internal static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult)> HandleAllBySyncType(PolicyDelegateCollection policyDelegateInfos, PolicyDelegateHandleType handleType, CancellationToken token = default, bool configureAwait = false)
		{
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
						var curPolResult = handledResults.AddPolicyDelegateResultCanceled(policyDelegateInfos.FirstOrDefault());
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

		internal static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult)> HandleAllBySyncType<T>(PolicyDelegateCollection<T> policyDelegateInfos, PolicyDelegateHandleType handleType, CancellationToken token = default, bool configureAwait = false)
		{
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
						var curPolResult = handledResults.AddPolicyDelegateResultCanceled(policyDelegateInfos.FirstOrDefault());
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

		private static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult)> HandleAllMisc(PolicyDelegateCollection policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}

				if (si.UseSync == SyncPolicyDelegateType.Sync)
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

		private static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult)> HandleAllMisc<T>(PolicyDelegateCollection<T> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}

				if (si.UseSync == SyncPolicyDelegateType.Sync)
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

		internal static (IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult) HandleWhenAllSync<T>(PolicyDelegateCollection<T> policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult<T>>();

			PolicyResult<T> curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}

				curPolResult = si.Handle(token);

				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		internal static (IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult) HandleWhenAllSync(PolicyDelegateCollection policyDelegateInfos, CancellationToken token = default)
		{
			var handledResults = new FlexSyncEnumerable<PolicyDelegateResult>();

			PolicyResult curPolResult = null;
			foreach (var si in policyDelegateInfos)
			{
				if (curPolResult.NotFailedOrCanceled())
				{
					break;
				}

				if (token.IsCancellationRequested)
				{
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}

				curPolResult = si.Handle(token);

				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		private static async Task<(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult)> HandleWhenAllAsync<T>(PolicyDelegateCollection<T> policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}
				curPolResult = await si.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}

		private static async Task<(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult)> HandleWhenAllAsync(PolicyDelegateCollection policyDelegateInfos, CancellationToken token = default, bool configureAwait = false)
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
					curPolResult = handledResults.AddPolicyDelegateResultCanceled(si);
					break;
				}
				curPolResult = await si.HandleAsync(configureAwait, token).ConfigureAwait(configureAwait);
				handledResults.AddPolicyDelegateResult(si, curPolResult);
			}
			return (handledResults, curPolResult);
		}
	}
}
