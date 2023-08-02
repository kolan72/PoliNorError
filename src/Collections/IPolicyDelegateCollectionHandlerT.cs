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

	public static class IPolicyDelegateCollectionHandlerTExtensions
	{
		public static Task<PolicyDelegateCollectionResult<T>> HandleAsync<T>(this IPolicyDelegateCollectionHandler<T> policyDelegateCollectionHandler, CancellationToken token = default)
		{
			return policyDelegateCollectionHandler.HandleAsync(false, token);
		}
	}
}
