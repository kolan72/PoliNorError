using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class RetryPolicy
	{
		/// <summary>
		/// Starts a deferred retry session: attempt 1 runs synchronously inside this call, an attempt that fails retryably is
		/// not slept on but handed over to the <paramref name="retryQueue"/>, which performs the remaining attempts and the
		/// inter-attempt delays on its own worker.
		/// </summary>
		/// <remarks>
		/// <para>The returned <see cref="QueuedRetryHandle"/> exposes <see cref="QueuedRetryHandle.Completion"/>, which resolves with the terminal
		/// <see cref="PolicyResult"/> exactly as <see cref="HandleAsync(Func{CancellationToken, Task}, bool, CancellationToken)"/> would have returned it.
		/// <see cref="PoliNorError.PolicyResult"/> handlers of this policy (including <c>SetFailed</c>-style ones) run exactly once, before
		/// <see cref="QueuedRetryHandle.Completion"/> resolves; they still run when the handle is never awaited by the caller.</para>
		/// <para>Because attempt 1 executes in-frame, this method returns after attempt 1 and its error processing,
		/// not before the retry session is finished. If attempt 1 succeeds or reaches a terminal state, nothing is enqueued and the
		/// returned handle is already completed.</para>
		/// <para>This method is asynchronous only: the deferred mode is never available on the synchronous <c>Handle</c> methods,
		/// and it requires the <see cref="DefaultRetryProcessor"/> (a custom <see cref="IRetryProcessor"/> fails with
		/// <see cref="NotImplementedException"/>).</para>
		/// <para>The session carries no captured <see cref="ExecutionContext"/> into the queue worker: the queued attempts run on the worker's
		/// execution context. The delegate should therefore be self-contained (no scoped/DI dependencies captured by the closure),
		/// idempotent when it can be invoked more than once, and its callbacks must be thread-safe: attempt 1 runs on the calling context
		/// while later attempts run on the queue worker.</para>
		/// <para><c>WithWait(...)</c> (<see cref="DelayErrorProcessor"/>) sleeps inline while processing an attempt's error: attempt 1's sleep
		/// always happens before the handle exists. Prefer <see cref="RetryDelay"/> for deferred sessions.</para>
		/// </remarks>
		/// <param name="func">The delegate to run. Its <see cref="CancellationToken"/> argument is linked to the caller's
		/// <paramref name="token"/> and to <see cref="QueuedRetryHandle.Cancel"/>.</param>
		/// <param name="retryQueue">The queue that executes the deferred part of the session. When its capacity is reached,
		/// the default <see cref="RetryQueueOverflowPolicy.Wait"/> blocks this call until a slot frees up (cancellable through
		/// <paramref name="token"/>); with <see cref="RetryQueueOverflowPolicy.Reject"/> — or when the queue has already been completed
		/// or canceled — a <see cref="RetryQueueRejectedException"/> is thrown (attempt 1 may have already executed).</param>
		/// <param name="configureAwait">The value used for all <c>ConfigureAwait</c> calls of this call and of the queued session.</param>
		/// <param name="token">The caller's cancellation token: it cancels attempt 1, any in-flight attempt and a session that is
		/// still waiting between attempts.</param>
		/// <returns>The handle whose <see cref="QueuedRetryHandle.Completion"/> resolves with the terminal <see cref="PolicyResult"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="retryQueue"/> is null.</exception>
		/// <exception cref="NotImplementedException">Thrown when the policy uses a custom <see cref="IRetryProcessor"/>.</exception>
		/// <exception cref="RetryQueueRejectedException">Thrown when the queue refuses the session (capacity with the
		/// <see cref="RetryQueueOverflowPolicy.Reject"/> policy, or the queue is already completed/canceled).</exception>
		/// <exception cref="OperationCanceledException">Thrown when <paramref name="token"/> is canceled while this call waits
		/// for a capacity slot; no session is queued in that case.</exception>
		public async Task<QueuedRetryHandle> HandleDeferredAsync(Func<CancellationToken, Task> func, RetryQueue retryQueue, bool configureAwait = false, CancellationToken token = default)
		{
			if (retryQueue == null)
				throw new ArgumentNullException(nameof(retryQueue));

			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				var noDelegateHandle = new QueuedRetryHandle(null);
				noDelegateHandle.Complete(new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this));
				return noDelegateHandle;
			}
			if (Fn == null)
			{
				var noDelegateResult = new PolicyResult().WithNoDelegateException().SetPolicyName(PolicyName);
				await HandlePolicyResultAsync(noDelegateResult, configureAwait, token).ConfigureAwait(configureAwait);
				var noDelegateHandle = new QueuedRetryHandle(null);
				noDelegateHandle.Complete(noDelegateResult);
				return noDelegateHandle;
			}

			var session = RetrySession.Create(Fn, processor, RetryInfo, Delay, DefaultRetryProcessor.RetryErrorContextCreator, configureAwait, token, true);
			var handle = new QueuedRetryHandle(session);
			session.TerminalHandler = FinalizeSessionAsync;

			var outcome = await session.StepAsync(configureAwait, session.Token).ConfigureAwait(configureAwait);
			if (outcome.IsTerminal)
			{
				await session.RunTerminalHandlerAsync().ConfigureAwait(configureAwait);
				session.DisposeCancellation();
				return handle;
			}

			var readyTime = RetryQueue.GetReadyTimeUtc(outcome.Delay);
			try
			{
				await retryQueue.EnqueueAsync(session, readyTime, session.Token).ConfigureAwait(configureAwait);
			}
			catch
			{
				session.DisposeCancellation();
				throw;
			}
			return handle;

			async Task FinalizeSessionAsync()
			{
				try
				{
					session.Result.SetWrappedPolicyResults(Wrapper).SetPolicyName(PolicyName);
					await HandlePolicyResultAsync(session.Result, configureAwait, token).ConfigureAwait(configureAwait);
				}
				catch (Exception ex)
				{
					session.Result.SetFailedWithCatchBlockError(ex, null, CatchBlockExceptionSource.RetryQueueWorker);
				}
				finally
				{
					handle.Complete(session.Result);
				}
			}
		}

		/// <summary>
		/// Starts a deferred retry session with a result value: attempt 1 runs synchronously inside this call, an attempt that fails
		/// retryably is not slept on but handed over to the <paramref name="retryQueue"/>, which performs the remaining attempts and
		/// the inter-attempt delays on its own worker.
		/// </summary>
		/// <remarks>
		/// <para>The returned <see cref="QueuedRetryHandle{T}"/> exposes <see cref="QueuedRetryHandle{T}.Completion"/>, which resolves with the
		/// terminal <see cref="PolicyResult{T}"/> exactly as <see cref="HandleAsync{T}(Func{CancellationToken, Task{T}}, bool, CancellationToken)"/>
		/// would have returned it. <see cref="PoliNorError.PolicyResult"/> handlers of this policy (including <c>SetFailed</c>-style ones) run
		/// exactly once, before <see cref="QueuedRetryHandle{T}.Completion"/> resolves; they still run when the handle is never awaited by the caller.</para>
		/// <para>Because attempt 1 executes in-frame, this method returns after attempt 1 and its error processing,
		/// not before the retry session is finished. If attempt 1 succeeds or reaches a terminal state, nothing is enqueued and the
		/// returned handle is already completed.</para>
		/// <para>This method is asynchronous only: the deferred mode is never available on the synchronous <c>Handle</c> methods,
		/// and it requires the <see cref="DefaultRetryProcessor"/> (a custom <see cref="IRetryProcessor"/> fails with
		/// <see cref="NotImplementedException"/>).</para>
		/// <para>The session carries no captured <see cref="ExecutionContext"/> into the queue worker: the queued attempts run on the worker's
		/// execution context. The delegate should therefore be self-contained (no scoped/DI dependencies captured by the closure),
		/// idempotent when it can be invoked more than once, and its callbacks must be thread-safe: attempt 1 runs on the calling context
		/// while later attempts run on the queue worker.</para>
		/// <para><c>WithWait(...)</c> (<see cref="DelayErrorProcessor"/>) sleeps inline while processing an attempt's error: attempt 1's sleep
		/// always happens before the handle exists. Prefer <see cref="RetryDelay"/> for deferred sessions.</para>
		/// </remarks>
		/// <typeparam name="T">Type of result.</typeparam>
		/// <param name="func">The delegate to run. Its <see cref="CancellationToken"/> argument is linked to the caller's
		/// <paramref name="token"/> and to <see cref="QueuedRetryHandle{T}.Cancel"/>.</param>
		/// <param name="retryQueue">The queue that executes the deferred part of the session. When its capacity is reached,
		/// the default <see cref="RetryQueueOverflowPolicy.Wait"/> blocks this call until a slot frees up (cancellable through
		/// <paramref name="token"/>); with <see cref="RetryQueueOverflowPolicy.Reject"/> — or when the queue has already been completed
		/// or canceled — a <see cref="RetryQueueRejectedException"/> is thrown (attempt 1 may have already executed).</param>
		/// <param name="configureAwait">The value used for all <c>ConfigureAwait</c> calls of this call and of the queued session.</param>
		/// <param name="token">The caller's cancellation token: it cancels attempt 1, any in-flight attempt and a session that is
		/// still waiting between attempts.</param>
		/// <returns>The handle whose <see cref="QueuedRetryHandle{T}.Completion"/> resolves with the terminal <see cref="PolicyResult{T}"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="retryQueue"/> is null.</exception>
		/// <exception cref="NotImplementedException">Thrown when the policy uses a custom <see cref="IRetryProcessor"/>.</exception>
		/// <exception cref="RetryQueueRejectedException">Thrown when the queue refuses the session (capacity with the
		/// <see cref="RetryQueueOverflowPolicy.Reject"/> policy, or the queue is already completed/canceled).</exception>
		/// <exception cref="OperationCanceledException">Thrown when <paramref name="token"/> is canceled while this call waits
		/// for a capacity slot; no session is queued in that case.</exception>
		public async Task<QueuedRetryHandle<T>> HandleDeferredAsync<T>(Func<CancellationToken, Task<T>> func, RetryQueue retryQueue, bool configureAwait = false, CancellationToken token = default)
		{
			if (retryQueue == null)
				throw new ArgumentNullException(nameof(retryQueue));

			ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				var noDelegateHandle = new QueuedRetryHandle<T>(null);
				noDelegateHandle.Complete(new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this));
				return noDelegateHandle;
			}
			if (Fn == null)
			{
				var noDelegateResult = new PolicyResult<T>().WithNoDelegateException().SetPolicyName(PolicyName);
				await HandlePolicyResultAsync(noDelegateResult, configureAwait, token).ConfigureAwait(configureAwait);
				var noDelegateHandle = new QueuedRetryHandle<T>(null);
				noDelegateHandle.Complete(noDelegateResult);
				return noDelegateHandle;
			}

			var session = RetrySession<T>.Create(Fn, processor, RetryInfo, Delay, DefaultRetryProcessor.RetryErrorContextCreator, configureAwait, token, true);
			var handle = new QueuedRetryHandle<T>(session);
			session.TerminalHandler = FinalizeSessionAsync;

			var outcome = await session.StepAsync(configureAwait, session.Token).ConfigureAwait(configureAwait);
			if (outcome.IsTerminal)
			{
				await session.RunTerminalHandlerAsync().ConfigureAwait(configureAwait);
				session.DisposeCancellation();
				return handle;
			}

			var readyTime = RetryQueue.GetReadyTimeUtc(outcome.Delay);
			try
			{
				await retryQueue.EnqueueAsync(session, readyTime, session.Token).ConfigureAwait(configureAwait);
			}
			catch
			{
				session.DisposeCancellation();
				throw;
			}
			return handle;

			async Task FinalizeSessionAsync()
			{
				try
				{
					session.Result.SetWrappedPolicyResults(Wrapper).SetPolicyName(PolicyName);
					await HandlePolicyResultAsync(session.Result, configureAwait, token).ConfigureAwait(configureAwait);
				}
				catch (Exception ex)
				{
					session.Result.SetFailedWithCatchBlockError(ex, null, CatchBlockExceptionSource.RetryQueueWorker);
				}
				finally
				{
					handle.Complete(session.Result);
				}
			}
		}
	}
}