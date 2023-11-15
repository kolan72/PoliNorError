using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class PolicyDelegateCollectionHandler<T> : IPolicyDelegateCollectionHandler<T>
	{
		private readonly PolicyDelegateCollection<T> _policyDelegates;

		public PolicyDelegateCollectionHandler(PolicyDelegateCollection<T> policyDelegates)
		{
			_policyDelegates = policyDelegates;
		}

		public PolicyDelegateCollectionResult<T> Handle(CancellationToken token = default)
		{
			var (HandleResults, PolResult) = PolicyDelegatesHandler.HandleAllSync(_policyDelegates, token);
			return GetResultOrThrow(HandleResults, PolResult);
		}

		public async Task<PolicyDelegateCollectionResult<T>> HandleAsync(bool configAwait = false, CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = _policyDelegates.GetHandleType();
			(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) = await PolicyDelegatesHandler.HandleAllBySyncType(_policyDelegates, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, lastPolicyResultState);
		}

		private PolicyDelegateCollectionResult<T> GetResultOrThrow(IEnumerable<PolicyDelegateResult<T>> handledResults, LastPolicyResultState resultState)
		{
			ThrowErrorIfNeed(handledResults);

			return new PolicyDelegateCollectionResult<T>(handledResults, _policyDelegates.Skip(handledResults.Count()).ToList(), resultState);
			void ThrowErrorIfNeed(IEnumerable<PolicyDelegateResult<T>> hResults)
			{
				if (resultState.IsFailed == true && _policyDelegates.ThrowOnLastFailed)
				{
					throw _policyDelegates.ErrorConverter.ToExceptionConverter()(hResults);
				}
			}
		}
	}
}
