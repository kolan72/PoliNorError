using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// An interface defining handling methods for the  PolicyDelegateCollection of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IPolicyDelegateCollectionHandler<T>
	{
		PolicyDelegateCollectionResult<T> Handle(CancellationToken token = default);
		Task<PolicyDelegateCollectionResult<T>> HandleAsync(bool configAwait = false, CancellationToken token = default);
	}
}
