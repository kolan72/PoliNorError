using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class PolicyCollection
	{
		public PolicyDelegateCollectionResult HandleDelegate(Action action, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(action).Handle(token);
		}

		public PolicyDelegateCollectionResult HandleDelegate(Func<CancellationToken, Task> func, CancellationToken token = default)
		{
			return  BuildCollectionHandlerFor(func).Handle(token);
		}

		public PolicyDelegateCollectionResult<T> HandleDelegate<T>(Func<T> action, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(action).Handle(token);
		}

		public PolicyDelegateCollectionResult<T> HandleDelegate<T>(Func<CancellationToken, Task<T>> func, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(func).Handle(token);
		}

		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Action action, CancellationToken token) => HandleDelegateAsync(action, false, token);

		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Action action, bool configAwait = false, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(action).HandleAsync(configAwait, token);
		}

		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Func<CancellationToken, Task> func, CancellationToken token) => HandleDelegateAsync(func, false, token);

		public Task<PolicyDelegateCollectionResult> HandleDelegateAsync(Func<CancellationToken, Task> func, bool configAwait = false, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<T> func, CancellationToken token) => HandleDelegateAsync(func, false, token);

		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<T> func, bool configAwait = false, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}

		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken token) => HandleDelegateAsync(func, false, token);

		public Task<PolicyDelegateCollectionResult<T>> HandleDelegateAsync<T>(Func<CancellationToken, Task<T>> func, bool configAwait = false, CancellationToken token = default)
		{
			return BuildCollectionHandlerFor(func).HandleAsync(configAwait, token);
		}
	}
}
