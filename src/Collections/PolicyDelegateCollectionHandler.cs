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
			_policyDelegates = policyDelegates;
		}

		public PolicyDelegateCollectionResult Handle(CancellationToken token = default)
		{
			var (HandleResults, PolResult) = PolicyDelegatesHandler.HandleAllSync(_policyDelegates, token);
			return GetResultOrThrow(HandleResults, PolResult.IsFailed);
		}

		public async Task<PolicyDelegateCollectionResult> HandleAsync(bool configAwait = false, CancellationToken token = default)
		{
			PolicyDelegateHandleType handleType = _policyDelegates.GetHandleType();
			var (HandleResults, PolResult) = await PolicyDelegatesHandler.HandleAllBySyncType(_policyDelegates, handleType, token, configAwait).ConfigureAwait(configAwait);

			return GetResultOrThrow(HandleResults, PolResult.IsFailed);
		}

		private PolicyDelegateCollectionResult GetResultOrThrow(IEnumerable<PolicyDelegateResult> handledResults, bool? isFailed)
		{
			ThrowErrorIfNeed(handledResults);

			return new PolicyDelegateCollectionResult(handledResults, _policyDelegates.Skip(handledResults.Count()).ToList());
			void ThrowErrorIfNeed(IEnumerable<PolicyDelegateResult> hResults)
			{
				if (isFailed == true && _policyDelegates.ThrowOnLastFailed)
				{
					throw _policyDelegates.ErrorConverter.ToExceptionConverter()(hResults);
				}
			}
		}
	}
}
