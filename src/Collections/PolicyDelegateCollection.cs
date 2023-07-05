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

		public static PolicyDelegateCollection Create(IPolicyBase pol, Action action, int n = 1)
		{
			var res = new PolicyDelegateCollection();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, action);
			}
			return res;
		}

		public static PolicyDelegateCollection Create(IPolicyBase pol, Func<CancellationToken, Task> func, int n = 1)
		{
			var res = new PolicyDelegateCollection();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, func);
			}
			return res;
		}

		public static PolicyDelegateCollection Create(params PolicyDelegate[] errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		public static PolicyDelegateCollection Create(IEnumerable<PolicyDelegate> errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		private PolicyDelegateCollection() { }

		private static PolicyDelegateCollection FromPolicyDelegates(IEnumerable<PolicyDelegate> errorPolicyInfos)
		{
			errorPolicyInfos.ThrowIfNotLastPolicyWithoutDelegateExists();

			var res = new PolicyDelegateCollection();

			foreach (var errorPolicy in errorPolicyInfos)
			{
				res.WithPolicyDelegate(errorPolicy);
			}
			return res;
		}

		public PolicyDelegateCollection WithPolicy(IPolicyBase policyBase)
		{
			return WithPolicyDelegate(policyBase.ToPolicyDelegate());
		}

		public PolicyDelegateCollection WithPolicyAndDelegate(IPolicyBase errorPolicy, Func<CancellationToken, Task> func) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public PolicyDelegateCollection WithPolicyAndDelegate(IPolicyBase errorPolicy, Action action) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(action));

		public PolicyDelegateCollection WithPolicyDelegate(PolicyDelegate errorPolicy)
		{
			AddPolicyDelegate(errorPolicy);
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

		public PolicyDelegateCollection WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateResultsToErrorConverter();
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

		public PolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act, convertType);
			return this;
		}

		public PolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act);
			return this;
		}

		public PolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func, convertType);
			return this;
		}

		public PolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func) 
		{
			this.Select(pd => pd.Policy).SetResultHandler(func);
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
	}
}
