using System;
using System.Collections.Generic;
using System.Linq;
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
			errorPolicyInfos.ThrowIfAnyPolicyWithoutDelegateExists();

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
			this.LastOrDefault()?.SetDelegate(func);
			return this;
		}

		public IPolicyDelegateCollection<T> AndDelegate(Func<T> func)
		{
			this.LastOrDefault()?.SetDelegate(func);
			return this;
		}

		public IPolicyDelegateCollection<T> WithThrowOnLastFailed(IPolicyDelegateResultsToErrorConverter<T> errorConverter = null)
		{
			_terminated = true;
			_errorConverter = errorConverter ?? new DefaultPolicyDelegateResultsToErrorConverter<T>((pdrs) => new PolicyDelegateCollectionException<T>(pdrs));
			return this;
		}

		public IPolicyDelegateCollection<T> IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilterForAll(func);
			return this;
		}

		public IPolicyDelegateCollection<T> ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilterForAll(func);
			return this;
		}

		public IPolicyDelegateCollectionHandler<T> BuildCollectionHandler()
		{
			return new PolicyDelegateCollectionHandler<T>(this);
		}

		internal IPolicyDelegateResultsToErrorConverter<T> ErrorConverter => _errorConverter;
	}
}
