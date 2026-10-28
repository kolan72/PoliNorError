using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Runs deferred (queued) retry sessions with a <see cref="RetryPolicy"/> and a <see cref="RetryQueue"/>:
	/// attempt 1 executes in-frame, the remaining attempts and inter-attempt delays run on the queue's worker.
	/// </summary>
	public sealed class DeferredRetry
	{
		private readonly RetryPolicy _policy;
		private readonly RetryQueue _queue;

		/// <summary>
		/// Initializes a new deferred retry runner over the given policy and queue.
		/// </summary>
		/// <param name="policy">The policy that runs attempt 1 in-frame, produces the terminal <see cref="PolicyResult"/>
		/// and runs that result's handlers exactly once.</param>
		/// <param name="queue">The queue that executes the deferred part of the session. When its capacity is reached,
		/// the default <see cref="RetryQueueOverflowPolicy.Wait"/> blocks the corresponding <c>StartAsync</c> call until a slot frees up
		/// (cancellable through that call's cancellation token); with <see cref="RetryQueueOverflowPolicy.Reject"/> — or when the queue has already been completed
		/// or canceled — a <see cref="RetryQueueRejectedException"/> is thrown by <c>StartAsync</c> (attempt 1 may have already executed).</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="queue"/> is null.</exception>
		public DeferredRetry(RetryPolicy policy, RetryQueue queue)
		{
			_policy = policy ?? throw new ArgumentNullException(nameof(policy));
			_queue = queue ?? throw new ArgumentNullException(nameof(queue));
		}

		/// <summary>
		/// Starts a deferred retry session: attempt 1 runs synchronously inside this call, an attempt that fails retryably is
		/// not slept on but handed over to the <see cref="RetryQueue"/>, which performs the remaining attempts and the
		/// inter-attempt delays on its own worker.
		/// </summary>
		/// <remarks>
		/// <para>The returned <see cref="QueuedRetryHandle"/> exposes <see cref="QueuedRetryHandle.Completion"/>, which resolves with the terminal
		/// <see cref="PolicyResult"/> exactly as <see cref="RetryPolicy.HandleAsync(Func{CancellationToken, Task}, bool, CancellationToken)"/> would have returned it.
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
		/// <param name="configureAwait">The value used for all <c>ConfigureAwait</c> calls of this call and of the queued session.</param>
		/// <param name="token">The caller's cancellation token: it cancels attempt 1, any in-flight attempt and a session that is
		/// still waiting between attempts.</param>
		/// <returns>The handle whose <see cref="QueuedRetryHandle.Completion"/> resolves with the terminal <see cref="PolicyResult"/>.</returns>
		/// <exception cref="NotImplementedException">Thrown when the policy uses a custom <see cref="IRetryProcessor"/>.</exception>
		/// <exception cref="RetryQueueRejectedException">Thrown when the queue refuses the session (capacity with the
		/// <see cref="RetryQueueOverflowPolicy.Reject"/> policy, or the queue is already completed/canceled).</exception>
		/// <exception cref="OperationCanceledException">Thrown when <paramref name="token"/> is canceled while this call waits
		/// for a capacity slot; no session is queued in that case.</exception>
		public async Task<QueuedRetryHandle> StartAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			_policy.ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			var (Fn, Wrapper) = _policy.WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				var noDelegateHandle = new QueuedRetryHandle(null);
				noDelegateHandle.Complete(new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(_policy));
				return noDelegateHandle;
			}
			if (Fn == null)
			{
				var noDelegateResult = new PolicyResult().WithNoDelegateException().SetPolicyName(_policy.PolicyName);
				await _policy.HandlePolicyResultAsync(noDelegateResult, configureAwait, token).ConfigureAwait(configureAwait);
				var noDelegateHandle = new QueuedRetryHandle(null);
				noDelegateHandle.Complete(noDelegateResult);
				return noDelegateHandle;
			}

			var session = RetrySession.Create(Fn, processor, _policy.RetryInfo, _policy.Delay, DefaultRetryProcessor.RetryErrorContextCreator, configureAwait, token, true);
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
				await _queue.EnqueueAsync(session, readyTime, session.Token).ConfigureAwait(configureAwait);
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
					session.Result.SetWrappedPolicyResults(Wrapper).SetPolicyName(_policy.PolicyName);
					await _policy.HandlePolicyResultAsync(session.Result, configureAwait, token).ConfigureAwait(configureAwait);
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
		/// retryably is not slept on but handed over to the <see cref="RetryQueue"/>, which performs the remaining attempts and
		/// the inter-attempt delays on its own worker.
		/// </summary>
		/// <remarks>
		/// <para>The returned <see cref="QueuedRetryHandle{T}"/> exposes <see cref="QueuedRetryHandle{T}.Completion"/>, which resolves with the
		/// terminal <see cref="PolicyResult{T}"/> exactly as <see cref="RetryPolicy.HandleAsync{T}(Func{CancellationToken, Task{T}}, bool, CancellationToken)"/>
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
		/// <param name="configureAwait">The value used for all <c>ConfigureAwait</c> calls of this call and of the queued session.</param>
		/// <param name="token">The caller's cancellation token: it cancels attempt 1, any in-flight attempt and a session that is
		/// still waiting between attempts.</param>
		/// <returns>The handle whose <see cref="QueuedRetryHandle{T}.Completion"/> resolves with the terminal <see cref="PolicyResult{T}"/>.</returns>
		/// <exception cref="NotImplementedException">Thrown when the policy uses a custom <see cref="IRetryProcessor"/>.</exception>
		/// <exception cref="RetryQueueRejectedException">Thrown when the queue refuses the session (capacity with the
		/// <see cref="RetryQueueOverflowPolicy.Reject"/> policy, or the queue is already completed/canceled).</exception>
		/// <exception cref="OperationCanceledException">Thrown when <paramref name="token"/> is canceled while this call waits
		/// for a capacity slot; no session is queued in that case.</exception>
		public async Task<QueuedRetryHandle<T>> StartAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			_policy.ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor processor);

			var (Fn, Wrapper) = _policy.WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				var noDelegateHandle = new QueuedRetryHandle<T>(null);
				noDelegateHandle.Complete(new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(_policy));
				return noDelegateHandle;
			}
			if (Fn == null)
			{
				var noDelegateResult = new PolicyResult<T>().WithNoDelegateException().SetPolicyName(_policy.PolicyName);
				await _policy.HandlePolicyResultAsync(noDelegateResult, configureAwait, token).ConfigureAwait(configureAwait);
				var noDelegateHandle = new QueuedRetryHandle<T>(null);
				noDelegateHandle.Complete(noDelegateResult);
				return noDelegateHandle;
			}

			var session = RetrySession<T>.Create(Fn, processor, _policy.RetryInfo, _policy.Delay, DefaultRetryProcessor.RetryErrorContextCreator, configureAwait, token, true);
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
				await _queue.EnqueueAsync(session, readyTime, session.Token).ConfigureAwait(configureAwait);
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
					session.Result.SetWrappedPolicyResults(Wrapper).SetPolicyName(_policy.PolicyName);
					await _policy.HandlePolicyResultAsync(session.Result, configureAwait, token).ConfigureAwait(configureAwait);
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