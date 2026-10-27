using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Represents the handle of a deferred (queued) retry session with a result value started by <see cref="RetryPolicy"/>'s
	/// <c>HandleDeferredAsync</c> method.
	/// </summary>
	/// <typeparam name="T">Type of result.</typeparam>
	public sealed class QueuedRetryHandle<T>
	{
		private readonly TaskCompletionSource<PolicyResult<T>> _completion =
			new TaskCompletionSource<PolicyResult<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

		private readonly RetrySessionBase _session;

		internal QueuedRetryHandle(RetrySessionBase session)
		{
			_session = session;
		}

		/// <summary>
		/// Gets the task that completes with the terminal <see cref="PolicyResult{T}"/> of the deferred retry session.
		/// <see cref="PoliNorError.PolicyResult"/> handlers of the policy are guaranteed to run exactly once before this task resolves.
		/// </summary>
		public Task<PolicyResult<T>> Completion => _completion.Task;

		/// <summary>
		/// Requests cancellation of the deferred retry session: a pending (sleeping) session is canceled before its next attempt,
		/// an in-flight attempt observes the cancellation through its <see cref="System.Threading.CancellationToken"/> argument.
		/// Calling this method more than once or after the session has completed has no effect.
		/// </summary>
		public void Cancel()
		{
			_session?.CancelSession();
		}

		internal void Complete(PolicyResult<T> result) => _completion.TrySetResult(result);
	}
}