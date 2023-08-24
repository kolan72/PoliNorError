using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of static methods to handle all <see cref="PolicyDelegate"/>s  or <see cref="PolicyDelegate{T}"/>s of <see cref="IPolicyDelegateCollection"/> or <see cref="IPolicyDelegateCollection{T}"/>.
	/// </summary>
	public static partial class PolicyDelegateCollectionHandling
	{
		public static PolicyDelegateCollectionResult HandleAll(this IPolicyDelegateCollection policyDelegateCollection, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().Handle(token);
		}

		public static Task<PolicyDelegateCollectionResult> HandleAllAsync(this IPolicyDelegateCollection policyDelegateCollection, CancellationToken token) => HandleAllAsync(policyDelegateCollection, false, token);

		public static Task<PolicyDelegateCollectionResult> HandleAllAsync(this IPolicyDelegateCollection policyDelegateCollection, bool configAwait = false, CancellationToken token = default)
		{
			return policyDelegateCollection.BuildCollectionHandler().HandleAsync(configAwait, token);
		}
	}
}
