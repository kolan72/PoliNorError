using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegateCollection<T> : PolicyDelegateCollectionBase<PolicyDelegate<T>>, IPolicyDelegateCollection<T>, INeedDelegateCollection<T>
	{
		private IPolicyDelegateResultsToErrorConverter<T> _errorConverter;

		public static IPolicyDelegateCollection<T> Create(IPolicyBase pol, Func<T> func, int n = 1)
		{
			var res = new PolicyDelegateCollection<T>();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, func);
			}
			return res;
		}

		public static IPolicyDelegateCollection<T> Create(IPolicyBase pol, Func<CancellationToken, Task<T>> func, int n = 1)
		{
			var res = new PolicyDelegateCollection<T>();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicyAndDelegate(pol, func);
			}
			return res;
		}

		public static IPolicyDelegateCollection<T> Create(params PolicyDelegate<T>[] errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		public static IPolicyDelegateCollection<T> Create(IEnumerable<PolicyDelegate<T>> errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		private PolicyDelegateCollection(){}

		private static IPolicyDelegateCollection<T> FromPolicyDelegates(IEnumerable<PolicyDelegate<T>> errorPolicyInfos)
		{
			errorPolicyInfos.ThrowIfNotLastPolicyWithoutDelegateExists();

			var res = new PolicyDelegateCollection<T>();

			foreach (var errorPolicy in errorPolicyInfos)
			{
				res.WithPolicyDelegate(errorPolicy);
			}
			return res;
		}

		public IPolicyDelegateCollection<T> WithPolicyDelegate(PolicyDelegate<T> errorPolicy)
		{
			AddPolicyDelegate(errorPolicy);
			return this;
		}

		public INeedDelegateCollection<T> WithPolicy(IPolicyBase policyBase)
		{
			WithPolicyDelegate(policyBase.ToPolicyDelegate<T>());
			return this;
		}

		public IPolicyDelegateCollection<T> AndDelegate(Func<CancellationToken, Task<T>> func)
		{
			var setResult = SetLastPolicyDelegate(func);
			if (setResult != SettingPolicyDelegateResult.Success) setResult.ThrowErrorByResult();
			return this;
		}

		public IPolicyDelegateCollection<T> AndDelegate(Func<T> func)
		{
			var setResult = SetLastPolicyDelegate(func);
			if (setResult != SettingPolicyDelegateResult.Success) setResult.ThrowErrorByResult();
			return this;
		}

		public IPolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateResultsToErrorConverter<T>();
			return this;
		}

		public IPolicyDelegateCollection<T> IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public IPolicyDelegateCollection<T> IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public IPolicyDelegateCollection<T> ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		public IPolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
			return this;
		}

		public IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>, CancellationToken> act)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act);
			return this;
		}

		public IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func);
			return this;
		}

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
	}
}
