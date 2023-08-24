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
}
