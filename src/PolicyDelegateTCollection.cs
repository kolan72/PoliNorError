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

		public static PolicyDelegateCollection<T> Create() => new PolicyDelegateCollection<T>();

		private bool _terminated;
		private IPolicyResultsToErrorConverter<T> _errorConverter;

		private PolicyDelegateCollection(){}

		public static PolicyDelegateCollection<T> FromPolicies(params IPolicyBase[] errorPolicies)
		{
			if (errorPolicies.Length == 0)
			{
				return Create();
			}
			return FromPolicies((IEnumerable<IPolicyBase>)errorPolicies);
		}

		public static PolicyDelegateCollection<T> FromPolicies(IEnumerable<IPolicyBase> errorPolicies)
		{
			var res = new PolicyDelegateCollection<T>();
			foreach (var errorPolicy in errorPolicies)
			{
				res.WithPolicy(errorPolicy);
			}
			return res;
		}

		public static PolicyDelegateCollection<T> FromPolicyDelegates(IEnumerable<PolicyDelegate<T>> errorPolicyInfos)
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

		public static PolicyDelegateCollection<T> FromPolicyDelegates(params PolicyDelegate<T>[] errorPolicyInfos)
		{
			if (errorPolicyInfos.Length == 0)
			{
				return Create();
			}
			return FromPolicyDelegates((IEnumerable<PolicyDelegate<T>>)errorPolicyInfos);
		}

		public static PolicyDelegateCollection<T> FromOneClonedPolicy(IPolicyBase pol, int n)
		{
			var res = new PolicyDelegateCollection<T>();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicy(pol);
			}
			return res;
		}

		public Task<PolicyDelegateCollectionResult<T>> HandleAllAsync(CancellationToken token = default) => HandleAllAsync(false, token);

		public async Task<PolicyDelegateCollectionResult<T>> HandleAllAsync(bool configAwait, CancellationToken token)
		{
			PolicyDelegateHandleType handleType = this.GetHandleType();

			(IEnumerable<PolicyHandledResult<T>> HandleResults, PolicyResult<T> PolResult) = await PolicyDelegatesHandler.HandleAllBySyncType(this, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, PolResult);
		}

		public PolicyDelegateCollectionResult<T> HandleAll(CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = this.GetHandleType();
			(IEnumerable<PolicyHandledResult<T>> HandleResults, PolicyResult<T> PolResult) result;
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

		internal SettingPolicyDelegateResult SetCommonDelegate(Func<T> func)
		{
			if (this.IsEmpty()) return SettingPolicyDelegateResult.Empty;

			if (_syncInfos.AnyWithDelegate()) return SettingPolicyDelegateResult.AlreadySet;

			foreach (var polInfo in this)
			{
				polInfo.SetDelegate(func);
			}
			return SettingPolicyDelegateResult.Success;
		}

		internal SettingPolicyDelegateResult SetCommonDelegate(Func<CancellationToken, Task<T>> func)
		{
			if (this.IsEmpty()) return SettingPolicyDelegateResult.Empty;

			if (_syncInfos.AnyWithDelegate()) return SettingPolicyDelegateResult.AlreadySet;

			foreach (var polInfo in this)
			{
				polInfo.SetDelegate(func);
			}
			return SettingPolicyDelegateResult.Success;
		}

		public void ClearDelegates()
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

		public PolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyResultsToErrorConverter<T> errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new PolicyDelegateCollectionHandleExceptionConverter<T>();
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal PolicyDelegateCollectionResult<T> GetResultOrThrow(IEnumerable<PolicyHandledResult<T>> handledResults, PolicyResult<T> polResult)
		{
			ThrowErrorIfNeed(polResult, handledResults);

			return new PolicyDelegateCollectionResult<T>(handledResults,  this.Skip(handledResults.Count()));
			void ThrowErrorIfNeed(PolicyResult<T> policyResult, IEnumerable<PolicyHandledResult<T>> hResults)
			{
				if (policyResult == null) return;
				if (policyResult.IsFailed && ThrowOnLastFailed)
				{
					throw _errorConverter.ToExceptionConverter()(hResults);
				}
			}
		}

		public PolicyDelegateCollection<T> ForError(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyDelegateCollection<T>  ForError<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public  PolicyDelegateCollection<T> ExcludeError(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		public  PolicyDelegateCollection<T> ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
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

		public PolicyDelegateCollection<T> WithCommonDelegate(Func<CancellationToken, Task<T>> func)
		{
			var res = SetCommonDelegate(func);
			if (res != SettingPolicyDelegateResult.Success) res.ThrowErrorByResult();
			return this;
		}

		public PolicyDelegateCollection<T> WithCommonDelegate(Func<T> func)
		{
			var res = SetCommonDelegate(func);
			if (res != SettingPolicyDelegateResult.Success) res.ThrowErrorByResult();
			return this;
		}
	}
}
