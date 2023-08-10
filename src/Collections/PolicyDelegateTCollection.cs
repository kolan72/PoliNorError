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

		public IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>> act, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return AddPolicyResultHandlerForAll(act.ToCancelableAction(convertType));
		}

		public IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Action<PolicyResult<T>, CancellationToken> act)
		{
			this.Select(pd => pd.Policy).SetResultHandler(act);
			return this;
		}

		public IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return AddPolicyResultHandlerForAll(func.ToCancelableFunc(convertType));
		}

		public IPolicyDelegateCollection<T> AddPolicyResultHandlerForAll(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			this.Select(pd => pd.Policy).SetResultHandler(func);
			return this;
		}

		public IPolicyDelegateCollectionHandler<T> BuildCollectionHandler()
		{
			return new PolicyDelegateCollectionHandler<T>(this);
		}

		internal IPolicyDelegateResultsToErrorConverter<T> ErrorConverter => _errorConverter;
	}
}
