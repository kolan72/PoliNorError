using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class PolicyDelegateCollection<T> : IEnumerable<PolicyDelegate<T>>
	{
		private readonly List<PolicyDelegate<T>> _syncInfos = new List<PolicyDelegate<T>>();
		private bool _terminated;
		private IPolicyDelegateResultsToErrorConverter<T> _errorConverter;

		public static PolicyDelegateCollection<T> CreateFromPolicy(IPolicyBase pol, int n = 1)
		{
			var res = new PolicyDelegateCollection<T>();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicy(pol);
			}
			return res;
		}

		public static PolicyDelegateCollection<T> CreateFromPolicies(IEnumerable<IPolicyBase> errorPolicies) => FromPolicies(errorPolicies);

		public static PolicyDelegateCollection<T> Create(params PolicyDelegate<T>[] errorPolicyInfos) => Create((IEnumerable<PolicyDelegate<T>>)errorPolicyInfos);

		public static PolicyDelegateCollection<T> Create(IEnumerable<PolicyDelegate<T>> errorPolicyInfos) => FromPolicyDelegates(errorPolicyInfos);

		private PolicyDelegateCollection(){}

		private static PolicyDelegateCollection<T> FromPolicies(IEnumerable<IPolicyBase> errorPolicies)
		{
			if (!errorPolicies.Any())
			{
				return new PolicyDelegateCollection<T>();
			}
			var res = new PolicyDelegateCollection<T>();
			foreach (var errorPolicy in errorPolicies)
			{
				res.WithPolicy(errorPolicy);
			}
			return res;
		}

		private static PolicyDelegateCollection<T> FromPolicyDelegates(IEnumerable<PolicyDelegate<T>> errorPolicyInfos)
		{
			if (!errorPolicyInfos.AnyWithDelegate())
			{
				//			If no policy with delegate, we can simplify collections creation.
				return FromPolicies(errorPolicyInfos.Select(pi => pi.Policy));
			}

			errorPolicyInfos.ThrowIfNotLastPolicyWithoutDelegateExists();

			var res = new PolicyDelegateCollection<T>();

			foreach (var errorPolicy in errorPolicyInfos)
			{
				res.WithPolicyDelegate(errorPolicy);
			}
			return res;
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

		internal PolicyDelegateCollection<T> Append(IEnumerable<PolicyDelegate<T>> policyDelegateInfos)
		{
			_syncInfos.AddRange(policyDelegateInfos);
			return this;
		}

		internal PolicyDelegateCollection<T> RemoveLast()
		{
			_syncInfos.RemoveLast();
			return this;
		}

		public PolicyDelegateCollection<T> WithPolicyDelegate(PolicyDelegate<T> errorPolicy)
		{
			this.ThrowIfInconsistency(errorPolicy);
			_syncInfos.Add(errorPolicy);
			return this;
		}

		public PolicyDelegateCollection<T> WithPolicyAndDelegate(IPolicyBase errorPolicy, Func<CancellationToken, Task<T>> func) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public PolicyDelegateCollection<T> WithPolicyAndDelegate(IPolicyBase errorPolicy, Func<T> func) => WithPolicyDelegate(errorPolicy.ToPolicyDelegate(func));

		public PolicyDelegateCollection<T> WithPolicy(Func<IPolicyBase> func) => WithPolicy(func());

		public PolicyDelegateCollection<T> WithPolicy(IPolicyBase errorPolicy)
		{
			return WithPolicyDelegate(errorPolicy.ToPolicyDelegate<T>());
		}

		public PolicyDelegate<T> LastPolicyDelegate => this.LastOrDefaultIfEmpty();

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

		internal void ClearDelegates()
		{
			if (this.IsEmpty()) return;
			foreach (var polInfo in this)
			{
				polInfo.ClearDelegate();
			}
		}
		public IEnumerable<IPolicyBase> Policies => _syncInfos.GetPolicies();

		public IEnumerator<PolicyDelegate<T>> GetEnumerator() => _syncInfos.GetEnumerator();

		public bool ThrowOnLastFailed => _terminated;

		public PolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateResultsToErrorConverter<T>();
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal PolicyDelegateCollectionResult<T> GetResultOrThrow(IEnumerable<PolicyDelegateResult<T>> handledResults, PolicyResult<T> polResult)
		{
			ThrowErrorIfNeed(polResult, handledResults);

			return new PolicyDelegateCollectionResult<T>(handledResults,  this.Skip(handledResults.Count()));
			void ThrowErrorIfNeed(PolicyResult<T> policyResult, IEnumerable<PolicyDelegateResult<T>> hResults)
			{
				if (policyResult == null) return;
				if (policyResult.IsFailed && ThrowOnLastFailed)
				{
					throw _errorConverter.ToExceptionConverter()(hResults);
				}
			}
		}

		public PolicyDelegateCollection<T> IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyDelegateCollection<T>  IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public  PolicyDelegateCollection<T> ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		public  PolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
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
	}
}
