using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegateCollection : PolicyDelegateCollectionBase<PolicyDelegate>, IPolicyDelegateCollection, INeedDelegateCollection
	{
		private IPolicyDelegateResultsToErrorConverter _errorConverter;

		public static IPolicyDelegateCollection Create(IPolicyBase pol, Action action, int n = 1)
		{
			var res = new PolicyDelegateCollection();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, action);
			}
			return res;
		}

		public static IPolicyDelegateCollection Create(IPolicyBase pol, Func<CancellationToken, Task> func, int n = 1)
		{
			var res = new PolicyDelegateCollection();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, func);
			}
			return res;
		}

		public static IPolicyDelegateCollection Create(params PolicyDelegate[] errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		public static IPolicyDelegateCollection Create(IEnumerable<PolicyDelegate> errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		private PolicyDelegateCollection() { }

		private static IPolicyDelegateCollection FromPolicyDelegates(IEnumerable<PolicyDelegate> errorPolicyInfos)
		{
			errorPolicyInfos.ThrowIfAnyPolicyWithoutDelegateExists();

			var res = new PolicyDelegateCollection();

			foreach (var errorPolicy in errorPolicyInfos)
			{
				res.WithPolicyDelegate(errorPolicy);
			}
			return res;
		}

		public IPolicyDelegateCollection WithPolicyDelegate(PolicyDelegate errorPolicy)
		{
			AddPolicyDelegate(errorPolicy);
			return this;
		}

		public INeedDelegateCollection WithPolicy(IPolicyBase policyBase)
		{
			WithPolicyDelegate(policyBase.ToPolicyDelegate());
			return this;
		}

		public IPolicyDelegateCollection AndDelegate(Action action)
		{
			this.LastOrDefault()?.SetDelegate(action);
			return this;
		}

		public IPolicyDelegateCollection AndDelegate(Func<CancellationToken, Task> func)
		{
			this.LastOrDefault()?.SetDelegate(func);
			return this;
		}

		public IPolicyDelegateCollection WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateResultsToErrorConverter();
			return this;
		}

		public IPolicyDelegateCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public IPolicyDelegateCollection IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public IPolicyDelegateCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
			return this;
		}

		public IPolicyDelegateCollection ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		public IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act);
			return this;
		}

		public IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func);
			return this;
		}

		public Task<PolicyDelegateCollectionResult> HandleAllAsync(CancellationToken token = default) => HandleAllAsync(false, token);

		public async Task<PolicyDelegateCollectionResult> HandleAllAsync(bool configAwait, CancellationToken token)
		{
			PolicyDelegateHandleType handleType = this.GetHandleType();
			var (HandleResults, PolResult) = await PolicyDelegatesHandler.HandleAllBySyncType(this, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, PolResult);
		}

		public PolicyDelegateCollectionResult HandleAll(CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = this.GetHandleType();
			(IEnumerable<PolicyDelegateResult> HandleResults, PolicyResult PolResult) result;
			if (handleType == PolicyDelegateHandleType.Sync)
			{
				result = PolicyDelegatesHandler.HandleWhenAllSync(this, token);
			}
			else
			{
				result = PolicyDelegatesHandler.HandleAllForceSync(this, token);
			}
			return GetResultOrThrow(result.HandleResults, result.PolResult);
		}

		internal PolicyDelegateCollectionResult GetResultOrThrow(IEnumerable<PolicyDelegateResult> handledResults, PolicyResult polResult)
		{
			ThrowErrorIfNeed(polResult, handledResults);

			return new PolicyDelegateCollectionResult(handledResults, this.Skip(handledResults.Count()));
			void ThrowErrorIfNeed(PolicyResult policyResult, IEnumerable<PolicyDelegateResult> hResults)
			{
				if (policyResult == null) return;
				if (policyResult.IsFailed && ThrowOnLastFailed)
				{
					throw _errorConverter.ToExceptionConverter()(hResults);
				}
			}
		}
	}
}
