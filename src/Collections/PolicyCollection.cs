using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class PolicyCollection : IEnumerable<IPolicyBase>, IWithPolicy<PolicyCollection>, ICanAddErrorProcessor
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

		public PolicyCollection AddPolicyResultHandlerForLast(Action<PolicyResult> act)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast(Action<PolicyResult> act, CancellationType convertType)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(act, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast(Action<PolicyResult, CancellationToken> act)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast(Func<PolicyResult, Task> func)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(func, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast(Func<PolicyResult, CancellationToken, Task> func)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast<T>(Action<PolicyResult<T>> act)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast<T>(Action<PolicyResult<T>> act, CancellationType convertType)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(act, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(act);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast<T>(Func<PolicyResult<T>, Task> func)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(func);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(func, convertType);
			return this;
		}

		public PolicyCollection AddPolicyResultHandlerForLast<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			this.AddPolicyResultHandlerToLastPolicyInner(func);
			return this;
		}

		/// <summary>
		/// For the last policy in the <see cref="PolicyCollection"/>, this method adds a special PolicyResult handler that sets <see cref="PolicyResult.IsFailed"/> to true only if the executed <paramref name="predicate"/> returns true.
		/// </summary>
		/// <param name="predicate">A predicate that a PolicyResult should satisfy.</param>
		/// <returns></returns>
		public PolicyCollection SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			this.SetPolicyResultFailedIfInner(predicate);
			return this;
		}

		///<inheritdoc cref="SetPolicyResultFailedIf"/>
		public PolicyCollection SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			this.SetPolicyResultFailedIfInner(predicate);
			return this;
		}

		public PolicyCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilterForAll(func);
			return this;
		}

		public PolicyCollection IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilterForAll(handledErrorFilter);
			return this;
		}

		public PolicyCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilterForAll(func);
			return this;
		}

		public PolicyCollection ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilterForAll(handledErrorFilter);
			return this;
		}

		public PolicyCollection IncludeErrorForLast<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilterForLast(func);
			return this;
		}

		public PolicyCollection IncludeErrorForLast(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilterForLast(handledErrorFilter);
			return this;
		}

		public PolicyCollection ExcludeErrorForLast<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilterForLast(func);
			return this;
		}

		public PolicyCollection ExcludeErrorForLast(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilterForLast(handledErrorFilter);
			return this;
		}

		/// <summary>
		/// Specifies two types-based filter condition for including an exception in the processing performed by the last policy of the PolicyCollection
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <returns></returns>
		public PolicyCollection IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception
		{
			this.AddIncludedErrorSetFilter<TException1, TException2>();
			return this;
		}

		/// <summary>
		/// Specifies the <see cref= "IErrorSet" /> interface-based filter conditions for including an exception in the processing performed by the last policy of the PolicyCollection
		/// </summary>
		/// <param name="errorSet"><see cref="IErrorSet"/></param>
		/// <returns></returns>
		public PolicyCollection IncludeErrorSet(IErrorSet errorSet)
		{
			this.AddIncludedErrorSetFilter(errorSet);
			return this;
		}

		/// <summary>
		/// Specifies two types-based filter condition for excluding an exception from the processing performed by the last policy of the PolicyCollection
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <returns></returns>
		public PolicyCollection ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception
		{
			this.AddExcludedErrorSetFilter<TException1, TException2>();
			return this;
		}

		/// <summary>
		/// Specifies the type- and optionally predicate-based filter condition for the inner exception of a handling exception to be included in the handling by the last policy of the PolicyCollection.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public PolicyCollection IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception
		{
			this.AddIncludedInnerErrorFilter(predicate);
			return this;
		}

		/// <summary>
		/// Specifies the type- and optionally predicate-based filter condition for the inner exception of a handling exception to be excluded from the handling by the last policy of the PolicyCollection.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public PolicyCollection ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception
		{
			this.AddExcludedInnerErrorFilter(predicate);
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

		/// <summary>
		/// Returns an <see cref="OuterPolicyRegistrar{Policy}"></see> with the <see cref="OuterPolicyRegistrar{Policy}.OuterPolicy"></see> that wraps the current PolicyCollection.
		/// </summary>
		/// <typeparam name="TWrapperPolicy"></typeparam>
		/// <param name="wrapperPolicy">>The policy that will wrap the current policy</param>
		/// <param name="throwOnWrappedCollectionFailed">Shows how an exception will be generated if the last policy in the PolicyCollection fails.</param>
		/// <returns></returns>
		public OuterPolicyRegistrar<TWrapperPolicy> WrapUp<TWrapperPolicy>(TWrapperPolicy wrapperPolicy, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed = ThrowOnWrappedCollectionFailed.LastError) where TWrapperPolicy : Policy
		{
			return new OuterPolicyRegistrar<TWrapperPolicy>(wrapperPolicy, this, throwOnWrappedCollectionFailed);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<IPolicyBase> GetEnumerator() => _policies.GetEnumerator();
	}
}
