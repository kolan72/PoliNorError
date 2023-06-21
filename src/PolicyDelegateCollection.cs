using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegateCollection : PolicyDelegateCollectionBase<PolicyDelegate>, IWithPolicy<PolicyDelegateCollection>
	{
		private IPolicyDelegateResultsToErrorConverter _errorConverter;

		public static PolicyDelegateCollection CreateFromPolicy(IPolicyBase pol, int n = 1)
		{
			var res = new PolicyDelegateCollection();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicy(pol);
			}
			return res;
		}

		public static PolicyDelegateCollection CreateFromPolicies(IEnumerable<IPolicyBase> errorPolicies) => FromPolicies(errorPolicies);

		public static PolicyDelegateCollection Create(params PolicyDelegate[] errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		public static PolicyDelegateCollection Create(IEnumerable<PolicyDelegate> errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		private PolicyDelegateCollection() { }

		private static PolicyDelegateCollection FromPolicies(IEnumerable<IPolicyBase> errorPolicies)
		{
			if (!errorPolicies.Any())
			{
				return new PolicyDelegateCollection();
			}
			var res = new PolicyDelegateCollection();
			foreach (var errorPolicy in errorPolicies)
			{
				res.WithPolicy(errorPolicy);
			}
			return res;
		}

		private static PolicyDelegateCollection FromPolicyDelegates(IEnumerable<PolicyDelegate> errorPolicyInfos)
		{
			if (!errorPolicyInfos.AnyWithDelegate())
			{
				//			If no policy with delegate, we can simplify collections creation.
				return FromPolicies(errorPolicyInfos.Select(pi => pi.Policy));
			}

			errorPolicyInfos.ThrowIfNotLastPolicyWithoutDelegateExists();

			var res = new PolicyDelegateCollection();

			foreach (var errorPolicy in errorPolicyInfos)
			{
				res.WithPolicyDelegate(errorPolicy);
			}
			return res;
		}

		internal PolicyDelegateCollection Append(IEnumerable<PolicyDelegate> policyDelegateInfos)
		{
			_syncInfos.AddRange(policyDelegateInfos);
			return this;
		}

		internal PolicyDelegateCollection RemoveLast()
		{
			_syncInfos.RemoveLast();
			return this;
		}

		public PolicyDelegateCollection WithPolicyDelegate(PolicyDelegate errorPolicy)
		{
			AddPolicyDelegate(errorPolicy);
			return this;
		}

		public PolicyDelegateCollection WithPolicyAndDelegate(IPolicyBase errorPolicy, Func<CancellationToken, Task> func) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public PolicyDelegateCollection WithPolicyAndDelegate(IPolicyBase errorPolicy, Action action) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(action));

		public PolicyDelegateCollection WithPolicy(Func<IPolicyBase> func) => WithPolicy(func());

		public PolicyDelegateCollection WithPolicy(IPolicyBase policyBase)
		{
			return WithPolicyDelegate(policyBase.ToPolicyDelegate());
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

		internal void ClearDelegates()
		{
			if (this.IsEmpty()) return;
			foreach (var polInfo in this)
			{
				polInfo.ClearDelegate();
			}
		}

		public PolicyDelegateCollection WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateResultsToErrorConverter();
			return this;
		}

		public PolicyDelegateCollection SetCommonDelegate(Action action)
		{
			var res = SetCommonDelegateInner(action);
			if (res != SettingPolicyDelegateResult.Success) res.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection SetCommonDelegate(Func<CancellationToken, Task> func)
		{
			var res = SetCommonDelegateInner(func);
			if (res != SettingPolicyDelegateResult.Success) res.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection AndDelegate(Action action)
		{
			var setResult = SetLastPolicyDelegate(action);
			if (setResult != SettingPolicyDelegateResult.Success) setResult.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection AndDelegate(Func<CancellationToken, Task> func)
		{
			var setResult = SetLastPolicyDelegate(func);
			if (setResult != SettingPolicyDelegateResult.Success) setResult.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public PolicyDelegateCollection IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyDelegateCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
			return this;
		}

		public PolicyDelegateCollection ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		internal SettingPolicyDelegateResult SetLastPolicyDelegate(Action action)
		{
			var lpd = this.CheckLastPolicyDelegateCanBeSet();
			if (lpd != SettingPolicyDelegateResult.None)
				return lpd;

			LastPolicyDelegate.SetDelegate(action);
			return SettingPolicyDelegateResult.Success;
		}

		internal SettingPolicyDelegateResult SetLastPolicyDelegate(Func<CancellationToken, Task> func)
		{
			var lpd = this.CheckLastPolicyDelegateCanBeSet();
			if (lpd != SettingPolicyDelegateResult.None)
				return lpd;

			LastPolicyDelegate.SetDelegate(func);
			return SettingPolicyDelegateResult.Success;
		}

		internal SettingPolicyDelegateResult SetCommonDelegateInner(Action action)
		{
			if (this.IsEmpty()) return SettingPolicyDelegateResult.Empty;

			if (_syncInfos.AnyWithDelegate()) return SettingPolicyDelegateResult.AlreadySet;

			foreach (var polInfo in this)
			{
				polInfo.SetDelegate(action);
			}
			return SettingPolicyDelegateResult.Success;
		}

		internal SettingPolicyDelegateResult SetCommonDelegateInner(Func<CancellationToken, Task> func)
		{
			if (this.IsEmpty()) return SettingPolicyDelegateResult.Empty;

			if (_syncInfos.AnyWithDelegate()) return SettingPolicyDelegateResult.AlreadySet;

			foreach (var polInfo in this)
			{
				polInfo.SetDelegate(func);
			}
			return SettingPolicyDelegateResult.Success;
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
