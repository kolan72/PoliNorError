using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal static class DeferredRetryTestHelpers
	{
		public const int DefaultTimeoutMs = 10000;

		public static async Task<T> Complete<T>(Task<T> task, int ms = DefaultTimeoutMs)
		{
			if (await Task.WhenAny(task, Task.Delay(ms)) != task)
			{
				throw new TimeoutException("The task has not completed within the allotted time.");
			}
			return await task;
		}

		public static async Task Complete(Task task, int ms = DefaultTimeoutMs)
		{
			if (await Task.WhenAny(task, Task.Delay(ms)) != task)
			{
				throw new TimeoutException("The task has not completed within the allotted time.");
			}
			await task;
		}

		public static async Task WaitUntil(Func<bool> condition, int ms = DefaultTimeoutMs)
		{
			var started = Environment.TickCount;
			while (!condition())
			{
				if (Environment.TickCount - started > ms)
				{
					throw new TimeoutException("The condition has not been satisfied within the allotted time.");
				}
				await Task.Delay(10);
			}
		}

		public static RetrySession CreateSession(
			Func<CancellationToken, Task> func,
			RetryCountInfo? retryCountInfo = null,
			RetryDelay retryDelay = null,
			bool linkCancellationToken = false,
			CancellationToken token = default,
			DefaultRetryProcessor processor = null,
			bool configureAwait = false)
		{
			return RetrySession.Create(
				func,
				processor ?? new DefaultRetryProcessor(),
				retryCountInfo ?? RetryCountInfo.Limited(1),
				retryDelay,
				DefaultRetryProcessor.RetryErrorContextCreator,
				configureAwait,
				token,
				linkCancellationToken);
		}
	}

	internal class RecordingDelayProvider : IDelayProvider
	{
		private readonly List<string> _trace;

		public RecordingDelayProvider(List<string> trace)
		{
			_trace = trace;
		}

		public void Backoff(TimeSpan delay, CancellationToken cancellationToken = default)
		{
			_trace.Add($"delay:{delay.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
		}

		public Task BackoffAsync(TimeSpan delay, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			_trace.Add($"delay:{delay.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
			return Task.CompletedTask;
		}
	}

	internal class RetrySessionTests
	{
		[Test]
		public async Task Should_Session_StepAsync_FirstAttemptSuccess_ReturnsTerminal_OkResult()
		{
			var calls = 0;
			var session = DeferredRetryTestHelpers.CreateSession(async _ =>
			{
				calls++;
				await Task.Delay(1);
			});

			var outcome = await DeferredRetryTestHelpers.Complete(session.StepAsync(false, session.Token));

			ClassicAssert.IsTrue(outcome.IsTerminal);
			ClassicAssert.AreEqual(1, calls);
			ClassicAssert.IsTrue(session.Result.NoError);
			ClassicAssert.IsTrue(session.Result.IsSuccess);
			ClassicAssert.IsFalse(session.Result.IsPolicySuccess);
		}

		[Test]
		public async Task Should_Session_StepAsync_TransientError_ReturnsWaitAndContinue_ThenSucceedsOnRetry()
		{
			var calls = 0;
			var retryDelay = new ConstantRetryDelay(TimeSpan.FromMilliseconds(37));
			var session = DeferredRetryTestHelpers.CreateSession(
				async _ =>
				{
					calls++;
					await Task.Delay(1);
					if (calls == 1)
					{
						throw new InvalidOperationException("boom");
					}
				},
				RetryCountInfo.Limited(3),
				retryDelay);

			var firstOutcome = await DeferredRetryTestHelpers.Complete(session.StepAsync(false, session.Token));

			ClassicAssert.IsFalse(firstOutcome.IsTerminal);
			ClassicAssert.IsNotNull(firstOutcome.LastError);
			ClassicAssert.AreEqual("boom", firstOutcome.LastError.Message);
			ClassicAssert.IsNotNull(firstOutcome.Delay);
			ClassicAssert.AreEqual(TimeSpan.FromMilliseconds(37), firstOutcome.Delay.Value);
			ClassicAssert.IsFalse(session.Result.IsFailed);
			ClassicAssert.AreEqual(1, session.Result.Errors.Count());

			session.IncrementCount();

			var secondOutcome = await DeferredRetryTestHelpers.Complete(session.StepAsync(false, session.Token));

			ClassicAssert.IsTrue(secondOutcome.IsTerminal);
			ClassicAssert.AreEqual(2, calls);
			ClassicAssert.IsTrue(session.Result.IsSuccess);
			ClassicAssert.IsFalse(session.Result.NoError);
			ClassicAssert.IsTrue(session.Result.IsPolicySuccess);
			ClassicAssert.AreEqual(1, session.Result.Errors.Count());
		}

		[Test]
		public async Task Should_Driver_RetryTransientError_ThenSucceed()
		{
			var processor = new DefaultRetryProcessor();
			var calls = 0;

			async Task func(CancellationToken _)
			{
				calls++;
				await Task.Delay(1);
				if (calls == 1)
				{
					throw new InvalidOperationException("boom");
				}
			}

			var result = await DeferredRetryTestHelpers.Complete(processor.RetryAsync(func, RetryCountInfo.Limited(2)));

			ClassicAssert.AreEqual(2, calls);
			ClassicAssert.IsTrue(result.IsPolicySuccess);
			ClassicAssert.IsFalse(result.NoError);
			ClassicAssert.IsTrue(result.IsSuccess);
			ClassicAssert.AreEqual(1, result.Errors.Count());
		}

		[Test]
		public async Task Should_Driver_TerminalFailure_StoreOneMoreErrorThanRetryCount()
		{
			var processor = new DefaultRetryProcessor();
			var calls = 0;

			async Task func(CancellationToken _)
			{
				calls++;
				await Task.Delay(1);
				throw new InvalidOperationException("always fails");
			}

			var result = await DeferredRetryTestHelpers.Complete(processor.RetryAsync(func, RetryCountInfo.Limited(2)));

			ClassicAssert.AreEqual(3, calls);
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.IsFalse(result.IsSuccess);
			ClassicAssert.AreEqual(3, result.Errors.Count());
		}

		[Test]
		public async Task Should_Session_ErrorFilterUnsatisfied_EndSessionAsFailed()
		{
			var processor = new DefaultRetryProcessor();
			processor.ExcludeError((exc) => exc.Message == "no filter for me");

			var calls = 0;
			var session = DeferredRetryTestHelpers.CreateSession(
				_ =>
				{
					calls++;
					throw new Exception("no filter for me");
				},
				RetryCountInfo.Limited(3),
				processor: processor);

			var outcome = await DeferredRetryTestHelpers.Complete(session.StepAsync(false, session.Token));

			ClassicAssert.IsTrue(outcome.IsTerminal);
			ClassicAssert.AreEqual(1, calls);
			ClassicAssert.IsTrue(session.Result.IsFailed);
			ClassicAssert.IsTrue(session.Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(1, session.Result.Errors.Count());
		}

		[Test]
		public async Task Should_Session_CanceledEarly_CelebrateNotRunningTheDelegate()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var calls = 0;
				var session = DeferredRetryTestHelpers.CreateSession(
					_ =>
					{
						calls++;
						return Task.CompletedTask;
					},
					token: cts.Token);

				var outcome = await DeferredRetryTestHelpers.Complete(session.StepAsync(false, session.Token));

				ClassicAssert.IsTrue(outcome.IsTerminal);
				ClassicAssert.AreEqual(0, calls);
				ClassicAssert.IsTrue(session.Result.IsCanceled);
				ClassicAssert.IsTrue(session.Result.NoError);
				ClassicAssert.IsFalse(session.Result.IsFailed);
			}
		}

		[Test]
		public async Task Should_Driver_AppliesRetryDelay_BetweenAttempts_InOrder_WithoutRealSleeps()
		{
			var trace = new List<string>();
			var processor = new DefaultRetryProcessor(new RecordingDelayProvider(trace));
			var calls = 0;

			async Task func(CancellationToken _)
			{
				calls++;
				trace.Add($"attempt{calls}");
				await Task.Delay(1);
				if (calls < 3)
				{
					throw new InvalidOperationException("boom");
				}
			}

			var result = await DeferredRetryTestHelpers.Complete(
				processor.RetryAsync(func, RetryCountInfo.Limited(5), new RetryDelay(_ => TimeSpan.FromSeconds(3))));

			ClassicAssert.AreEqual(3, calls);
			ClassicAssert.IsTrue(result.IsPolicySuccess);
			ClassicAssert.AreEqual(
				new[] { "attempt1", "delay:3", "attempt2", "delay:3", "attempt3" },
				trace);
		}

		[Test]
		public async Task Should_SessionT_StepAsync_FirstAttemptSuccess_ProducesResultValue()
		{
			var calls = 0;
			var session = RetrySession<int>.Create(
				async _ =>
				{
					calls++;
					await Task.Delay(1);
					return 42;
				},
				new DefaultRetryProcessor(),
				RetryCountInfo.Limited(2),
				null,
				DefaultRetryProcessor.RetryErrorContextCreator,
				false,
				default);

			var outcome = await DeferredRetryTestHelpers.Complete(session.StepAsync(false, session.Token));

			ClassicAssert.IsTrue(outcome.IsTerminal);
			ClassicAssert.AreEqual(1, calls);
			ClassicAssert.AreEqual(42, session.Result.Result);
			ClassicAssert.IsTrue(session.Result.NoError);
			ClassicAssert.IsTrue(session.Result.IsSuccess);
		}

		[Test]
		public async Task Should_DriverT_RetryTransientError_ThenSucceedWithResultValue()
		{
			var processor = new DefaultRetryProcessor();
			var calls = 0;

			async Task<int> func(CancellationToken _)
			{
				calls++;
				await Task.Delay(1);
				if (calls == 1)
				{
					throw new InvalidOperationException("boom");
				}
				return 7;
			}

			var result = await DeferredRetryTestHelpers.Complete(processor.RetryAsync<int>(func, RetryCountInfo.Limited(2)));

			ClassicAssert.AreEqual(2, calls);
			ClassicAssert.AreEqual(7, result.Result);
			ClassicAssert.IsTrue(result.IsPolicySuccess);
			ClassicAssert.AreEqual(1, result.Errors.Count());
		}
	}
}