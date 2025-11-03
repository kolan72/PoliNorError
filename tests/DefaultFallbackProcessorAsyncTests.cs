using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.BulkErrorProcessor;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class DefaultFallbackProcessorAsyncTests
	{
		[Test]
		public async Task Should_FallbackAsync_CallFallback()
		{
			var processor = FallbackProcessor.CreateDefault();

			int i = 1;

			async Task fallback(CancellationToken _) { await Task.Delay(1); i++; }

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, fallback);

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(1, res.Errors.Count());
		}

		[Test]
		public async Task Should_FallbackAsync_Call_Error_Func_When_Error()
		{
			int i = 1;
			async Task onErrorTask(Exception _, CancellationToken __) { await Task.FromResult(i++); }

			var processor = FallbackProcessor
										.CreateDefault().WithErrorProcessorOf(onErrorTask);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => await Task.Delay(1));

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(1, res.Errors.Count());
		}

		[Test]
		public async Task Should_FallbackAsync_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }

			var processor = FallbackProcessor.CreateDefault().WithErrorProcessorOf(save);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => await Task.Delay(1));

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(1, res.Errors.Count());
		}

		[Test]
		public async Task Should_FallbackAsyncT_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }

			var processor = FallbackProcessor.CreateDefault().WithErrorProcessorOf(save);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => { await Task.Delay(1); return 1; });

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(1, res.Errors.Count());
			ClassicAssert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Call_Error_Func_When_Error()
		{
			int i = 1;
			async Task onErrorTask(Exception _, CancellationToken __) { await Task.FromResult(i++); }

			var processor = FallbackProcessor.CreateDefault().WithErrorProcessorOf(onErrorTask);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => { await Task.Delay(1); return 1; });

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
			ClassicAssert.AreEqual(1, res.Errors.Count());
			ClassicAssert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Returns_FallbackValue_If_NotError()
		{
			var processor = FallbackProcessor.CreateDefault();

			int i = 1;

			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); return ++i; }

			var res = await processor.FallbackAsync<int>(async (_) => { await Task.Delay(1); throw new Exception(); }, fallback);

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Returns_SuccessValue_If_NotError()
		{
			var processor = FallbackProcessor.CreateDefault();

			int i = 1;

			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); return ++i; }

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); return i; }, fallback);

			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(false, res.IsFailed);
		}

		[Test]
		public async Task Should_FallbackAsync_Result_IsFailed_Equals_True_When_Error_In_CatchBlockProcessing()
		{
			var processor = FallbackProcessor.CreateDefault();
			var polResult = await processor.FallbackAsync((_) => throw new Exception("Test_Save"), (_) => throw new Exception("Test_Fallback"));

			ClassicAssert.AreEqual(true, polResult.IsFailed);
			ClassicAssert.AreEqual(true, polResult.CatchBlockErrors.FirstOrDefault().IsCritical);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Result_IsFailed_Equals_True_When_Error_In_CatchBlockProcessing()
		{
			var processor = FallbackProcessor.CreateDefault();
			var polResult = await processor.FallbackAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test_Save"); }, async (_) => { await Task.Delay(1); throw new Exception("Test_Fallback");});

			ClassicAssert.AreEqual(true, polResult.IsFailed);
			ClassicAssert.AreEqual(true, polResult.CatchBlockErrors.FirstOrDefault().IsCritical);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
		}

		[Test]
		public async Task Should_FallbackAsync_Break_When_ErrorProcessing_Canceled()
		{
			var throwingExc = new ApplicationException();
			int i = 0;
			async Task save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }

			var mockedBulkProcessor = Substitute.For<IBulkErrorProcessor>();

			mockedBulkProcessor.ProcessAsync(throwingExc, Arg.Any<ProcessingErrorContext>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Canceled) })));

			async Task fallback(CancellationToken _) { await Task.Delay(1); i++; }

			var processor = new DefaultFallbackProcessor(mockedBulkProcessor);

			var tryResCount = await processor.FallbackAsync(save, fallback);

			ClassicAssert.AreEqual(true, tryResCount.IsFailed);
			ClassicAssert.AreEqual(true, tryResCount.IsCanceled);
			ClassicAssert.AreEqual(0, i);
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackAsync_Null_Delegate()
		{
			var proc = FallbackProcessor.CreateDefault();
			var fallbackResult = await proc.FallbackAsync(null, async (_) => await Task.Delay(1));
			ClassicAssert.IsTrue(fallbackResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, fallbackResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), fallbackResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackTAsync_Null_Delegate()
		{
			var proc = FallbackProcessor.CreateDefault();
			var fallbackResult = await proc.FallbackAsync(null, async (_) => { await Task.Delay(1); return 1; });
			ClassicAssert.IsTrue(fallbackResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, fallbackResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), fallbackResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(false, true)]
		public async Task Should_WithInnerErrorProcessor_HandleError_Correctly(bool sync, bool withCancellationType)
		{
			var processor = FallbackProcessor.CreateDefault();
			var innerProcessors = new InnerErrorProcessorFuncs();

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action);
				}

				processor.Fallback(ActionWithInner, (_) => {});
				processor.Fallback(Action, (_) => {});
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc);
				}

				await processor.FallbackAsync(AsyncFuncWithInner, async (_) => await Task.Delay(1));
				await processor.FallbackAsync(AsyncFunc, async (_) => await Task.Delay(1));
			}

			Assert.That(innerProcessors.I, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken);
				processor.Fallback(ActionWithInner, (_) => { });
				processor.Fallback(Action, (_) => { });
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken);
				await processor.FallbackAsync(AsyncFuncWithInner, async (_) => await Task.Delay(1));
				await processor.FallbackAsync(AsyncFunc, async (_) => await Task.Delay(1));
			}

			Assert.That(innerProcessors.J, Is.EqualTo(1));

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo);
				}
				processor.Fallback(ActionWithInner, (_) => { });
				processor.Fallback(Action, (_) => { });
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo);
				}
				await processor.FallbackAsync(AsyncFuncWithInner, async (_) => await Task.Delay(1));
				await processor.FallbackAsync(AsyncFunc, async (_) => await Task.Delay(1));
			}

			Assert.That(innerProcessors.K, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken);
				processor.Fallback(ActionWithInner, (_) => { });
				processor.Fallback(Action, (_) => { });
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken);
				await processor.FallbackAsync(AsyncFuncWithInner, async (_) => await Task.Delay(1));
				await processor.FallbackAsync(AsyncFunc, async (_) => await Task.Delay(1));
			}

			Assert.That(innerProcessors.L, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackAsync_With_TParam_For_NonGeneric_AsyncFunc_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new DefaultFallbackProcessor()
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, async (_) => await Task.Delay(1));
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.FallbackAsync(async (_) => await Task.Delay(1), 5, async (_) => await Task.Delay(1));
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackAsync_With_TParam_For_NonGeneric_AsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new DefaultFallbackProcessor()
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = await processor.FallbackAsync(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, async (_) => await Task.Delay(1));
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.FallbackAsync(async (v, _) => { await Task.Delay(1); addable += v; }, 5, async (_) => await Task.Delay(1));
				Assert.That(m, Is.EqualTo(0));
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackAsync_With_TParam_For_Generic_AsyncFunc_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new DefaultFallbackProcessor()
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, async(_) => { await Task.Delay(1); return 10;});
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
				Assert.That(result.Result, Is.EqualTo(10));
			}
			else
			{
				result = await processor.FallbackAsync(async (_) => { await Task.Delay(1); return 1; }, 5, async (_) => { await Task.Delay(1); return 10;});
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
				Assert.That(result.Result, Is.EqualTo(1));
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_FallbackAsync_With_TParam_For_Generic_AsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new DefaultFallbackProcessor()
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = await processor.FallbackAsync(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5, async (_) => { await Task.Delay(1); return 10;});
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.FallbackAsync(async (v, _) => { await Task.Delay(1); addable += v; return addable; }, 5, async (_) => { await Task.Delay(1); return 10; });
				Assert.That(m, Is.EqualTo(0));
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.Result, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public async Task Should_FallbackAsync_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

				async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
				int i = 0;
				var processor = FallbackProcessor.CreateDefault();
				async Task fallback(CancellationToken _) { await Task.Delay(1); i++; }
				var result = await processor.FallbackAsync(save, fallback, cancelTokenSource.Token);

				ClassicAssert.AreEqual(true, result.IsCanceled);
				ClassicAssert.AreEqual(0, i);

				Assert.That(result.NoError, Is.True);
			}
		}

		[Test]
		public async Task Should_FallbackAsyncT_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
			int i = 0;
			var processor = FallbackProcessor.CreateDefault();
			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); i++; return i; }
			var tryResCount = await processor.FallbackAsync(save, fallback, cancelTokenSource.Token);

			ClassicAssert.AreEqual(true, tryResCount.IsCanceled);
			ClassicAssert.AreEqual(0, i);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_FallbackAsync_With_Param_With_ConfigAwait_False_BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(int i, CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => k = i, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultFallbackProcessor();
				var result = await processor.FallbackAsync(f, 1, (_) => Task.CompletedTask, token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_FallbackAsync_With_Context_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => k = 1, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new DefaultFallbackProcessor();
				var result = await processor.FallbackAsync(f, 1, (_) => Task.CompletedTask, token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_FallbackAsyncT_WithParam_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

#pragma warning disable RCS1163 // Unused parameter.
				async Task<int> save(int k, CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
#pragma warning restore RCS1163 // Unused parameter.
				const int i = 0;
				var processor = new DefaultFallbackProcessor();
				var tryResCount = await processor.FallbackAsync(save, 1, async (_) => { await Task.Delay(1); return 1; },  false, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);

				Assert.That(i, Is.EqualTo(0));
			}
		}
	}
}