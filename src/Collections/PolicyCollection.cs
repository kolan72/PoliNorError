using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class PolicyCollection : IEnumerable<IPolicyBase>, IWithPolicy<PolicyCollection>
	{
		protected readonly List<IPolicyBase> _policies = new List<IPolicyBase>();

		/// <summary>
		/// Creates a collection from a single policy, which will be added n times.
		/// </summary>
		/// <param name="policy">The policy that will be added</param>
		/// <param name="n">The number of times</param>
		/// <returns></returns>
		public static PolicyCollection Create(IPolicyBase policy, int n = 1)
		{
			var res = new PolicyCollection();
			for (int i = 0; i < n; i++)
			{
				res.WithPolicy(policy);
			}
			return res;
		}

		public static PolicyCollection Create(params IPolicyBase[] policies) => FromPolicies(policies);

		public static PolicyCollection Create(IEnumerable<IPolicyBase> errorPolicies) => FromPolicies(errorPolicies);

		public PolicyCollection WithPolicy(Func<IPolicyBase> func) => WithPolicy(func());

		public PolicyCollection WithPolicy(IPolicyBase policyBase)
		{
			_policies.Add(policyBase);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act)
		{
			this.SetResultHandler(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act, CancellationType convertType)
		{
			this.SetResultHandler(act, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act)
		{
			this.SetResultHandler(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func)
		{
			this.SetResultHandler(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func)
		{
			this.SetResultHandler(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			this.SetResultHandler(func, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>> act)
		{
			this.SetResultHandler(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			this.SetResultHandler(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>> act, CancellationType convertType)
		{
			this.SetResultHandler(act, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Func<PolicyResult<T>, Task> func)
		{
			this.SetResultHandler(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			this.SetResultHandler(func, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			this.SetResultHandler(func);
			return this;
		}

		public PolicyCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public PolicyCollection IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
			return this;
		}

		public PolicyCollection ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		private static PolicyCollection FromPolicies(IEnumerable<IPolicyBase> errorPolicies)
		{
			var res = new PolicyCollection();
			foreach (var errorPolicy in errorPolicies)
			{
				res.WithPolicy(errorPolicy);
			}
			return res;
		}

		public IPolicyDelegateCollection ToPolicyDelegateCollection(Action action)
		{
			return PolicyDelegateCollection.Create(_policies.Select(p => p.ToPolicyDelegate(action)));
		}

		public IPolicyDelegateCollection ToPolicyDelegateCollection(Func<CancellationToken, Task> func)
		{
			return PolicyDelegateCollection.Create(_policies.Select(p => p.ToPolicyDelegate(func)));
		}

		public IPolicyDelegateCollection<T> ToPolicyDelegateCollection<T>(Func<T> func)
		{
			return PolicyDelegateCollection<T>.Create(_policies.Select(p => p.ToPolicyDelegate(func)));
		}

		public IPolicyDelegateCollection<T> ToPolicyDelegateCollection<T>(Func<CancellationToken, Task<T>> func)
		{
			return PolicyDelegateCollection<T>.Create(_policies.Select(p => p.ToPolicyDelegate(func)));
		}

		/// <summary>
		/// Creates PoiicyDelegateCollection from this collection and <paramref name="action"/> and returns  <see cref="IPolicyDelegateCollection"/>
		/// from calling <see cref="IPolicyDelegateCollection.BuildCollectionHandler"/>
		/// </summary>
		/// <param name="action">The delegate will be common to the entire collection.</param>
		/// <returns></returns>
		public IPolicyDelegateCollectionHandler BuildCollectionHandlerFor(Action action)
		{
			return ToPolicyDelegateCollection(action).BuildCollectionHandler();
		}

		/// <summary>
		/// Creates PoiicyDelegateCollection from this collection and <paramref name="func"/> and returns  <see cref="IPolicyDelegateCollection"/>
		/// from calling <see cref="IPolicyDelegateCollection.BuildCollectionHandler"/>
		/// </summary>
		/// <param name="func">The delegate will be common to the entire collection.</param>
		/// <returns></returns>
		public IPolicyDelegateCollectionHandler BuildCollectionHandlerFor(Func<CancellationToken, Task> func)
		{
			return ToPolicyDelegateCollection(func).BuildCollectionHandler();
		}

		/// <summary>
		/// Creates PoiicyDelegateCollection{T} from this collection and <paramref name="func"/> and returns  <see cref="IPolicyDelegateCollection{T}"/>
		/// from calling <see cref="IPolicyDelegateCollection{T}.BuildCollectionHandler"/>
		/// </summary>
		/// <param name="func">The delegate will be common to the entire collection.</param>
		/// <returns></returns>
		public IPolicyDelegateCollectionHandler<T> BuildCollectionHandlerFor<T>(Func<T> func)
		{
			return ToPolicyDelegateCollection(func).BuildCollectionHandler();
		}

		/// <summary>
		/// Creates PoiicyDelegateCollection{T} from this collection and <paramref name="func"/> and returns  <see cref="IPolicyDelegateCollection{T}"/>
		/// from calling <see cref="IPolicyDelegateCollection{T}.BuildCollectionHandler"/>
		/// </summary>
		/// <param name="func">The delegate will be common to the entire collection.</param>
		/// <returns></returns>
		public IPolicyDelegateCollectionHandler<T> BuildCollectionHandlerFor<T>(Func<CancellationToken, Task<T>> func)
		{
			return ToPolicyDelegateCollection(func).BuildCollectionHandler();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<IPolicyBase> GetEnumerator() => _policies.GetEnumerator();
	}
}
