using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// An interface defining handling methods for the  PolicyDelegateCollection
	/// </summary>
	public interface IPolicyDelegateCollectionHandler
	{
		PolicyDelegateCollectionResult Handle(CancellationToken token = default);
		Task<PolicyDelegateCollectionResult> HandleAsync(bool configAwait = false, CancellationToken token = default);
	}

	public static class IPolicyDelegateCollectionHandlerExtensions
	{
		public static Task<PolicyDelegateCollectionResult> HandleAsync(this IPolicyDelegateCollectionHandler policyDelegateCollectionHandler, CancellationToken token = default)
		{
			return policyDelegateCollectionHandler.HandleAsync(false, token);
		}
	}
}
