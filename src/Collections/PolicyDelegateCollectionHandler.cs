using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class PolicyDelegateCollectionHandler : IPolicyDelegateCollectionHandler
	{
		private readonly PolicyDelegateCollection _policyDelegates;

		public PolicyDelegateCollectionHandler(PolicyDelegateCollection policyDelegates)
		{
			_policyDelegates = policyDelegates ?? throw new ArgumentNullException(nameof(policyDelegates));
		}

		public PolicyDelegateCollectionResult Handle(CancellationToken token = default)
		{
			var (HandleResults, PolResult) = PolicyDelegatesHandler.HandleAllSync(_policyDelegates, token);
			return GetResultOrThrow(HandleResults, PolResult);
		}

		public async Task<PolicyDelegateCollectionResult> HandleAsync(bool configAwait = false, CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = _policyDelegates.GetHandleType();
			var (HandleResults, PolResult) = await PolicyDelegatesHandler.HandleAllBySyncType(_policyDelegates, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, PolResult);
		}

		private PolicyDelegateCollectionResult GetResultOrThrow(IEnumerable<PolicyDelegateResult> handledResults, LastPolicyResultState resultState)
		{
			if (resultState.IsFailed == true && _policyDelegates.ThrowOnLastFailed)
			{
				throw _policyDelegates.ErrorConverter.ToExceptionConverter()(handledResults);
			}

			// Count() is O(1): FlexSyncEnumerable implements IReadOnlyCollection<T>.
			var unusedPolicyDelegates = _policyDelegates.Skip(handledResults.Count()).ToList();
			return new PolicyDelegateCollectionResult(handledResults, unusedPolicyDelegates, resultState);
		}
	}
}
