using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyDelegateCollectionHandling
	{
		public static PolicyDelegateCollectionResult<T> HandleAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().Handle(token);
		}

		public static Task<PolicyDelegateCollectionResult<T>> HandleAllAsync<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, CancellationToken token) => HandleAllAsync(policyDelegateCollection, false, token);

		public static Task<PolicyDelegateCollectionResult<T>> HandleAllAsync<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, bool configAwait = false, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().HandleAsync(configAwait, token);
		}
	}
}
