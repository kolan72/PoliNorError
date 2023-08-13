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

		public IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act, CancellationType convertType = CancellationType.Precancelable)
		{
			return AddPolicyResultHandlerForAll(act.ToCancelableAction(convertType));
		}

		public IPolicyDelegateCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act);
			return this;
		}

		public IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func, CancellationType convertType = CancellationType.Precancelable)
		{
			return AddPolicyResultHandlerForAll(func.ToCancelableFunc(convertType));
		}

		public IPolicyDelegateCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func);
			return this;
		}

		public IPolicyDelegateCollectionHandler BuildCollectionHandler()
		{
			return new PolicyDelegateCollectionHandler(this);
		}

		internal IPolicyDelegateResultsToErrorConverter ErrorConverter => _errorConverter;
	}
}
