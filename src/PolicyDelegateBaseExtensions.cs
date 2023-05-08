using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyDelegateBaseExtensions
	{
		public static bool IsNotNullAndWithoutDelegate(this PolicyDelegateBase delegateInfo)
		{
			return (delegateInfo != null) && Predicates.Not(PolicyDelegatePredicates.WithDelegateFunc)(delegateInfo);
		}

		public static bool IsNotNullAndWithDelegate(this PolicyDelegateBase delegateInfo)
		{
			return (delegateInfo != null) && PolicyDelegatePredicates.WithDelegateFunc(delegateInfo);
		}

		public static PolicyResult Handle(this PolicyDelegate delegateInfo, CancellationToken cancellationToken = default) => delegateInfo.Policy.Handle(delegateInfo.Execute, cancellationToken);

		public static PolicyResult<T> Handle<T>(this PolicyDelegate<T> delegateInfo, CancellationToken cancellationToken = default) => delegateInfo.Policy.Handle(delegateInfo.Execute, cancellationToken);

		public static Task<PolicyResult> HandleAsync(this PolicyDelegate delegateInfo, CancellationToken cancellationToken) => HandleAsync(delegateInfo, false, cancellationToken);

		public static Task<PolicyResult> HandleAsync(this PolicyDelegate delegateInfo, bool configureAwait, CancellationToken cancellationToken) => delegateInfo.Policy.HandleAsync(delegateInfo.ExecuteAsync, configureAwait, cancellationToken);

		public static Task<PolicyResult<T>> HandleAsync<T>(this PolicyDelegate<T> delegateInfo, CancellationToken cancellationToken) => HandleAsync(delegateInfo, false, cancellationToken);

		public static Task<PolicyResult<T>> HandleAsync<T>(this PolicyDelegate<T> delegateInfo, bool configureAwait, CancellationToken cancellationToken) => delegateInfo.Policy.HandleAsync(delegateInfo.ExecuteAsync, configureAwait, cancellationToken);
	}
}
