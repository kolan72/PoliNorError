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
			PolicyDelegateHandleType handleType = _policyDelegates.GetHandleType();
			(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) result;
			if (handleType == PolicyDelegateHandleType.Sync)
			{
				result = PolicyDelegatesHandler.HandleWhenAllSync(_policyDelegates, token);
			}
			else
			{
				result = PolicyDelegatesHandler.HandleAllForceSync(_policyDelegates, token);
			}
			return GetResultOrThrow(result.HandleResults, result.lastPolicyResultState.IsFailed);
		}

		public async Task<PolicyDelegateCollectionResult<T>> HandleAsync(bool configAwait = false, CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = _policyDelegates.GetHandleType();

			(IEnumerable<PolicyDelegateResult<T>> HandleResults, LastPolicyResultState lastPolicyResultState) = await PolicyDelegatesHandler.HandleAllBySyncType(_policyDelegates, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, lastPolicyResultState.IsFailed);
		}

		private PolicyDelegateCollectionResult<T> GetResultOrThrow(IEnumerable<PolicyDelegateResult<T>> handledResults, bool? isFailed)
		{
			ThrowErrorIfNeed(handledResults);

			return new PolicyDelegateCollectionResult<T>(handledResults, _policyDelegates.Skip(handledResults.Count()).ToList());
			void ThrowErrorIfNeed(IEnumerable<PolicyDelegateResult<T>> hResults)
			{
				if (isFailed == true && _policyDelegates.ThrowOnLastFailed)
				{
					throw _policyDelegates.ErrorConverter.ToExceptionConverter()(hResults);
				}
			}
		}
	}
}
