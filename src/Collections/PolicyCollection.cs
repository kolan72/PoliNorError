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

		public PolicyCollection AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken> act)
		{
			this.SetResultHandler(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Action<PolicyResult> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.SetResultHandler(act, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task> func)
		{
			this.SetResultHandler(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll(Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.SetResultHandler(func, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			this.SetResultHandler(act, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			this.SetResultHandler(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForAll<T>(Func<PolicyResult<T>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
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

		public PolicyDelegateCollectionResult HandleDelegate(Action action, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(action);
			return policyDelegateCollection.HandleAll(token);
		}

		public PolicyDelegateCollectionResult HandleDelegate(Func<CancellationToken, Task> func, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(func);
			return policyDelegateCollection.HandleAll(token);
		}

		public PolicyDelegateCollectionResult<T> HandleDelegate<T>(Func<T> action, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(action);
			return policyDelegateCollection.HandleAll(token);
		}

		public PolicyDelegateCollectionResult<T> HandleDelegate<T>(Func<CancellationToken, Task<T>> func, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(func);
			return policyDelegateCollection.HandleAll(token);
		}

		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Action action, CancellationToken token = default) => HandleDelegateAsync(action, false, token);

		public async Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Action action, bool configAwait, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(action);
			return await policyDelegateCollection.HandleAllAsync(configAwait, token);
		}

		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Func<CancellationToken, Task> func, CancellationToken token = default) => HandleDelegateAsync(func, false, token);

		public async Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Func<CancellationToken, Task> func, bool configAwait, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(func);
			return await policyDelegateCollection.HandleAllAsync(configAwait, token);
		}

		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<T> func, CancellationToken token = default) => HandleDelegateAsync(func, false, token);

		public async Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<T> func, bool configAwait, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(func);
			return await policyDelegateCollection.HandleAllAsync(configAwait, token);
		}

		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken token = default) => HandleDelegateAsync(func, false, token);

		public async Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<CancellationToken, Task<T>> func, bool configAwait, CancellationToken token = default)
		{
			var policyDelegateCollection = ToPolicyDelegateCollection(func);
			return await policyDelegateCollection.HandleAllAsync(configAwait, token);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<IPolicyBase> GetEnumerator() => _policies.GetEnumerator();
	}
}
