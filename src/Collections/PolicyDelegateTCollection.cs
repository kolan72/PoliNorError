using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegateCollection<T> : PolicyDelegateCollectionBase<PolicyDelegate<T>>,  IWithPolicyBase<IPolicyNeedDelegateCollection<T>>, IPolicyNeedDelegateCollection<T>
	{
		private IPolicyDelegateResultsToErrorConverter<T> _errorConverter;

		public static PolicyDelegateCollection<T> Create(IPolicyBase pol, Func<T> func, int n = 1)
		{
			var res = new PolicyDelegateCollection<T>();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, func);
			}
			return res;
		}

		public static PolicyDelegateCollection<T> Create(IPolicyBase pol, Func<CancellationToken, Task<T>> func, int n = 1)
		{
			var res = new PolicyDelegateCollection<T>();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, func);
			}
			return res;
		}

		public static PolicyDelegateCollection<T> Create(params PolicyDelegate<T>[] errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		public static PolicyDelegateCollection<T> Create(IEnumerable<PolicyDelegate<T>> errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		private PolicyDelegateCollection(){}

		private static PolicyDelegateCollection<T> FromPolicyDelegates(IEnumerable<PolicyDelegate<T>> errorPolicyInfos)
		{
			errorPolicyInfos.ThrowIfNotLastPolicyWithoutDelegateExists();

			var res = new PolicyDelegateCollection<T>();

			foreach (var errorPolicy in errorPolicyInfos)
			{
				res.WithPolicyDelegate(errorPolicy);
			}
			return res;
		}

		public PolicyDelegateCollection<T> WithPolicyAndDelegate(IPolicyBase errorPolicy, Func<CancellationToken, Task<T>> func) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public PolicyDelegateCollection<T> WithPolicyAndDelegate(IPolicyBase errorPolicy, Func<T> func) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public PolicyDelegateCollection<T> WithPolicyDelegate(PolicyDelegate<T> errorPolicy)
		{
			AddPolicyDelegate(errorPolicy);
			return this;
		}

		public PolicyDelegateCollection<T> AndDelegate(Func<CancellationToken, Task<T>> func)
		{
			var setResult = SetLastPolicyDelegate(func);
			if (setResult != SettingPolicyDelegateResult.Success) setResult.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection<T> AndDelegate(Func<T> func)
		{
			var setResult = SetLastPolicyDelegate(func);
			if (setResult != SettingPolicyDelegateResult.Success) setResult.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateResultsToErrorConverter<T>();
			return this;
		}

		public PolicyDelegateCollection<T> IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyDelegateCollection<T> IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public PolicyDelegateCollection<T> ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
			return this;
		}

		public PolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act, convertType);
			return this;
		}

		public PolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>, CancellationToken> act)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act);
			return this;
		}

		public PolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func, convertType);
			return this;
		}

		public PolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func);
			return this;
		}

		public PolicyDelegateCollection<T> SetCommonDelegate(Func<CancellationToken, Task<T>> func)
		{
			var res = SetCommonDelegateInner(func);
			if (res != SettingPolicyDelegateResult.Success) res.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection<T> SetCommonDelegate(Func<T> func)
		{
			var res = SetCommonDelegateInner(func);
			if (res != SettingPolicyDelegateResult.Success) res.ThrowErrorByResult();
			return this;
		}

		public Task<PolicyDelegateCollectionResult<T>> HandleAllAsync(CancellationToken token = default) => HandleAllAsync(false, token);

		public async Task<PolicyDelegateCollectionResult<T>> HandleAllAsync(bool configAwait, CancellationToken token)
		{
			PolicyDelegateHandleType handleType = this.GetHandleType();

			(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult) = await PolicyDelegatesHandler.HandleAllBySyncType(this, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, PolResult);
		}

		public PolicyDelegateCollectionResult<T> HandleAll(CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = this.GetHandleType();
			(IEnumerable<PolicyDelegateResult<T>> HandleResults, PolicyResult<T> PolResult) result;
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

		internal SettingPolicyDelegateResult SetLastPolicyDelegate(Func<T> execute)
		{
			var lpd = this.CheckLastPolicyDelegateCanBeSet();
			if (lpd != SettingPolicyDelegateResult.None)
				return lpd;

			LastPolicyDelegate.SetDelegate(execute);
			return SettingPolicyDelegateResult.Success;
		}

		internal SettingPolicyDelegateResult SetLastPolicyDelegate(Func<CancellationToken, Task<T>> executeAsync)
		{
			var lpd = this.CheckLastPolicyDelegateCanBeSet();
			if (lpd != SettingPolicyDelegateResult.None)
				return lpd;

			LastPolicyDelegate.SetDelegate(executeAsync);
			return SettingPolicyDelegateResult.Success;
		}

		internal PolicyDelegateCollectionResult<T> GetResultOrThrow(IEnumerable<PolicyDelegateResult<T>> handledResults, PolicyResult<T> polResult)
		{
			ThrowErrorIfNeed(polResult, handledResults);

			return new PolicyDelegateCollectionResult<T>(handledResults, this.Skip(handledResults.Count()));
			void ThrowErrorIfNeed(PolicyResult<T> policyResult, IEnumerable<PolicyDelegateResult<T>> hResults)
			{
				if (policyResult == null) return;
				if (policyResult.IsFailed && ThrowOnLastFailed)
				{
					throw _errorConverter.ToExceptionConverter()(hResults);
				}
			}
		}

		internal SettingPolicyDelegateResult SetCommonDelegateInner(Func<T> func)
		{
			if (this.IsEmpty()) return SettingPolicyDelegateResult.Empty;

			if (_syncInfos.AnyWithDelegate()) return SettingPolicyDelegateResult.AlreadySet;

			foreach (var polInfo in this)
			{
				polInfo.SetDelegate(func);
			}
			return SettingPolicyDelegateResult.Success;
		}

		internal SettingPolicyDelegateResult SetCommonDelegateInner(Func<CancellationToken, Task<T>> func)
		{
			if (this.IsEmpty()) return SettingPolicyDelegateResult.Empty;

			if (_syncInfos.AnyWithDelegate()) return SettingPolicyDelegateResult.AlreadySet;

			foreach (var polInfo in this)
			{
				polInfo.SetDelegate(func);
			}
			return SettingPolicyDelegateResult.Success;
		}

		public IPolicyNeedDelegateCollection<T> WithPolicy(IPolicyBase policyBase)
		{
			return WithPolicyDelegate(policyBase.ToPolicyDelegate<T>());
		}
	}
}
