using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class DeferredRetryTests
	{
		private static Func<CancellationToken, Task> TransientThenSuccess(int failTimes)
		{
			var calls = 0;
			return async _ =>
			{
				calls++;
				await Task.Delay(1);
				if (calls <= failTimes)
				{
					throw new InvalidOperationException($"boom {calls}");
				}
			};
		}

		private static Func<CancellationToken, Task<int>> TransientThenResult(int failTimes, int result)
		{
			var calls = 0;
			return async _ =>
			{
				calls++;
				await Task.Delay(1);
				if (calls <= failTimes)
				{
					throw new InvalidOperationException($"boom {calls}");
				}
				return result;
			};
		}

		[Test]
		public void Ctor_WithNullPolicyOrNullQueue_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new DeferredRetry(null, new RetryQueue()));
			Assert.Throws<ArgumentNullException>(() => new DeferredRetry(new RetryPolicy(1), null));
		}

		[Test]
		public async Task Should_StartAsync_Match_HandleAsync_TerminalStateAndHandlers()
		{
			var policyOfHandleAsync = new RetryPolicy(2);
			var handlersOfHandleAsync = 0;
			policyOfHandleAsync.AddPolicyResultHandler(_ => handlersOfHandleAsync++);
			var handleAsyncResult = await policyOfHandleAsync.HandleAsync(TransientThenSuccess(1));

			var policyOfDeferred = new RetryPolicy(2);
			var handlersOfDeferred = 0;
			policyOfDeferred.AddPolicyResultHandler(_ => handlersOfDeferred++);
			var queue = new RetryQueue();
			var handle = await new DeferredRetry(policyOfDeferred, queue).StartAsync(TransientThenSuccess(1));
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.AreEqual(handleAsyncResult.IsFailed, deferredResult.IsFailed);
			ClassicAssert.AreEqual(handleAsyncResult.IsCanceled, deferredResult.IsCanceled);
			ClassicAssert.AreEqual(handleAsyncResult.IsSuccess, deferredResult.IsSuccess);
			ClassicAssert.AreEqual(handleAsyncResult.NoError, deferredResult.NoError);
			ClassicAssert.AreEqual(handleAsyncResult.IsPolicySuccess, deferredResult.IsPolicySuccess);
			ClassicAssert.AreEqual(handleAsyncResult.PolicyName, deferredResult.PolicyName);
			ClassicAssert.AreEqual(handleAsyncResult.Errors.Count(), deferredResult.Errors.Count());
			ClassicAssert.AreEqual(handleAsyncResult.Errors.First().GetType(), deferredResult.Errors.First().GetType());
			ClassicAssert.AreEqual(1, handlersOfHandleAsync);
			ClassicAssert.AreEqual(1, handlersOfDeferred);

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
			ClassicAssert.AreEqual(0, queue.PendingCount);
		}

		[Test]
		public async Task Should_StartAsyncT_Match_HandleAsyncT_TerminalStateAndResultValue()
		{
			var policyOfHandleAsync = new RetryPolicy(2);
			var handleAsyncResult = await policyOfHandleAsync.HandleAsync(TransientThenResult(1, 42));

			var policyOfDeferred = new RetryPolicy(2);
			var queue = new RetryQueue();
			var handle = await new DeferredRetry(policyOfDeferred, queue).StartAsync(TransientThenResult(1, 42));
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.AreEqual(handleAsyncResult.Result, deferredResult.Result);
			ClassicAssert.AreEqual(handleAsyncResult.IsFailed, deferredResult.IsFailed);
			ClassicAssert.AreEqual(handleAsyncResult.IsSuccess, deferredResult.IsSuccess);
			ClassicAssert.AreEqual(handleAsyncResult.NoError, deferredResult.NoError);
			ClassicAssert.AreEqual(handleAsyncResult.IsPolicySuccess, deferredResult.IsPolicySuccess);
			ClassicAssert.AreEqual(handleAsyncResult.PolicyName, deferredResult.PolicyName);
			ClassicAssert.AreEqual(handleAsyncResult.Errors.Count(), deferredResult.Errors.Count());

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
		}

		[Test]
		public async Task Should_StartAsync_WithoutAwaitingCompletion_StillRunHandlersExactlyOnce()
		{
			var policy = new RetryPolicy(2);
			var handled = 0;
			var handlerSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			policy.AddPolicyResultHandler(_ =>
			{
				if (Interlocked.Increment(ref handled) == 1)
				{
					handlerSignal.TrySetResult(true);
				}
			});

			var handle = await new DeferredRetry(policy, new RetryQueue()).StartAsync(TransientThenSuccess(1));

			await DeferredRetryTestHelpers.Complete(handlerSignal.Task);
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.AreEqual(1, handled);
			ClassicAssert.IsTrue(deferredResult.IsPolicySuccess);
			ClassicAssert.AreEqual(1, handle.Completion.Result.Errors.Count());
		}

		[Test]
		public async Task Should_StartAsync_SetFailedInsideHandler_BeVisibleInCompletion()
		{
			var policy = new RetryPolicy(1);
			var handled = 0;
			policy.AddPolicyResultHandler(r =>
			{
				handled++;
				r.SetFailed();
			});

			var handle = await new DeferredRetry(policy, new RetryQueue())
				.StartAsync(async _ => await Task.Yield());
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.AreEqual(1, handled);
			ClassicAssert.IsTrue(deferredResult.IsFailed);
		}

		[Test]
		public async Task Should_StartAsync_CanceledEarly_MatchHandleAsync()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var callsOfHandleAsync = 0;
				Func<CancellationToken, Task> func = async _ =>
				{
					callsOfHandleAsync++;
					await Task.Yield();
				};
				var policyOfHandleAsync = new RetryPolicy(2);
				var handleAsyncResult = await policyOfHandleAsync.HandleAsync(func, false, cts.Token);

				var callsOfDeferred = 0;
				Func<CancellationToken, Task> funcOfDeferred = async _ =>
				{
					callsOfDeferred++;
					await Task.Yield();
				};
				var policyOfDeferred = new RetryPolicy(2);
				var queue = new RetryQueue();
				var handle = await new DeferredRetry(policyOfDeferred, queue).StartAsync(funcOfDeferred, false, cts.Token);
				var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

				ClassicAssert.AreEqual(0, callsOfHandleAsync);
				ClassicAssert.AreEqual(0, callsOfDeferred);
				ClassicAssert.IsTrue(handleAsyncResult.IsCanceled);
				ClassicAssert.IsTrue(handleAsyncResult.NoError);
				ClassicAssert.IsFalse(handleAsyncResult.IsFailed);
				ClassicAssert.AreEqual(handleAsyncResult.IsCanceled, deferredResult.IsCanceled);
				ClassicAssert.AreEqual(handleAsyncResult.NoError, deferredResult.NoError);
				ClassicAssert.AreEqual(handleAsyncResult.IsFailed, deferredResult.IsFailed);
				ClassicAssert.AreEqual(0, queue.PendingCount);
			}
		}

		[Test]
		public async Task Should_StartAsync_CanceledDuringAttempt1_ProduceFailedAndCanceledResult()
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(2);
				var queue = new RetryQueue();

				async Task func(CancellationToken token)
				{
					cts.Cancel();
					await Task.Delay(1, token);
				}

				var handle = await new DeferredRetry(policy, queue).StartAsync(func, false, cts.Token);
				var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

				ClassicAssert.IsTrue(deferredResult.IsFailed);
				ClassicAssert.IsTrue(deferredResult.IsCanceled);
				ClassicAssert.IsNotNull(deferredResult.PolicyCanceledError);
				ClassicAssert.AreEqual(0, queue.PendingCount);
			}
		}

		[Test]
		public async Task Should_StartAsync_CanceledDuringReadyWait_ProduceFailedAndCanceledResult()
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(2, retryDelay: new ConstantRetryDelay(TimeSpan.FromSeconds(30)));
				var queue = new RetryQueue();
				var calls = 0;

				async Task func(CancellationToken _)
				{
					calls++;
					await Task.Delay(1);
					throw new InvalidOperationException("boom");
				}

				var handle = await new DeferredRetry(policy, queue).StartAsync(func, false, cts.Token);
				cts.Cancel();

				var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

				ClassicAssert.AreEqual(1, calls);
				ClassicAssert.IsTrue(deferredResult.IsFailed);
				ClassicAssert.IsTrue(deferredResult.IsCanceled);
				ClassicAssert.IsNotNull(deferredResult.PolicyCanceledError);
				ClassicAssert.AreEqual(0, queue.PendingCount);

				await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
			}
		}

		[Test]
		public async Task Should_StartAsync_CanceledDuringAttempt2_ProduceFailedAndCanceledResult()
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(2, retryDelay: new ConstantRetryDelay(TimeSpan.FromMilliseconds(30)));
				var queue = new RetryQueue();
				var attempt2Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				var calls = 0;

				async Task func(CancellationToken token)
				{
					var callNumber = Interlocked.Increment(ref calls);
					await Task.Delay(1);
					if (callNumber == 1)
					{
						throw new InvalidOperationException("boom");
					}
					attempt2Started.TrySetResult(true);
					await Task.Delay(Timeout.Infinite, token);
				}

				var handle = await new DeferredRetry(policy, queue).StartAsync(func, false, cts.Token);
				await DeferredRetryTestHelpers.Complete(attempt2Started.Task);
				cts.Cancel();

				var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

				ClassicAssert.GreaterOrEqual(Volatile.Read(ref calls), 2);
				ClassicAssert.IsTrue(deferredResult.IsFailed);
				ClassicAssert.IsTrue(deferredResult.IsCanceled);
				ClassicAssert.IsNotNull(deferredResult.PolicyCanceledError);
				ClassicAssert.AreEqual(0, queue.PendingCount);

				await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
			}
		}

		[Test]
		public async Task Should_HandleCancel_ResolveSessionAsFailedAndCanceled()
		{
			var policy = new RetryPolicy(2, retryDelay: new ConstantRetryDelay(TimeSpan.FromSeconds(30)));
			var queue = new RetryQueue();
			var calls = 0;

			async Task func(CancellationToken _)
			{
				calls++;
				await Task.Delay(1);
				throw new InvalidOperationException("boom");
			}

			var handle = await new DeferredRetry(policy, queue).StartAsync(func);
			handle.Cancel();

			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.AreEqual(1, calls);
			ClassicAssert.IsTrue(deferredResult.IsFailed);
			ClassicAssert.IsTrue(deferredResult.IsCanceled);
			ClassicAssert.IsNotNull(deferredResult.PolicyCanceledError);
			ClassicAssert.AreEqual(0, queue.PendingCount);

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
		}

		[Test]
		public async Task Should_CancelAsync_ResolveDeferredSessionAsFailedAndCanceled()
		{
			var policy = new RetryPolicy(2, retryDelay: new ConstantRetryDelay(TimeSpan.FromSeconds(30)));
			var queue = new RetryQueue();
			var calls = 0;

			async Task func(CancellationToken _)
			{
				calls++;
				await Task.Delay(1);
				throw new InvalidOperationException("boom");
			}

			var handle = await new DeferredRetry(policy, queue).StartAsync(func);
			await DeferredRetryTestHelpers.Complete(queue.CancelAsync());
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.AreEqual(1, calls);
			ClassicAssert.IsTrue(deferredResult.IsFailed);
			ClassicAssert.IsTrue(deferredResult.IsCanceled);
			ClassicAssert.IsNotNull(deferredResult.PolicyCanceledError);
			ClassicAssert.AreEqual(0, queue.PendingCount);
		}

		[Test]
		public async Task Should_StartAsync_WithWrappedPolicy_MatchHandleAsync()
		{
			var policyOfHandleAsync = new RetryPolicy(2);
			new SimplePolicy().Then(policyOfHandleAsync);
			var handleAsyncResult = await policyOfHandleAsync.HandleAsync(TransientThenSuccess(1));

			var policyOfDeferred = new RetryPolicy(2);
			new SimplePolicy().Then(policyOfDeferred);
			var queue = new RetryQueue();
			var handle = await new DeferredRetry(policyOfDeferred, queue).StartAsync(TransientThenSuccess(1));
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.IsNotNull(handleAsyncResult.WrappedPolicyResults);
			ClassicAssert.IsNotNull(deferredResult.WrappedPolicyResults);
			ClassicAssert.AreEqual(
				handleAsyncResult.WrappedPolicyResults.Count(),
				deferredResult.WrappedPolicyResults.Count());
			ClassicAssert.Greater(deferredResult.WrappedPolicyResults.Count(), 0);
			ClassicAssert.AreEqual(handleAsyncResult.IsPolicySuccess, deferredResult.IsPolicySuccess);
			ClassicAssert.AreEqual(handleAsyncResult.IsFailed, deferredResult.IsFailed);

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
		}

		[Test]
		public void Should_StartAsync_Throw_ForCustomRetryProcessor()
		{
			var subsProcessor = Substitute.For<IRetryProcessor>();
			var policy = new RetryPolicy(subsProcessor, 1);
			var queue = new RetryQueue();
			var calls = 0;

			Func<CancellationToken, Task> func = _ =>
			{
				calls++;
				return Task.CompletedTask;
			};

			Assert.ThrowsAsync<NotImplementedException>(async () => await new DeferredRetry(policy, queue).StartAsync(func));
			ClassicAssert.AreEqual(0, calls);
		}

		[Test]
		public async Task Should_StartAsync_NullDelegate_MatchHandleAsync()
		{
			var policyOfHandleAsync = new RetryPolicy(1);
			var handlersOfHandleAsync = 0;
			policyOfHandleAsync.AddPolicyResultHandler(_ => handlersOfHandleAsync++);
			var handleAsyncResult = await policyOfHandleAsync.HandleAsync((Func<CancellationToken, Task>)null);

			var policyOfDeferred = new RetryPolicy(1);
			var handlersOfDeferred = 0;
			policyOfDeferred.AddPolicyResultHandler(_ => handlersOfDeferred++);
			var handle = await new DeferredRetry(policyOfDeferred, new RetryQueue()).StartAsync((Func<CancellationToken, Task>)null);
			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);

			ClassicAssert.IsTrue(handleAsyncResult.IsFailed);
			ClassicAssert.IsTrue(deferredResult.IsFailed);
			ClassicAssert.AreEqual(handleAsyncResult.FailedReason, deferredResult.FailedReason);
			ClassicAssert.AreEqual(handleAsyncResult.PolicyName, deferredResult.PolicyName);
			ClassicAssert.AreEqual(1, handlersOfHandleAsync);
			ClassicAssert.AreEqual(1, handlersOfDeferred);
		}

		[Test]
		public async Task Should_StartAsync_WithWithWait_SleepInlineDuringAttempt1()
		{
			var policy = new RetryPolicy(2).WithWait(TimeSpan.FromMilliseconds(15));
			var queue = new RetryQueue();

			var stopwatch = Stopwatch.StartNew();
			var handle = await new DeferredRetry(policy, queue).StartAsync(TransientThenSuccess(1));
			stopwatch.Stop();

			ClassicAssert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, 10);

			var deferredResult = await DeferredRetryTestHelpers.Complete(handle.Completion);
			ClassicAssert.IsTrue(deferredResult.IsPolicySuccess);
			ClassicAssert.AreEqual(1, deferredResult.Errors.Count());

			await DeferredRetryTestHelpers.Complete(queue.CompleteAsync());
		}

		[Test]
		public async Task Should_StartAsync_Throw_WhenQueueRejectsWithRejectPolicy()
		{
			var queue = new RetryQueue(new RetryQueueOptions { Capacity = 1, Overflow = RetryQueueOverflowPolicy.Reject });
			var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var attempt2Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var calls = 0;

			async Task func(CancellationToken token)
			{
				var callNumber = Interlocked.Increment(ref calls);
				await Task.Delay(1);
				if (callNumber == 2)
				{
					attempt2Started.TrySetResult(true);
					var cancellableDelay = Task.Delay(Timeout.Infinite, token);
					var winner = await Task.WhenAny(gate.Task, cancellableDelay);
					if (winner != gate.Task)
					{
						token.ThrowIfCancellationRequested();
					}
					return;
				}
				throw new InvalidOperationException($"boom {callNumber}");
			}

			var firstPolicy = new RetryPolicy(2);
			var firstHandle = await new DeferredRetry(firstPolicy, queue).StartAsync(func);
			await DeferredRetryTestHelpers.Complete(attempt2Started.Task);

			var secondPolicy = new RetryPolicy(2);
			Assert.ThrowsAsync<RetryQueueRejectedException>(async () => await new DeferredRetry(secondPolicy, queue).StartAsync(func));

			await DeferredRetryTestHelpers.Complete(queue.CancelAsync());
			var firstResult = await DeferredRetryTestHelpers.Complete(firstHandle.Completion);
			ClassicAssert.IsTrue(firstResult.IsCanceled);
			ClassicAssert.AreEqual(0, queue.PendingCount);
		}
	}
}